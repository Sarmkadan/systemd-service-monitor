#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Utilities;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ServiceHealthCheckerTests
{
    [Fact]
    public void GetHealthStatus_NullService_ReturnsUnknown()
    {
        // Act
        var status = ServiceHealthChecker.GetHealthStatus(null!);

        // Assert
        status.Should().Be(ServiceHealthStatus.Unknown);
    }

    [Fact]
    public void GetHealthStatus_FailedService_ReturnsCritical()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Failed,
            RestartCount = 0
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Critical);
    }

    [Fact]
    public void GetHealthStatus_ServiceWithHighRestartCount_ReturnsCritical()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            RestartCount = 15
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Critical);
    }

    [Fact]
    public void GetHealthStatus_ServiceWithModerateRestartCount_ReturnsWarning()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            RestartCount = 7
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Warning);
    }

    [Fact]
    public void GetHealthStatus_ActivatingService_ReturnsWarning()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Activating,
            RestartCount = 0
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Warning);
    }

    [Fact]
    public void GetHealthStatus_DeactivatingService_ReturnsWarning()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Deactivating,
            RestartCount = 0
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Warning);
    }

    [Fact]
    public void GetHealthStatus_ActiveAndStableService_ReturnsHealthy()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "nginx.service",
            State = ServiceState.Active,
            RestartCount = 0,
            AutoStart = true
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Healthy);
    }

    [Fact]
    public void GetHealthStatus_ActiveServiceWithMinimalRestarts_ReturnsHealthy()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            RestartCount = 2,
            AutoStart = true
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Healthy);
    }

    [Fact]
    public void GetHealthStatus_ActiveServiceWithManyRestarts_ReturnsWarning()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Active,
            RestartCount = 8
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Warning);
    }

    [Fact]
    public void GetHealthStatus_InactiveButAutoStartDisabled_ReturnsHealthy()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "optional.service",
            State = ServiceState.Inactive,
            RestartCount = 0,
            AutoStart = false
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Healthy);
    }

    [Fact]
    public void GetHealthStatus_InactiveButAutoStartEnabled_ReturnsWarning()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Inactive,
            RestartCount = 0,
            AutoStart = true
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Warning);
    }

    [Fact]
    public void GetHealthStatus_UnknownState_ReturnsUnknown()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "test.service",
            State = ServiceState.Unknown,
            RestartCount = 0
        };

        // Act
        var status = ServiceHealthChecker.GetHealthStatus(service);

        // Assert
        status.Should().Be(ServiceHealthStatus.Unknown);
    }

    [Fact]
    public void GetHealthSummary_ReturnNonEmptyString()
    {
        // Arrange
        var service = new ServiceInfo
        {
            UnitName = "nginx.service",
            State = ServiceState.Active,
            RestartCount = 0
        };

        // Act
        var summary = ServiceHealthChecker.GetHealthSummary(service);

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain(service.UnitName);
    }
}
