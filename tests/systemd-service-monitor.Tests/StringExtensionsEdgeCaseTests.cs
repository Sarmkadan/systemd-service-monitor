#nullable enable
using FluentAssertions;
using SystemdServiceMonitor.Extensions;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public sealed class StringExtensionsEdgeCaseTests
{
    [Fact]
    public void Truncate_NullInput_ReturnsEmpty() =>
        ((string?)null).Truncate(10).Should().BeEmpty();

    [Fact]
    public void Truncate_EmptyInput_ReturnsEmpty() =>
        "".Truncate(10).Should().BeEmpty();

    [Fact]
    public void Truncate_ShortString_ReturnsUnchanged() =>
        "hello".Truncate(10).Should().Be("hello");

    [Fact]
    public void Truncate_LongString_TruncatesWithEllipsis()
    {
        var result = "This is a very long service name".Truncate(15);
        result.Should().EndWith("...");
        result.Length.Should().BeLessThanOrEqualTo(15);
    }

    [Fact]
    public void IsNullOrWhiteSpaceEx_Null_ReturnsTrue() =>
        ((string?)null).IsNullOrWhiteSpaceEx().Should().BeTrue();

    [Fact]
    public void IsNullOrWhiteSpaceEx_Empty_ReturnsTrue() =>
        "".IsNullOrWhiteSpaceEx().Should().BeTrue();

    [Fact]
    public void IsNullOrWhiteSpaceEx_Whitespace_ReturnsTrue() =>
        "   ".IsNullOrWhiteSpaceEx().Should().BeTrue();

    [Fact]
    public void IsNullOrWhiteSpaceEx_ValidString_ReturnsFalse() =>
        "nginx".IsNullOrWhiteSpaceEx().Should().BeFalse();

    [Theory]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("some-service-name", "SomeServiceName")]
    [InlineData("single", "Single")]
    public void ToPascalCase_VariousInputs(string input, string expected) =>
        input.ToPascalCase().Should().Be(expected);

    [Fact]
    public void ToPascalCase_EmptyInput_ReturnsEmpty() =>
        "".ToPascalCase().Should().BeEmpty();

    [Fact]
    public void ToPascalCase_NullInput_ReturnsNull() =>
        ((string)null!).ToPascalCase().Should().BeNull();
}
