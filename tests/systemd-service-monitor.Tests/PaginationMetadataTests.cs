#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

/// <summary>
/// Tests for the PaginationMetadata class.
/// </summary>
namespace SystemdServiceMonitor.Tests;

public class PaginationMetadataTests
{
    /// <summary>
    /// Tests that PaginationMetadata can be instantiated with default values.
    /// </summary>
    [Fact]
    public void PaginationMetadata_DefaultConstructor_InitializesCorrectly()
    {
        // Arrange & Act
        var metadata = new PaginationMetadata();

        // Assert
        metadata.PageNumber.Should().Be(0);
        metadata.PageSize.Should().Be(0);
        metadata.TotalCount.Should().Be(0);
        metadata.TotalPages.Should().Be(0);
        metadata.HasNextPage.Should().BeFalse();
        metadata.HasPreviousPage.Should().BeFalse();
        metadata.StartIndex.Should().Be(0);
        metadata.EndIndex.Should().Be(0);
    }

    /// <summary>
    /// Tests that PaginationMetadata can be instantiated with custom values.
    /// </summary>
    [Fact]
    public void PaginationMetadata_CustomValues_InitializesCorrectly()
    {
        // Arrange & Act
        var metadata = new PaginationMetadata
        {
            PageNumber = 3,
            PageSize = 50,
            TotalCount = 125,
            TotalPages = 3,
            HasNextPage = false,
            HasPreviousPage = true,
            StartIndex = 101,
            EndIndex = 125
        };

        // Assert
        metadata.PageNumber.Should().Be(3);
        metadata.PageSize.Should().Be(50);
        metadata.TotalCount.Should().Be(125);
        metadata.TotalPages.Should().Be(3);
        metadata.HasNextPage.Should().BeFalse();
        metadata.HasPreviousPage.Should().BeTrue();
        metadata.StartIndex.Should().Be(101);
        metadata.EndIndex.Should().Be(125);
    }

    /// <summary>
    /// Tests that PaginationMetadata properties can be modified after creation.
    /// </summary>
    [Fact]
    public void PaginationMetadata_Properties_CanBeModified()
    {
        // Arrange
        var metadata = new PaginationMetadata();

        // Act
        metadata.PageNumber = 5;
        metadata.PageSize = 25;
        metadata.TotalCount = 100;
        metadata.TotalPages = 4;
        metadata.HasNextPage = true;
        metadata.HasPreviousPage = true;
        metadata.StartIndex = 76;
        metadata.EndIndex = 100;

        // Assert
        metadata.PageNumber.Should().Be(5);
        metadata.PageSize.Should().Be(25);
        metadata.TotalCount.Should().Be(100);
        metadata.TotalPages.Should().Be(4);
        metadata.HasNextPage.Should().BeTrue();
        metadata.HasPreviousPage.Should().BeTrue();
        metadata.StartIndex.Should().Be(76);
        metadata.EndIndex.Should().Be(100);
    }

    /// <summary>
    /// Tests that PaginationMetadata can be used in assertions for pagination scenarios.
    /// </summary>
    [Fact]
    public void PaginationMetadata_PaginationScenario_ValidValues()
    {
        // Arrange - Simulate pagination for 25 items per page, 100 total items, page 3
        var metadata = new PaginationMetadata
        {
            PageNumber = 3,
            PageSize = 25,
            TotalCount = 100,
            TotalPages = 4,
            HasNextPage = true,
            HasPreviousPage = true,
            StartIndex = 51,
            EndIndex = 75
        };

        // Assert
        metadata.PageNumber.Should().Be(3);
        metadata.TotalPages.Should().Be(4);
        metadata.HasNextPage.Should().BeTrue();
        metadata.HasPreviousPage.Should().BeTrue();
        metadata.StartIndex.Should().Be(51);
        metadata.EndIndex.Should().Be(75);
    }

    /// <summary>
    /// Tests that PaginationMetadata with first page has correct values.
    /// </summary>
    [Fact]
    public void PaginationMetadata_FirstPage_CorrectValues()
    {
        // Arrange
        var metadata = new PaginationMetadata
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 50,
            TotalPages = 5,
            HasNextPage = true,
            HasPreviousPage = false,
            StartIndex = 1,
            EndIndex = 10
        };

        // Assert
        metadata.HasPreviousPage.Should().BeFalse();
        metadata.HasNextPage.Should().BeTrue();
        metadata.StartIndex.Should().Be(1);
        metadata.EndIndex.Should().Be(10);
    }

    /// <summary>
    /// Tests that PaginationMetadata with last page has correct values.
    /// </summary>
    [Fact]
    public void PaginationMetadata_LastPage_CorrectValues()
    {
        // Arrange
        var metadata = new PaginationMetadata
        {
            PageNumber = 5,
            PageSize = 10,
            TotalCount = 50,
            TotalPages = 5,
            HasNextPage = false,
            HasPreviousPage = true,
            StartIndex = 41,
            EndIndex = 50
        };

        // Assert
        metadata.HasNextPage.Should().BeFalse();
        metadata.HasPreviousPage.Should().BeTrue();
        metadata.StartIndex.Should().Be(41);
        metadata.EndIndex.Should().Be(50);
    }
}
