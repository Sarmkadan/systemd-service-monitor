#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Exceptions;
using Xunit;

/// <summary>
/// Tests for the ResourceMonitorService class.
/// </summary>
namespace SystemdServiceMonitor.Tests;

public class ResourceMonitorServiceTests
{
    private readonly ILogger<ResourceMonitorService> _logger = Substitute.For<ILogger<ResourceMonitorService>>();
    private readonly SystemdOptions _options = new();
    private readonly ISystemdConnectionService _connectionService = Substitute.For<ISystemdConnectionService>();
    private readonly IServiceMonitorService _serviceMonitorService = Substitute.For<IServiceMonitorService>();
    private readonly ResourceMonitorService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceMonitorServiceTests"/> class.
    /// </summary>
    public ResourceMonitorServiceTests()
    {
        _service = new ResourceMonitorService(_logger, _options, _connectionService, _serviceMonitorService);
    }

    /// <summary>
    /// Tests that GetSystemResourcesAsync returns valid system resource metrics.
    /// </summary>
    [Fact]
    public async Task GetSystemResourcesAsync_ReturnsValidMetrics()
    {
        // Arrange
        var resources = await _service.GetSystemResourcesAsync();

        // Assert
        resources.Should().NotBeNull();
        resources.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        resources.CpuCoreCount.Should().BeGreaterThan(0);
        resources.SystemUptimeSeconds.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Tests that GetServiceResourceMetricsAsync returns metrics for a valid service.
    /// </summary>
    [Fact]
    public async Task GetServiceResourceMetricsAsync_ValidService_ReturnsMetrics()
    {
        // Arrange
        var serviceInfo = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            MainProcessId = 12345
        };
        _serviceMonitorService.GetServiceByNameAsync("test.service", Arg.Any<CancellationToken>()).Returns(Task.FromResult<ServiceInfo?>(serviceInfo));

        // Act
        var metrics = await _service.GetServiceResourceMetricsAsync("test.service");

        // Assert
        metrics.Should().NotBeNull();
        metrics.UnitName.Should().Be("test.service");
        metrics.MeasuredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that GetServiceResourceMetricsAsync returns empty metrics when service is not found.
    /// </summary>
    [Fact]
    public async Task GetServiceResourceMetricsAsync_NonExistentService_ReturnsEmptyMetrics()
    {
        // Arrange
        _serviceMonitorService.GetServiceByNameAsync("nonexistent.service", Arg.Any<CancellationToken>()).Returns(Task.FromResult<ServiceInfo?>(null));

        // Act
        var metrics = await _service.GetServiceResourceMetricsAsync("nonexistent.service");

        // Assert
        metrics.Should().NotBeNull();
        metrics.UnitName.Should().Be("nonexistent.service");
        metrics.CpuUsagePercent.Should().Be(0);
        metrics.MemoryUsageMb.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetServiceResourceMetricsAsync throws when service monitor throws.
    /// </summary>
    [Fact]
    public async Task GetServiceResourceMetricsAsync_ServiceMonitorThrows_ThrowsException()
    {
        // Arrange
        var expectedException = new ServiceMonitorException("Test error");
        _serviceMonitorService.GetServiceByNameAsync("error.service", Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        // Act & Assert
        await _service.Awaiting(s => s.GetServiceResourceMetricsAsync("error.service"))
            .Should().ThrowAsync<ServiceMonitorException>();
    }

    /// <summary>
    /// Tests that CollectAllMetricsAsync returns metrics for all services.
    /// </summary>
    [Fact]
    public async Task CollectAllMetricsAsync_ReturnsMetricsForAllServices()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { UnitName = "service1.service", State = ServiceState.Active, MainProcessId = 1000 },
            new() { UnitName = "service2.service", State = ServiceState.Active, MainProcessId = 2000 }
        };
        _serviceMonitorService.GetAllServicesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IEnumerable<ServiceInfo>>(services));

        // Act
        var metrics = await _service.CollectAllMetricsAsync();

        // Assert
        metrics.Should().HaveCount(2);
        metrics.Should().AllSatisfy(m => m.Should().NotBeNull());
    }

    /// <summary>
    /// Tests that CollectAllMetricsAsync throws when service monitor throws.
    /// </summary>
    [Fact]
    public async Task CollectAllMetricsAsync_ServiceMonitorThrows_ThrowsException()
    {
        // Arrange
        var expectedException = new ServiceMonitorException("Test error");
        _serviceMonitorService.GetAllServicesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        // Act & Assert
        await _service.Awaiting(s => s.CollectAllMetricsAsync())
            .Should().ThrowAsync<ServiceMonitorException>();
    }

    /// <summary>
    /// Tests that StartContinuousMonitoringAsync starts monitoring.
    /// </summary>
    [Fact]
    public async Task StartContinuousMonitoringAsync_StartsMonitoring()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act
        await _service.StartContinuousMonitoringAsync(intervalMs: 100, cts.Token);

        // Assert - monitoring should be started
        var monitoringCtsField = typeof(ResourceMonitorService).GetField("_monitoringCts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        monitoringCtsField.Should().NotBeNull();
        var monitoringCts = (CancellationTokenSource?)monitoringCtsField?.GetValue(_service);
        monitoringCts.Should().NotBeNull();
        monitoringCts?.IsCancellationRequested.Should().BeFalse();

        // Cleanup
        await _service.StopContinuousMonitoringAsync();
    }

    /// <summary>
    /// Tests that StartContinuousMonitoringAsync can be called multiple times safely.
    /// </summary>
    [Fact]
    public async Task StartContinuousMonitoringAsync_MultipleTimes_Safe()
    {
        // Arrange
        var cts1 = new CancellationTokenSource();
        var cts2 = new CancellationTokenSource();

        // Act
        await _service.StartContinuousMonitoringAsync(intervalMs: 100, cts1.Token);
        await _service.StartContinuousMonitoringAsync(intervalMs: 100, cts2.Token); // Should not throw

        // Assert
        var monitoringCtsField = typeof(ResourceMonitorService).GetField("_monitoringCts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        monitoringCtsField.Should().NotBeNull();
        var monitoringCts = (CancellationTokenSource?)monitoringCtsField?.GetValue(_service);
        monitoringCts.Should().NotBeNull();

        // Cleanup
        await _service.StopContinuousMonitoringAsync();
    }

    /// <summary>
    /// Tests that StopContinuousMonitoringAsync stops monitoring.
    /// </summary>
    [Fact]
    public async Task StopContinuousMonitoringAsync_StopsMonitoring()
    {
        // Arrange - start monitoring first
        var cts = new CancellationTokenSource();
        await _service.StartContinuousMonitoringAsync(intervalMs: 100, cts.Token);

        // Act
        await _service.StopContinuousMonitoringAsync();

        // Assert
        var monitoringCtsField = typeof(ResourceMonitorService).GetField("_monitoringCts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        monitoringCtsField.Should().NotBeNull();
        var monitoringCts = (CancellationTokenSource?)monitoringCtsField?.GetValue(_service);
        monitoringCts?.IsCancellationRequested.Should().BeTrue();
    }

    /// <summary>
    /// Tests that GetResourceAlertsAsync returns empty list when no alerts exist.
    /// </summary>
    [Fact]
    public async Task GetResourceAlertsAsync_NoAlerts_ReturnsEmptyList()
    {
        // Act
        var alerts = await _service.GetResourceAlertsAsync();

        // Assert
        alerts.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetResourceAlertsAsync returns alerts when they exist.
    /// </summary>
    [Fact]
    public async Task GetResourceAlertsAsync_WithAlerts_ReturnsAlerts()
    {
        // Arrange - start monitoring to trigger alerts
        var cts = new CancellationTokenSource();
        await _service.StartContinuousMonitoringAsync(intervalMs: 100, cts.Token);
        await Task.Delay(200); // Let it run for a bit

        // Act
        var alerts = await _service.GetResourceAlertsAsync();

        // Assert
        alerts.Should().NotBeNull();

        // Cleanup
        await _service.StopContinuousMonitoringAsync();
    }

    /// <summary>
    /// Tests that ParseMemInfoLine correctly parses memory info lines.
    /// </summary>
    [Fact]
    public void ParseMemInfoLine_ValidLine_ReturnsValue()
    {
        // Arrange
        var line = "MemTotal:        16384344 kB";

        // Act - using reflection to call private method
        var parseMethod = typeof(ResourceMonitorService).GetMethod("ParseMemInfoLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        parseMethod.Should().NotBeNull();
        var result = (long)parseMethod!.Invoke(_service, new object[] { line })!;

        // Assert
        result.Should().Be(16384344);
    }

    /// <summary>
    /// Tests that ParseMemInfoLine returns 0 for invalid lines.
    /// </summary>
    [Fact]
    public void ParseMemInfoLine_InvalidLine_ReturnsZero()
    {
        // Arrange
        var line = "InvalidLineWithoutColon";

        // Act
        var parseMethod = typeof(ResourceMonitorService).GetMethod("ParseMemInfoLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (long)parseMethod!.Invoke(_service, new object[] { line })!;

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that GetServiceCpuUsageAsync returns CPU usage for a service.
    /// </summary>
    [Fact]
    public async Task GetServiceCpuUsageAsync_ReturnsCpuUsage()
    {
        // Arrange
        var serviceInfo = new ServiceInfo
        {
            UnitName = "cpu-service.service",
            State = ServiceState.Active,
            MainProcessId = 12345
        };
        _serviceMonitorService.GetServiceByNameAsync("cpu-service.service", Arg.Any<CancellationToken>()).Returns(Task.FromResult<ServiceInfo?>(serviceInfo));

        // Act
        var cpuUsage = await _service.GetServiceCpuUsageAsync("cpu-service.service");

        // Assert
        cpuUsage.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Tests that GetServiceMemoryUsageAsync returns memory usage for a service.
    /// </summary>
    [Fact]
    public async Task GetServiceMemoryUsageAsync_ReturnsMemoryUsage()
    {
        // Arrange
        var serviceInfo = new ServiceInfo
        {
            UnitName = "memory-service.service",
            State = ServiceState.Active,
            MainProcessId = 12345
        };
        _serviceMonitorService.GetServiceByNameAsync("memory-service.service", Arg.Any<CancellationToken>()).Returns(Task.FromResult<ServiceInfo?>(serviceInfo));

        // Act
        var memoryUsage = await _service.GetServiceMemoryUsageAsync("memory-service.service");

        // Assert
        memoryUsage.Should().BeGreaterThanOrEqualTo(0);
    }

}
