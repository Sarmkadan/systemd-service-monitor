#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ResourceMonitorServiceTests
{
    private readonly ILogger<ResourceMonitorService> _logger;
    private readonly SystemdOptions _options;
    private readonly ISystemdConnectionService _connectionService;
    private readonly IServiceMonitorService _serviceMonitor;
    private readonly ResourceMonitorService _service;

    public ResourceMonitorServiceTests()
    {
        _logger = Substitute.For<ILogger<ResourceMonitorService>>();
        _options = new SystemdOptions();
        _connectionService = Substitute.For<ISystemdConnectionService>();
        _serviceMonitor = Substitute.For<IServiceMonitorService>();
        _service = new ResourceMonitorService(_logger, _options, _connectionService, _serviceMonitor);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_ReturnsValidSystemResource()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.Should().NotBeNull();
        result.RecordedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CpuCoreCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_ContainsValidCpuCoreCount()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.CpuCoreCount.Should().Be(Environment.ProcessorCount);
        result.CpuCoreCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_LoadAveragesAreNonNegative()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.CpuLoad1Min.Should().BeGreaterThanOrEqualTo(0);
        result.CpuLoad5Min.Should().BeGreaterThanOrEqualTo(0);
        result.CpuLoad15Min.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_UptimeIsNonNegative()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.SystemUptimeSeconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_MemoryMetricsAreNonNegative()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.MemoryAvailableMb.Should().BeGreaterThanOrEqualTo(0);
        result.MemoryTotalMb.Should().BeGreaterThanOrEqualTo(0);
        result.MemoryUsedMb.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_DiskMetricsAreNonNegative()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.DiskAvailableMb.Should().BeGreaterThanOrEqualTo(0);
        result.DiskTotalMb.Should().BeGreaterThanOrEqualTo(0);
        result.DiskUsedMb.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_CalledMultipleTimes_ProducesDifferentTimestamps()
    {
        // Act
        var result1 = await _service.GetSystemResourcesAsync();
        await Task.Delay(100);
        var result2 = await _service.GetSystemResourcesAsync();

        // Assert
        result1.RecordedAt.Should().NotBe(result2.RecordedAt);
        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_MemoryTotalGreaterThanOrEqualToUsed()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.MemoryTotalMb.Should().BeGreaterThanOrEqualTo(result.MemoryUsedMb);
    }

    [Fact]
    public async Task GetSystemResourcesAsync_DiskTotalGreaterThanOrEqualToUsed()
    {
        // Act
        var result = await _service.GetSystemResourcesAsync();

        // Assert
        result.DiskTotalMb.Should().BeGreaterThanOrEqualTo(result.DiskUsedMb);
    }
}
