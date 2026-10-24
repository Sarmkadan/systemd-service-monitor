#nullable enable

using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

/// <summary>
/// End-to-end tests for <see cref="AlertRulesEngine"/>. A rules file is loaded from JSON, service states
/// are published into the real in-memory repository, read back through the real
/// <see cref="ServiceMonitorService"/>, and fed into <see cref="IAlertRulesEngine.EvaluateServiceAsync"/>
/// one poll at a time. A <see cref="ManualTimeProvider"/> drives the engine clock, so cooldowns and
/// incident timestamps are asserted exactly.
/// </summary>
/// <remarks>
/// The poll cadence matches the <see cref="ServiceMonitorService"/> default of 5 seconds. The engine counts
/// consecutive matching evaluations rather than measuring elapsed time, so "failed for 30 seconds" is expressed
/// as seven consecutive failed samples at that cadence.
/// </remarks>
public class AlertRulesEngineIntegrationTests
{
    private const string NginxUnit = "nginx.service";
    private const string NginxDownRule = "nginx down for 30s";
    private const string NginxMemoryRule = "nginx memory high";
    private const string HostCpuRule = "host CPU saturated";
    private const string PostgresDownRule = "postgres failed";

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly DateTimeOffset StartTime = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Realistic rules file. Durations are expressed as consecutive samples at the 5 s poll cadence and
    /// cooldowns are five minutes, so the fake clock can cross them in a few hundred simulated seconds.
    /// </summary>
    private const string RulesJson = """
    [
      {
        "Name": "nginx down for 30s",
        "Description": "Nginx failed for 30 seconds (seven 5-second samples)",
        "ServicePattern": "nginx.service",
        "Condition": "ServiceFailed",
        "Severity": "Critical",
        "ConsecutiveEvaluationsRequired": 7,
        "CooldownMinutes": 5
      },
      {
        "Name": "nginx memory high",
        "Description": "Nginx resident memory above 512 MB",
        "ServicePattern": "nginx*",
        "Condition": "MemoryThresholdExceeded",
        "Threshold": 512,
        "Severity": "High",
        "CooldownMinutes": 5
      },
      {
        "Name": "host CPU saturated",
        "Description": "Any service above 90% CPU",
        "ServicePattern": "*",
        "Condition": "CpuThresholdExceeded",
        "Threshold": 90,
        "Severity": "Medium",
        "CooldownMinutes": 5
      },
      {
        "Name": "postgres failed",
        "Description": "Must not match nginx; present to prove pattern filtering",
        "ServicePattern": "postgresql*",
        "Condition": "ServiceFailed",
        "Severity": "Critical",
        "CooldownMinutes": 5
      }
    ]
    """;

    private static readonly JsonSerializerOptions RulesJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    // -------------------------------------------------------------------------
    // Boundary: consecutive-failure threshold
    // -------------------------------------------------------------------------

    /// <summary>
    /// Six failed samples span 25 seconds, one short of the seven-sample threshold, so no incident opens.
    /// </summary>
    [Fact]
    public async Task NginxFailed_ForSixSamples_ShouldNotOpenIncident()
    {
        // Arrange: nginx is failed at t = 0, 5, ..., 25 (six samples) and recovers at t = 30.
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 40, t => t < 30 ? Down() : Up());

        // Assert
        snapshots.Should().OnlyContain(s => s.Incidents.Count == 0);
    }

    /// <summary>
    /// The seventh consecutive failed sample (t = 30 s) is the first to meet the threshold. The incident opens
    /// at exactly that tick, at Critical severity, and is still open on the next poll.
    /// </summary>
    [Fact]
    public async Task NginxFailed_ForSevenSamples_ShouldOpenCriticalIncidentAtThirtySeconds()
    {
        // Arrange: nginx is failed at t = 0, 5, ..., 30 (seven samples) and stays failed until t = 35.
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 35, t => t <= 30 ? Down() : Up());

        // Assert: nothing before the boundary, exactly one incident at the boundary.
        snapshots.Single(s => s.Second == 25).Incidents.Should().BeEmpty();

        var opened = snapshots.Single(s => s.Second == 30).Incidents.Should().ContainSingle().Subject;
        opened.Rule.Should().Be(NginxDownRule);
        opened.OpenedAt.Should().Be(30);
        opened.Severity.Should().Be(AlertSeverity.Critical);
        opened.State.Should().Be(AlertIncidentState.Open);
    }

    // -------------------------------------------------------------------------
    // Recovery
    // -------------------------------------------------------------------------

    /// <summary>
    /// When nginx comes back, the incident auto-resolves on the first healthy sample. It is not left open
    /// and does not reopen while the service stays healthy.
    /// </summary>
    [Fact]
    public async Task NginxRecovers_ShouldAutoResolveIncidentAtFirstHealthySample()
    {
        // Arrange: incident opens at t = 30; nginx is healthy from t = 35 onward.
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 50, t => t <= 30 ? Down() : Up());

        // Assert
        snapshots.Single(s => s.Second == 30).Incidents.Should().ContainSingle()
            .Which.State.Should().Be(AlertIncidentState.Open);

        snapshots.Single(s => s.Second == 35).Incidents.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new IncidentView(
                NginxDownRule, OpenedAt: 30, AlertSeverity.Critical, AlertIncidentState.AutoResolved, ResolvedAt: 35));

        snapshots.Last().Incidents.Should().OnlyContain(i => i.State == AlertIncidentState.AutoResolved);
    }

    // -------------------------------------------------------------------------
    // Cooldown
    // -------------------------------------------------------------------------

    /// <summary>
    /// After an incident opens, a rule must not open another for the same service while the cooldown is
    /// running, even if the condition clears and returns. Here nginx fails, recovers, and fails again
    /// continuously. The re-fire is suppressed until the five-minute cooldown has elapsed.
    /// </summary>
    /// <remarks>
    /// The consecutive-failure counter resets on every cooldown-suppressed attempt, so after the cooldown ends
    /// the rule needs seven fresh failed samples before it fires again (t = 350 here, not t = 330).
    /// </remarks>
    [Fact]
    public async Task NginxFailsAgainWithinCooldown_ShouldNotRefireUntilCooldownElapses()
    {
        // Arrange: first failure t = 0..30 (incident at 30), recovery t = 35..39, then failed from t = 40 to 400.
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 400, t => t <= 30 || (t >= 40 && t <= 400) ? Down() : Up());

        // Assert: suppressed attempts at 70, 105, ..., 315 (all inside the 300 s cooldown from t = 30) open nothing.
        foreach (var second in new[] { 70, 280, 315 })
        {
            snapshots.Single(s => s.Second == second).Incidents
                .Count(i => i.Rule == NginxDownRule).Should().Be(1, $"cooldown still runs at t={second}");
        }

        var openedAt = snapshots.Last().Incidents
            .Where(i => i.Rule == NginxDownRule)
            .Select(i => i.OpenedAt)
            .OrderBy(t => t)
            .ToList();

        openedAt.Should().Equal(30, 350);
    }

    /// <summary>
    /// With a single-evaluation rule, a continuous breach re-fires on the exact tick where the cooldown
    /// window closes: one tick earlier is still suppressed.
    /// </summary>
    [Fact]
    public async Task MemoryBreachContinues_ShouldRefireExactlyWhenCooldownElapses()
    {
        // Arrange: nginx uses 600 MB from t = 0 through t = 300 (cooldown is 5 minutes).
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 300, _ => Up(memoryMb: 600));

        // Assert
        snapshots.Single(s => s.Second == 295).Incidents.Should().ContainSingle(i => i.Rule == NginxMemoryRule);

        var memoryOpenedAt = snapshots.Last().Incidents
            .Where(i => i.Rule == NginxMemoryRule)
            .Select(i => i.OpenedAt)
            .OrderBy(t => t)
            .ToList();

        memoryOpenedAt.Should().Equal(0, 300);
    }

    // -------------------------------------------------------------------------
    // Threshold boundary for value-based conditions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Thresholds are strict. CPU at exactly 90% and memory at exactly 512 MB do not open incidents. One unit
    /// above both opens both on the same poll.
    /// </summary>
    [Fact]
    public async Task CpuAndMemoryAtExactThreshold_ShouldNotOpen_AndOneUnitAboveShouldOpenBoth()
    {
        // Arrange: at-threshold values for t = 0..10, then CPU 90.5% and memory 513 MB from t = 15.
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 15, t => t < 15
            ? Up(cpu: 90, memoryMb: 512)
            : Up(cpu: 90.5, memoryMb: 513));

        // Assert
        snapshots.Single(s => s.Second == 10).Incidents.Should().BeEmpty();

        snapshots.Single(s => s.Second == 15).Incidents
            .Should().BeEquivalentTo(new[]
            {
                new IncidentView(HostCpuRule, OpenedAt: 15, AlertSeverity.Medium, AlertIncidentState.Open, ResolvedAt: null),
                new IncidentView(NginxMemoryRule, OpenedAt: 15, AlertSeverity.High, AlertIncidentState.Open, ResolvedAt: null),
            });
    }

    // -------------------------------------------------------------------------
    // Multiple rules matching one service
    // -------------------------------------------------------------------------

    /// <summary>
    /// Three rules match nginx: the exact-name down rule, the wildcard memory rule, and the wildcard "*"
    /// CPU rule. The postgres rule does not match and must stay silent. Each rule opens and resolves on its
    /// own schedule. Clearing one rule's condition does not resolve another rule's incident on the same service.
    /// </summary>
    [Fact]
    public async Task OneServiceMatchingSeveralRules_ShouldTrackEachRuleIndependently()
    {
        // Arrange
        //   t =  0..10: CPU 95% (host CPU rule fires at 0), memory 600 MB until t = 60 (memory rule fires at 0)
        //   t =  0..30: nginx failed (down rule fires at 30 after seven samples)
        //   t = 15:     CPU drops to 10% (host CPU rule clears; memory rule must stay open)
        //   t = 35:     nginx healthy (down rule clears)
        //   t = 65:     memory drops to 100 MB (memory rule clears)
        var rig = await Rig.CreateAsync(LoadRules());

        // Act
        var snapshots = await rig.RunAsync(lastSecond: 70, t => new UnitSample(
            t <= 30 ? ServiceState.Failed : ServiceState.Active,
            t <= 30 ? ServiceSubState.Failed : ServiceSubState.Running,
            CpuPercent: t <= 10 ? 95 : 10,
            MemoryMb: t <= 60 ? 600 : 100));

        // Assert: open set at each checkpoint
        snapshots.Single(s => s.Second == 0).Incidents
            .Select(i => i.Rule).Should().BeEquivalentTo(new[] { HostCpuRule, NginxMemoryRule });

        snapshots.Single(s => s.Second == 15).Incidents
            .Single(i => i.Rule == HostCpuRule).State.Should().Be(AlertIncidentState.AutoResolved);
        snapshots.Single(s => s.Second == 15).Incidents
            .Single(i => i.Rule == NginxMemoryRule).State.Should().Be(AlertIncidentState.Open);

        snapshots.Single(s => s.Second == 30).Incidents
            .Single(i => i.Rule == NginxDownRule).OpenedAt.Should().Be(30);

        snapshots.Single(s => s.Second == 35).Incidents
            .Single(i => i.Rule == NginxDownRule).ResolvedAt.Should().Be(35);

        snapshots.Last().Incidents
            .Single(i => i.Rule == NginxMemoryRule).ResolvedAt.Should().Be(65);

        // Final totals: exactly one incident per triggered rule and none for postgres.
        snapshots.Last().Incidents.Select(i => i.Rule)
            .Should().BeEquivalentTo(new[] { HostCpuRule, NginxMemoryRule, NginxDownRule });
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static List<AlertRule> LoadRules()
        => JsonSerializer.Deserialize<List<AlertRule>>(RulesJson, RulesJsonOptions)
           ?? throw new InvalidOperationException("Rules fixture did not deserialize.");

    private static UnitSample Up(double cpu = 10, long memoryMb = 100)
        => new(ServiceState.Active, ServiceSubState.Running, cpu, memoryMb);

    private static UnitSample Down(double cpu = 10, long memoryMb = 100)
        => new(ServiceState.Failed, ServiceSubState.Failed, cpu, memoryMb);

    /// <summary>One poll's worth of nginx state as published to the repository.</summary>
    private sealed record UnitSample(ServiceState State, ServiceSubState SubState, double CpuPercent, long MemoryMb);

    /// <summary>Snapshot of one incident as seen by the engine at a given poll.</summary>
    private sealed record IncidentView(
        string Rule,
        int OpenedAt,
        AlertSeverity Severity,
        AlertIncidentState State,
        int? ResolvedAt);

    /// <summary>All incidents known to the engine after the poll at <see cref="Second"/>.</summary>
    private sealed record Snapshot(int Second, IReadOnlyList<IncidentView> Incidents);

    /// <summary>
    /// Wires the real repository, <see cref="ServiceMonitorService"/>, and <see cref="AlertRulesEngine"/>
    /// together under one fake clock.
    /// </summary>
    private sealed class Rig
    {
        private readonly Guid _unitId = Guid.NewGuid();
        private readonly ServiceRepository _repository = new();
        private readonly ServiceMonitorService _monitor;
        private readonly Dictionary<Guid, string> _ruleNames = new();
        private bool _published;

        public ManualTimeProvider Clock { get; } = new(StartTime);

        public AlertRulesEngine Engine { get; }

        private Rig()
        {
            _monitor = new ServiceMonitorService(
                NullLogger<ServiceMonitorService>.Instance,
                Substitute.For<ISystemdConnectionService>(),
                _repository);

            Engine = new AlertRulesEngine(
                NullLogger<AlertRulesEngine>.Instance,
                new InMemoryOnCallScheduleService(NullLogger<InMemoryOnCallScheduleService>.Instance),
                Options.Create(new AlertOptions()),
                Substitute.For<IHttpClientFactory>(),
                Clock);
        }

        public static async Task<Rig> CreateAsync(IEnumerable<AlertRule> rules)
        {
            var rig = new Rig();
            foreach (var rule in rules)
            {
                await rig.Engine.AddRuleAsync(rule);
                rig._ruleNames[rule.Id] = rule.Name;
            }

            return rig;
        }

        /// <summary>
        /// Runs one poll per 5 s from t = 0 through <paramref name="lastSecond"/>. Each poll publishes the
        /// scripted state, reads it through <see cref="ServiceMonitorService"/>, evaluates it, and records
        /// what the engine reports.
        /// </summary>
        public async Task<List<Snapshot>> RunAsync(int lastSecond, Func<int, UnitSample> script)
        {
            var snapshots = new List<Snapshot>();

            for (var second = 0; second <= lastSecond; second += (int)PollInterval.TotalSeconds)
            {
                await PublishAndEvaluateAsync(script(second));
                snapshots.Add(new Snapshot(second, await SnapshotAsync()));
                Clock.Advance(PollInterval);
            }

            return snapshots;
        }

        private async Task PublishAndEvaluateAsync(UnitSample sample)
        {
            var record = new ServiceInfo
            {
                Id = _unitId,
                UnitName = NginxUnit,
                State = sample.State,
                SubState = sample.SubState,
                CpuUsagePercent = sample.CpuPercent,
                MemoryUsageMb = sample.MemoryMb,
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

            var status = await _monitor.GetServiceStatusAsync(NginxUnit)
                ?? throw new InvalidOperationException($"{NginxUnit} missing from repository.");

            await Engine.EvaluateServiceAsync(status);
        }

        private async Task<IReadOnlyList<IncidentView>> SnapshotAsync()
        {
            var incidents = await Engine.GetIncidentHistoryAsync(maxResults: 1000);

            return incidents
                .OrderBy(i => i.CreatedAt)
                .Select(i => new IncidentView(
                    Rule: _ruleNames[i.AlertRuleId],
                    OpenedAt: SecondsSinceStart(i.CreatedAt),
                    Severity: i.Severity,
                    State: i.State,
                    ResolvedAt: i.ResolvedAt is { } resolved ? SecondsSinceStart(resolved) : null))
                .ToList();
        }

        private static int SecondsSinceStart(DateTime utc)
            => (int)(utc - StartTime.UtcDateTime).TotalSeconds;
    }
}
