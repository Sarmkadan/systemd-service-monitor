#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

/// <summary>
/// Tests for the PathResolver class.
/// </summary>
public class PathResolverTests
{
    /// <summary>
    /// Tests the NormalizeServiceName method.
    /// </summary>
    [Theory]
    [InlineData("myservice", "myservice.service")]
    [InlineData("myservice.service", "myservice.service")]
    [InlineData("MYSERVICE", "myservice.service")]
    public void NormalizeServiceName_ShouldAddExtensionOrKeepIt(string input, string expected)
    {
        // Act
        var result = PathResolver.NormalizeServiceName(input);
        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests the RemoveServiceExtension method.
    /// </summary>
    [Theory]
    [InlineData("myservice.service", "myservice")]
    [InlineData("myservice", "myservice")]
    public void RemoveServiceExtension_ShouldRemoveExtensionIfPresent(string input, string expected)
    {
        // Act
        var result = PathResolver.RemoveServiceExtension(input);
        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests the IsValidServicePath method with an invalid path.
    /// </summary>
    [Fact]
    public void IsValidServicePath_ShouldReturnFalseForInvalidPath()
    {
        // Act
        var result = PathResolver.IsValidServicePath("/tmp/foo");
        // Assert
        result.Should().BeFalse();
    }
}
