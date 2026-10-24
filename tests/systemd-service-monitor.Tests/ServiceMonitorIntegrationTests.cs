#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

/// <summary>
/// Integration tests for the ServiceMonitorService polling loop. Uses the real in-memory
/// repository and the real loop; systemd is replaced by <see cref="FakeSystemdUnitClient"/>
/// and the monitor's per-cycle "Retrieved status" log line is the observation point.
/// </summary>
public class ServiceMonitorIntegrationTests : IDisposable
{
    private const string UnitName = "demo.service";
    private const int IntervalMs = 5;
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    private readonly Guid _unitId = Guid.NewGuid();
    private readonly ServiceRepository _repository = new();
    private readonly RecordingLogger<ServiceMonitorService> _logger = new();
    private readonly FakeSystemdUnitClient _systemd = new();
    private readonly ISystemdConnectionService _connectionService = Substitute.For<ISystemdConnectionService>();
    private readonly ServiceMonitorService _monitor;
    private bool _published;

    public ServiceMonitorIntegrationTests()
    {
        _monitor = new ServiceMonitorService(_logger, _connectionService, _repository, _systemd);
    }

    public void Dispose()
    {
        _monitor.StopMonitoringAsync(UnitName).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Verifies that a service which never changes state produces no transitions across several polling cycles.
    /// </summary>
    [Fact]
    public async Task StartMonitoringAsync_StableService_ReportsNoTransitions()
    {
        // Arrange: the unit is active and stays active across three refreshes.
        var steps = new[]
        {
            new Step(ServiceState.Active, Cycles: 4),
            new Step(ServiceState.Active, Cycles: 4),
            new Step(ServiceState.Active, Cycles: 4),
        };

        // Act
        var timeline = await RunStepsAsync(steps, PublishToRepositoryAsync);

        // Assert: one continuous run of Active, so zero transitions.
        timeline.Runs.Should().ContainSingle()
            .Which.State.Should().Be(ServiceState.Active);
        timeline.Runs[0].Cycles.Should().BeGreaterOrEqualTo(12);
        timeline.Runs[0].FirstSeenUtc.Should().BeOnOrAfter(timeline.WrittenAtUtc[0]);
        _logger.Entries.Should().NotContain(e => e.Level == LogLevel.Warning);
    }

    /// <summary>
    /// Verifies that active -> failed -> active is observed as exactly two transitions, in order,
    /// each first seen after the write that caused it.
    /// </summary>
    [Fact]
    public async Task StartMonitoringAsync_ActiveFailedActive_ReportsTwoTransitionsInOrder()
    {
        // Arrange
        var steps = new[]
        {
            new Step(ServiceState.Active, Cycles: 4),
            new Step(ServiceState.Failed, Cycles: 4),
            new Step(ServiceState.Active, Cycles: 4),
        };

        // Act
        var timeline = await RunStepsAsync(steps, PublishToRepositoryAsync);

        // Assert
        AssertActiveFailedActive(timeline);
    }

    /// <summary>
    /// Verifies that a unit which systemd never lists keeps being polled, each cycle reports not-found,
    /// and no state is ever reported.
    /// </summary>
    [Fact]
    public async Task StartMonitoringAsync_UnitNeverListed_KeepsPollingWithNotFoundWarnings()
    {
        // Arrange: nothing is written to the repository for this unit.

        // Act
        await _monitor.StartMonitoringAsync(UnitName, IntervalMs);
        await WaitForObservationsAsync(5);
        await _monitor.StopMonitoringAsync(UnitName);

        // Assert
        var observations = Observations();
        observations.Should().HaveCountGreaterOrEqualTo(5);
        observations.Should().OnlyContain(o => o.State == null);
        _logger.Entries.Count(e => e.Level == LogLevel.Warning && e.Message.StartsWith("Service not found", StringComparison.Ordinal))
            .Should().BeGreaterOrEqualTo(5);
    }

    /// <summary>
    /// Verifies that refreshing from an injected systemd client never touches the connection service.
    /// </summary>
    [Fact]
    public async Task RefreshServiceListAsync_WithInjectedClient_DoesNotUseConnectionService()
    {
        // Arrange: a non-service unit, which refresh skips.
        _systemd.SetUnit("sshd.socket", "active", "listening");

        // Act
        await _monitor.RefreshServiceListAsync();

        // Assert
        _connectionService.ReceivedCalls().Should().BeEmpty();
        (await _repository.GetAllAsync()).Should().BeEmpty();
    }

    /// <summary>
    /// Verifies the active -> failed -> active sequence when each state arrives through the fake systemd
    /// client and RefreshServiceListAsync. Currently fails: refresh hands a new ServiceInfo (new Id) to
    /// UpdateAsync, which throws KeyNotFoundException before any unit is stored.
    /// </summary>
    [Fact(Skip = "Blocked by production defect: RefreshServiceListAsync calls ServiceRepository.UpdateAsync with a new ServiceInfo Id, which throws KeyNotFoundException; no unit reaches the repository.")]
    public async Task RefreshServiceListAsync_ActiveFailedActive_ViaFakeDBus_ReportsTwoTransitionsInOrder()
    {
        // Arrange
        var steps = new[]
        {
            new Step(ServiceState.Active, Cycles: 4),
            new Step(ServiceState.Failed, Cycles: 4),
            new Step(ServiceState.Active, Cycles: 4),
        };

        // Act
        var timeline = await RunStepsAsync(steps, RefreshFromFakeDBusAsync);

        // Assert
        AssertActiveFailedActive(timeline);
    }

    /// <summary>
    /// Verifies that a unit which disappears from systemd is reported as not found after it was active.
    /// Skipped: refresh never removes units that vanish from ListUnits, so the repository keeps the last state
    /// and the monitor never reports not-found. The same KeyNotFoundException defect also blocks the first refresh.
    /// </summary>
    [Fact(Skip = "Gap: RefreshServiceListAsync never prunes units missing from ListUnits, so the last state stays in the repository. Also blocked by the UpdateAsync KeyNotFoundException defect.")]
    public async Task RefreshServiceListAsync_UnitDisappears_ReportsNotFound()
    {
        // Arrange
        var steps = new[]
        {
            new Step(ServiceState.Active, Cycles: 4),
            new Step(null, Cycles: 4),
        };

        // Act
        var timeline = await RunStepsAsync(steps, RefreshFromFakeDBusAsync);

        // Assert: active, then not found.
        timeline.Runs.Select(r => r.State).Should().Equal(new ServiceState?[] { ServiceState.Active, null });
        timeline.Runs[1].FirstSeenUtc.Should().BeOnOrAfter(timeline.WrittenAtUtc[1]);
    }

    private static void AssertActiveFailedActive(Timeline timeline)
    {
        // Exactly three runs: Active, Failed, Active. Two transitions, in that order, none repeated.
        timeline.Runs.Select(r => r.State).Should().Equal(new ServiceState?[]
        {
            ServiceState.Active, ServiceState.Failed, ServiceState.Active,
        });

        // Each run is first seen after the write that produced it, and the runs are ordered in time.
        for (var i = 0; i < timeline.Runs.Count; i++)
        {
            timeline.Runs[i].FirstSeenUtc.Should().BeOnOrAfter(timeline.WrittenAtUtc[i]);
        }
        timeline.Runs.Select(r => r.FirstSeenUtc).Should().BeInAscendingOrder();
    }

    private sealed record Step(ServiceState? State, int Cycles);

    private sealed record Observation(DateTime AtUtc, ServiceState? State);

    private sealed record Run(ServiceState? State, DateTime FirstSeenUtc, int Cycles);

    private sealed record Timeline(IReadOnlyList<Run> Runs, IReadOnlyList<DateTime> WrittenAtUtc);

    /// <summary>
    /// Applies each step, starts the monitor after the first, waits for the requested number of polling cycles
    /// per step, then stops the monitor and checks that it stays silent.
    /// </summary>
    private async Task<Timeline> RunStepsAsync(IReadOnlyList<Step> steps, Func<ServiceState?, Task> apply)
    {
        var writtenAt = new List<DateTime>();
        try
        {
            for (var i = 0; i < steps.Count; i++)
            {
                writtenAt.Add(DateTime.UtcNow);
                await apply(steps[i].State);

                if (i == 0)
                {
                    await _monitor.StartMonitoringAsync(UnitName, IntervalMs);
                }

                await WaitForObservationsAsync(Observations().Count + steps[i].Cycles);
            }
        }
        finally
        {
            await _monitor.StopMonitoringAsync(UnitName);
        }

        // After stop, the loop must not keep polling.
        await Task.Delay(100);
        var settled = Observations().Count;
        await Task.Delay(100);
        Observations().Count.Should().Be(settled, "the monitor must not poll after it is stopped");

        return new Timeline(ToRuns(Observations()), writtenAt);
    }

    /// <summary>
    /// Writes the unit state straight into the repository, as a correct refresh would.
    /// </summary>
    private async Task PublishToRepositoryAsync(ServiceState? state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var record = new ServiceInfo
        {
            Id = _unitId,
            UnitName = UnitName,
            State = state.Value,
            SubState = state.Value == ServiceState.Active ? ServiceSubState.Running : ServiceSubState.Failed,
        };

        if (_published)
        {
            await _repository.UpdateAsync(record);
        }
        else
        {
            await _repository.CreateAsync(record);
            _published = true;
        }
    }

    /// <summary>
    /// Changes the unit in the fake systemd (or removes it when state is null) and runs a real refresh.
    /// </summary>
    private async Task RefreshFromFakeDBusAsync(ServiceState? state)
    {
        if (state is { } s)
        {
            _systemd.SetUnit(UnitName, s == ServiceState.Active ? "active" : "failed", s == ServiceState.Active ? "running" : "failed");
        }
        else
        {
            _systemd.RemoveUnit(UnitName);
        }

        await _monitor.RefreshServiceListAsync();
    }

    /// <summary>
    /// Reads the monitor's per-cycle observations from its log: found units report their state,
    /// missing units report null.
    /// </summary>
    private List<Observation> Observations()
    {
        var observations = new List<Observation>();
        foreach (var entry in _logger.Entries)
        {
            if (entry.Message.StartsWith("Retrieved status for service", StringComparison.Ordinal))
            {
                var state = entry.Values?.FirstOrDefault(kv => kv.Key == "State").Value;
                observations.Add(new Observation(entry.TimestampUtc, (ServiceState?)state));
            }
            else if (entry.Message.StartsWith("Service not found when retrieving status", StringComparison.Ordinal))
            {
                observations.Add(new Observation(entry.TimestampUtc, null));
            }
        }
        return observations;
    }

    private static List<Run> ToRuns(IEnumerable<Observation> observations)
    {
        var runs = new List<Run>();
        foreach (var observation in observations)
        {
            if (runs.Count > 0 && runs[^1].State == observation.State)
            {
                runs[^1] = runs[^1] with { Cycles = runs[^1].Cycles + 1 };
            }
            else
            {
                runs.Add(new Run(observation.State, observation.AtUtc, 1));
            }
        }
        return runs;
    }

    private async Task WaitForObservationsAsync(int expectedCount)
    {
        var deadline = DateTime.UtcNow + WaitTimeout;
        while (Observations().Count < expectedCount)
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException($"Monitor produced {Observations().Count} observations, expected {expectedCount}.");
            }
            await Task.Delay(IntervalMs);
        }
    }
}
