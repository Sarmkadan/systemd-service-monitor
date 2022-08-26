#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class PaginationHelperTests
{
    [Fact]
    public void CalculateTotalPages_ZeroPageSize_ReturnsZero()
    {
        // Arrange & Act
        var result = PaginationHelper.CalculateTotalPages(totalCount: 100, pageSize: 0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateTotalPages_NotEvenlyDivisible_RoundsUp()
    {
        // Arrange & Act
        var result = PaginationHelper.CalculateTotalPages(totalCount: 101, pageSize: 10);

        // Assert
        result.Should().Be(11);
    }

    [Fact]
    public void CalculateSkip_FirstPage_ReturnsZero()
    {
        // Arrange & Act
        var result = PaginationHelper.CalculateSkip(pageNumber: 1, pageSize: 50);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ValidatePaginationParams_NegativePageNumber_ClampsToOne()
    {
        // Arrange & Act
        var (pageNumber, pageSize) = PaginationHelper.ValidatePaginationParams(pageNumber: -5, pageSize: 20);

        // Assert
        pageNumber.Should().Be(1);
        pageSize.Should().Be(20);
    }

    [Fact]
    public void GetPageNumbers_NearEndOfTotalPages_AdjustsWindowToFit()
    {
        // Arrange - current page near the end of 10 total pages, window of 5
        // halfPages=2 → startPage=max(1,9-2)=7, endPage=min(10,7+4)=10
        // endPage-startPage+1=4 < 5 → adjust: startPage=max(1,10-4)=6
        var result = PaginationHelper.GetPageNumbers(currentPage: 9, totalPages: 10, pagesToShow: 5);

        // Assert
        result.Should().BeEquivalentTo(new[] { 6, 7, 8, 9, 10 }, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public void GetMetadata_MiddlePage_ReportsCorrectStartAndEndIndex()
    {
        // Arrange: page 3 of 10 items per page, 25 total → page 3 has items 21-25
        var metadata = PaginationHelper.GetMetadata(pageNumber: 3, pageSize: 10, totalCount: 25);

        // Assert
        metadata.StartIndex.Should().Be(21);
        metadata.EndIndex.Should().Be(25);
        metadata.HasPreviousPage.Should().BeTrue();
        metadata.HasNextPage.Should().BeFalse();
    }
}
