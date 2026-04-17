// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ValidationHelperTests
{
    [Theory]
    [InlineData("nginx.service")]
    [InlineData("my-daemon.service")]
    [InlineData("app_worker")]
    public void ValidateServiceName_ValidFormats_ReturnsValid(string name)
    {
        // Act
        var result = ValidationHelper.ValidateServiceName(name);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("my service")]
    [InlineData("bad$name")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateServiceName_InvalidFormats_ReturnsInvalidWithMessage(string? name)
    {
        // Act
        var result = ValidationHelper.ValidateServiceName(name);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    [InlineData(-1)]
    public void ValidatePort_OutOfBounds_ReturnsInvalid(int port)
    {
        // Act
        var result = ValidationHelper.ValidatePort(port);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("65535");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(65535)]
    public void ValidatePort_ValidPorts_ReturnsValid(int port)
    {
        // Act
        var result = ValidationHelper.ValidatePort(port);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateTimeRange_ExceedsOneYear_ReturnsInvalid()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-400);
        var end = DateTime.UtcNow;

        // Act
        var result = ValidationHelper.ValidateTimeRange(start, end);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("365");
    }

    [Fact]
    public void ValidateTimeRange_StartAfterEnd_ReturnsInvalid()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = ValidationHelper.ValidateTimeRange(start, end);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateUrl_FtpScheme_ReturnsInvalid()
    {
        // Act
        var result = ValidationHelper.ValidateUrl("ftp://example.com/files");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HTTP");
    }

    [Fact]
    public void SanitizeInput_StringExceedingMaxLength_TruncatesToLimit()
    {
        // Arrange
        var input = new string('x', 2000);

        // Act
        var result = ValidationHelper.SanitizeInput(input, maxLength: 100);

        // Assert
        result.Should().HaveLength(100);
    }
}
