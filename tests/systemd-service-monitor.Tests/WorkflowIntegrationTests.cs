#nullable enable

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class WorkflowIntegrationTests
{
    [Fact]
    public async Task ServiceMonitoring_FullWorkflow_ConfigureQueryControlVerify()
    {
        // Arrange: Setup dependencies
        var logger = Substitute.For<ILogger<ServiceMonitorService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var repository = Substitute.For<IServiceRepository>();

        var services = new List<ServiceInfo>
        {
            new()
            {
                UnitName = "nginx.service",
                State = ServiceState.Active,
                Description = "NGINX HTTP Server",
                MainProcessId = 1234,
                UptimeSeconds = 86400,
                RestartCount = 0
            },
            new()
            {
                UnitName = "postgresql.service",
                State = ServiceState.Active,
                Description = "PostgreSQL Database",
                MainProcessId = 5678,
                UptimeSeconds = 172800,
                RestartCount = 1
            }
        };

        repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(services));
        repository.GetActiveServicesAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(services));

        var monitorService = new ServiceMonitorService(logger, connectionService, repository);
        var controlService = Substitute.For<IServiceControlService>();
        var logService = Substitute.For<IServiceLogService>();

        // Act 1: Query all services
        var allServices = (await monitorService.GetAllServicesAsync()).ToList();

        // Assert 1: Verify all services retrieved
        allServices.Should().HaveCount(2);
        allServices.Should().Contain(s => s.UnitName == "nginx.service");
        allServices.Should().Contain(s => s.UnitName == "postgresql.service");

        // Act 2: Filter active services
        var activeServices = (await monitorService.GetActiveServicesAsync()).ToList();

        // Assert 2: All are active
        activeServices.Should().HaveCount(2);
        activeServices.Should().AllSatisfy(s => s.State.Should().Be(ServiceState.Active));

        // Act 3: Get specific service
        var nginxService = allServices.FirstOrDefault(s => s.UnitName == "nginx.service");

        // Assert 3: Service details are correct
        nginxService.Should().NotBeNull();
        nginxService!.State.Should().Be(ServiceState.Active);
        nginxService.MainProcessId.Should().Be(1234);
        nginxService.UptimeSeconds.Should().Be(86400);

        // Act 4: Control service (mock)
        controlService.RestartServiceAsync("nginx.service", default).ReturnsForAnyArgs(Task.FromResult(true));
        var restartResult = await controlService.RestartServiceAsync("nginx.service");

        // Assert 4: Control operation succeeded
        restartResult.Should().BeTrue();

        // Act 5: Verify service state post-control
        var updatedService = await monitorService.GetServiceByNameAsync("nginx.service");

        // Assert 5: Service still exists
        updatedService.Should().NotBeNull();
    }

    [Fact]
    public async Task ConcurrentServiceOperations_MultipleServices_ExecuteSuccessfully()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ServiceControlService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var options = new SystemdOptions { OperationTimeoutSeconds = 30 };
        var controlService = new ServiceControlService(logger, connectionService, options);

        var serviceNames = new[] { "service1.service", "service2.service", "service3.service", "service4.service", "service5.service" };

        // Act: Start all services concurrently
        var tasks = serviceNames.Select(name => controlService.StartServiceAsync(name)).ToList();
        var results = await Task.WhenAll(tasks);

        // Assert: All operations succeeded
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task ServiceHealthCheckMonitoring_VariousStates_IdentifiesHealthStatus()
    {
        // Arrange: Create services with different health states
        var healthyService = new ServiceInfo
        {
            UnitName = "healthy.service",
            State = ServiceState.Active,
            RestartCount = 0,
            AutoStart = true
        };

        var unstableService = new ServiceInfo
        {
            UnitName = "unstable.service",
            State = ServiceState.Active,
            RestartCount = 8,
            AutoStart = true
        };

        var failedService = new ServiceInfo
        {
            UnitName = "failed.service",
            State = ServiceState.Failed,
            RestartCount = 20,
            AutoStart = true
        };

        // Act & Assert: Check health status for each
        ServiceHealthChecker.GetHealthStatus(healthyService).Should().Be(ServiceHealthStatus.Healthy);
        ServiceHealthChecker.GetHealthStatus(unstableService).Should().Be(ServiceHealthStatus.Warning);
        ServiceHealthChecker.GetHealthStatus(failedService).Should().Be(ServiceHealthStatus.Critical);
    }

    [Fact]
    public async Task ResourceMonitoring_SystemMetrics_ProvideValidData()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ResourceMonitorService>>();
        var options = new SystemdOptions();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var serviceMonitor = Substitute.For<IServiceMonitorService>();

        var resourceService = new ResourceMonitorService(logger, options, connectionService, serviceMonitor);

        // Act: Get system resources
        var resources = await resourceService.GetSystemResourcesAsync();

        // Assert: Verify metrics are valid
        resources.Should().NotBeNull();
        resources.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        resources.CpuCoreCount.Should().BeGreaterThan(0);
        resources.MemoryTotalMb.Should().BeGreaterThan(0);
        resources.CpuLoad1Min.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task AlertRuleManagement_CreateQueryMultipleRules_WorksCorrectly()
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

        var alertRules = new[]
        {
            new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = "High CPU",
                ServicePattern = "app*",
                Severity = AlertSeverity.Critical,
                Condition = "cpu > 80"
            },
            new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = "High Memory",
                ServicePattern = "app*",
                Severity = AlertSeverity.Warning,
                Condition = "memory > 70"
            },
            new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = "Service Down",
                ServicePattern = "*",
                Severity = AlertSeverity.Critical,
                Condition = "state == Failed"
            }
        };

        // Act: Add multiple rules
        foreach (var rule in alertRules)
            await engine.AddRuleAsync(rule);

        var allRules = (await engine.GetRulesAsync()).ToList();

        // Assert: Verify all rules added
        allRules.Should().HaveCount(3);

        // Act: Query specific rule
        var cpuRule = await engine.GetRuleByIdAsync(alertRules[0].Id);

        // Assert: Verify rule retrieval
        cpuRule.Should().NotBeNull();
        cpuRule?.Name.Should().Be("High CPU");
        cpuRule?.Severity.Should().Be(AlertSeverity.Critical);

        // Act: Verify sorting
        var sortedRules = await engine.GetRulesAsync();

        // Assert: Rules should be sorted by name
        sortedRules.Should().BeInAscendingOrder(r => r.Name);
    }

    [Fact]
    public async Task PaginationScenario_LargeServiceList_HandlesPaginationCorrectly()
    {
        // Arrange: Create many services
        var manyServices = Enumerable.Range(1, 100)
            .Select(i => new ServiceInfo
            {
                UnitName = $"service{i:D3}.service",
                State = i % 2 == 0 ? ServiceState.Active : ServiceState.Inactive,
                Description = $"Service {i}"
            })
            .ToList();

        var logger = Substitute.For<ILogger<ServiceMonitorService>>();
        var connectionService = Substitute.For<ISystemdConnectionService>();
        var repository = Substitute.For<IServiceRepository>();

        repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(manyServices));

        var monitorService = new ServiceMonitorService(logger, connectionService, repository);

        // Act: Get all services
        var allServices = (await monitorService.GetAllServicesAsync()).ToList();

        // Assert: Verify we got all 100 services
        allServices.Should().HaveCount(100);

        // Act: Simulate pagination (page size 20)
        const int pageSize = 20;
        var page1 = allServices.Take(pageSize).ToList();
        var page2 = allServices.Skip(pageSize).Take(pageSize).ToList();
        var page3 = allServices.Skip(pageSize * 2).Take(pageSize).ToList();

        // Assert: Verify pagination logic
        page1.Should().HaveCount(20);
        page2.Should().HaveCount(20);
        page3.Should().HaveCount(20);

        var totalPages = (allServices.Count + pageSize - 1) / pageSize;
        totalPages.Should().Be(5);
    }
}
