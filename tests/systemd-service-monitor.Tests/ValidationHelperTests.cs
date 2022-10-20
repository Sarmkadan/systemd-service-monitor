#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

/// <summary>
/// Tests for the ValidationHelper class.
/// </summary>
public class ValidationHelperTests
{
    /// <summary>
    /// Tests that the ValidateServiceName method returns a valid result for valid service names.
    /// </summary>
    /// <param name="name">The service name to validate.</param>
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

    /// <summary>
    /// Tests that the ValidateServiceName method returns an invalid result with a message for invalid service names.
    /// </summary>
    /// <param name="name">The service name to validate.</param>
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

    /// <summary>
    /// Tests that the ValidatePort method returns an invalid result for out-of-bounds ports.
    /// </summary>
    /// <param name="port">The port to validate.</param>
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

    /// <summary>
    /// Tests that the ValidatePort method returns a valid result for valid ports.
    /// </summary>
    /// <param name="port">The port to validate.</param>
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

    /// <summary>
    /// Tests that the ValidateTimeRange method returns an invalid result when the end date exceeds the start date by more than one year.
    /// </summary>
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

    /// <summary>
    /// Tests that the ValidateTimeRange method returns an invalid result when the start date is after the end date.
    /// </summary>
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

    /// <summary>
    /// Tests that the ValidateUrl method returns an invalid result for URLs with the FTP scheme.
    /// </summary>
    public void ValidateUrl_FtpScheme_ReturnsInvalid()
    {
        // Act
        var result = ValidationHelper.ValidateUrl("ftp://example.com/files");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("HTTP");
    }

    /// <summary>
    /// Tests that the SanitizeInput method truncates input strings that exceed the maximum length.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <param name="maxLength">The maximum length of the sanitized string.</param>
    /// <returns>The sanitized string.</returns>
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
