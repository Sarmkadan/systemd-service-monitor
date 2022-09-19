#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ServiceControlServiceTests
{
    private readonly ILogger<ServiceControlService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly SystemdOptions _options;
    private readonly ServiceControlService _service;

    public ServiceControlServiceTests()
    {
        _logger = Substitute.For<ILogger<ServiceControlService>>();
        _connectionService = Substitute.For<ISystemdConnectionService>();
        _options = new SystemdOptions
        {
            OperationTimeoutSeconds = 30,
            MaxRetries = 3
        };
        _service = new ServiceControlService(_logger, _connectionService, _options);
    }

    [Fact]
    public async Task StartServiceAsync_WithValidService_ReturnsTrue()
    {
        // Arrange
        const string serviceName = "nginx.service";

        // Act
        var result = await _service.StartServiceAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task StopServiceAsync_WithValidService_ReturnsTrue()
    {
        // Arrange
        const string serviceName = "nginx.service";

        // Act
        var result = await _service.StopServiceAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RestartServiceAsync_WithValidService_ReturnsTrue()
    {
        // Arrange
        const string serviceName = "nginx.service";

        // Act
        var result = await _service.RestartServiceAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task StartServiceAsync_WithEmptyServiceName_ReturnsFalse()
    {
        // Arrange
        const string serviceName = "";

        // Act
        var result = await _service.StartServiceAsync(serviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task StopServiceAsync_WithEmptyServiceName_ReturnsFalse()
    {
        // Arrange
        const string serviceName = "";

        // Act
        var result = await _service.StopServiceAsync(serviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RestartServiceAsync_WithNullServiceName_ReturnsFalse()
    {
        // Arrange
        const string serviceName = null!;

        // Act
        var result = await _service.RestartServiceAsync(serviceName ?? string.Empty);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ReloadServiceAsync_WithValidService_ReturnsTrue()
    {
        // Arrange
        const string serviceName = "nginx.service";

        // Act
        var result = await _service.ReloadServiceAsync(serviceName);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task OperationAsync_WithInvalidInput_ReturnsFalse(string? input)
    {
        // Arrange
        var serviceName = input ?? string.Empty;

        // Act
        var startResult = await _service.StartServiceAsync(serviceName);
        var stopResult = await _service.StopServiceAsync(serviceName);
        var restartResult = await _service.RestartServiceAsync(serviceName);

        // Assert
        startResult.Should().BeFalse();
        stopResult.Should().BeFalse();
        restartResult.Should().BeFalse();
    }

    [Fact]
    public async Task RestartServiceAsync_CalledMultipleTimes_SucceedsEachTime()
    {
        // Arrange
        const string serviceName = "test.service";

        // Act
        var result1 = await _service.RestartServiceAsync(serviceName);
        var result2 = await _service.RestartServiceAsync(serviceName);
        var result3 = await _service.RestartServiceAsync(serviceName);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeTrue();
    }
}
