#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ConcurrencyAndEdgeCaseTests
{
    [Fact]
    public async Task ConcurrentReads_MultipleThreads_AllSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceMonitorService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var repository = Substitute.For<IServiceRepository>();

        var services = new List<ServiceInfo>
        {
            new() { UnitName = "service1.service", State = ServiceState.Active },
            new() { UnitName = "service2.service", State = ServiceState.Active }
        };

        repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(services));
        var monitorService = new ServiceMonitorService(logger, connectionService, repository);

        // Act: Concurrent reads from multiple threads
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => monitorService.GetAllServicesAsync())
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert: All reads succeeded
        results.Should().AllSatisfy(r => r.Should().HaveCount(2));
    }

    [Fact]
    public async Task ConcurrentServiceOperations_ThirtyParallelRestarts_AllSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceControlService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var options = new SystemdOptions { OperationTimeoutSeconds = 30 };
        var controlService = new ServiceControlService(logger, connectionService, options);

        // Act: 30 concurrent restart operations on same service
        var tasks = Enumerable.Range(0, 30)
            .Select(_ => controlService.RestartServiceAsync("test.service"))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert: All operations succeeded
        results.Should().HaveCount(30);
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task ConcurrentServiceMonitoring_DifferentServices_AllSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceControlService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var options = new SystemdOptions();
        var controlService = new ServiceControlService(logger, connectionService, options);

        var serviceNames = Enumerable.Range(1, 50)
            .Select(i => $"service{i:D3}.service")
            .ToList();

        // Act: Concurrent operations on 50 different services
        var startTasks = serviceNames.Select(s => controlService.StartServiceAsync(s));
        var stopTasks = serviceNames.Select(s => controlService.StopServiceAsync(s));
        var restartTasks = serviceNames.Select(s => controlService.RestartServiceAsync(s));

        var allTasks = startTasks.Concat(stopTasks).Concat(restartTasks).ToList();
        var results = await Task.WhenAll(allTasks);

        // Assert: All 150 operations succeeded
        results.Should().HaveCount(150);
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task EmptyServiceList_ReturnedCorrectly()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceMonitorService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var repository = Substitute.For<IServiceRepository>();

        repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(new List<ServiceInfo>()));
        var monitorService = new ServiceMonitorService(logger, connectionService, repository);

        // Act
        var result = (await monitorService.GetAllServicesAsync()).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ServiceWithNullFields_HandledGracefully()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            Description = null!,
            Dependencies = null,
            Dependents = null
        };

        // Act & Assert: Should not throw
        var status = ServiceHealthChecker.GetHealthStatus(service);
        status.Should().NotBe(ServiceHealthStatus.Unknown);
    }

    [Fact]
    public async Task ResourceMonitoring_RapidConsecutiveCalls_AllSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ResourceMonitorService>>();
        var options = new SystemdOptions();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var serviceMonitor = Substitute.For<IServiceMonitorService>();
        var resourceService = new ResourceMonitorService(logger, options, connectionService, serviceMonitor);

        // Act: Call GetSystemResourcesAsync 10 times rapidly
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => resourceService.GetSystemResourcesAsync())
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert: All calls succeeded
        results.Should().AllSatisfy(r =>
        {
            r.Should().NotBeNull();
            r.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            r.CpuCoreCount.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task AlertRuleManagement_AddingManyRulesConcurrently_AllAdded()
    {
        // Arrange
        var logger = Substitute.For<ILogger<AlertRulesEngine>>();
        var onCallService = Substitute.For<IOnCallScheduleService>();
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var options = new AlertOptions { Enabled = true, MaxRules = 100 };

        var engine = new AlertRulesEngine(
            logger,
            onCallService,
            Microsoft.Extensions.Options.Options.Create(options),
            httpClientFactory);

        var rules = Enumerable.Range(1, 50)
            .Select(i => new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = $"Alert{i:D3}",
                ServicePattern = $"app{i}*",
                Severity = AlertSeverity.Warning
            })
            .ToList();

        // Act: Add rules concurrently
        var tasks = rules.Select(r => engine.AddRuleAsync(r)).ToList();
        await Task.WhenAll(tasks);

        var allRules = (await engine.GetRulesAsync()).ToList();

        // Assert: All 50 rules were added
        allRules.Should().HaveCount(50);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ServiceMonitor_WithInvalidServiceNames_ReturnsNullOrEmpty(string? serviceName)
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceMonitorService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var repository = Substitute.For<IServiceRepository>();

        repository.GetByUnitNameAsync(serviceName ?? "", default)
            .ReturnsForAnyArgs(Task.FromResult<ServiceInfo?>(null));

        var monitorService = new ServiceMonitorService(logger, connectionService, repository);

        // Act & Assert
        var result = await monitorService.GetServiceByNameAsync(serviceName ?? "");
        result.Should().BeNull();
    }

    [Fact]
    public void ServiceHealthChecker_LargeRestartCount_HandleCorrectly()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "unstable.service",
            State = ServiceState.Active,
            RestartCount = 1000
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Critical);
    }

    [Fact]
    public async Task PaginationHelper_EdgeCases_CalculateCorrectly()
    {
        // Arrange & Act & Assert: Test with various edge cases
        Utilities.PaginationHelper.CalculateTotalPages(0, 10).Should().Be(0);
        Utilities.PaginationHelper.CalculateTotalPages(1, 10).Should().Be(1);
        Utilities.PaginationHelper.CalculateTotalPages(10, 10).Should().Be(1);
        Utilities.PaginationHelper.CalculateTotalPages(11, 10).Should().Be(2);

        Utilities.PaginationHelper.CalculateSkip(1, 10).Should().Be(0);
        Utilities.PaginationHelper.CalculateSkip(2, 10).Should().Be(10);
        Utilities.PaginationHelper.CalculateSkip(3, 10).Should().Be(20);
    }

    [Fact]
    public async Task MultipleServiceOperations_FastSequence_AllSucceed()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceControlService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var options = new SystemdOptions();
        var controlService = new ServiceControlService(logger, connectionService, options);

        // Act: Execute multiple operations in sequence (not parallel)
        var result1 = await controlService.StartServiceAsync("service1.service");
        var result2 = await controlService.StopServiceAsync("service1.service");
        var result3 = await controlService.RestartServiceAsync("service1.service");
        var result4 = await controlService.ReloadServiceAsync("service1.service");

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
        result4.Should().BeTrue();
    }
}
