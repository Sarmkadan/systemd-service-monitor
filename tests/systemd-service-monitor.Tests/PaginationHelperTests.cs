#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Utilities;
using Xunit;

/// <summary>
/// Tests for the PaginationHelper class.
/// </summary>
public class PaginationHelperTests
{
    /// <summary>
    /// Tests that null pagination parameters use the default values.
    /// </summary>
    [Fact]
    public void ValidatePaginationParams_NullInputs_ReturnsDefaults()
    {
        // Act
        var result = PaginationHelper.ValidatePaginationParams(null, null);

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(PaginationHelper.DefaultPageSize);
    }

    /// <summary>
    /// Tests that pagination parameters are clamped to their valid bounds.
    /// </summary>
    /// <param name="pageNumber">The page number to validate.</param>
    /// <param name="pageSize">The page size to validate.</param>
    /// <param name="expectedPageNumber">The expected normalized page number.</param>
    /// <param name="expectedPageSize">The expected normalized page size.</param>
    [Theory]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, PaginationHelper.MaxPageSize + 1, 1, PaginationHelper.MaxPageSize)]
    [InlineData(-1, PaginationHelper.DefaultPageSize, 1, PaginationHelper.DefaultPageSize)]
    public void ValidatePaginationParams_OutOfBounds_ClampsToValidBounds(
        int pageNumber,
        int pageSize,
        int expectedPageNumber,
        int expectedPageSize)
    {
        // Act
        var result = PaginationHelper.ValidatePaginationParams(pageNumber, pageSize);

        // Assert
        result.PageNumber.Should().Be(expectedPageNumber);
        result.PageSize.Should().Be(expectedPageSize);
    }

    /// <summary>
    /// Tests that CalculateSkip returns the correct number of items to skip.
    /// </summary>
    /// <param name="pageNumber">The requested page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="expected">The expected number of items to skip.</param>
    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(4, 25, 75)]
    [InlineData(-1, 10, 0)]
    public void CalculateSkip_PageNumber_ReturnsExpectedSkip(
        int pageNumber,
        int pageSize,
        int expected)
    {
        // Act
        var result = PaginationHelper.CalculateSkip(pageNumber, pageSize);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Tests that CalculateTotalPages handles empty, exact, and partial pages.
    /// </summary>
    /// <param name="totalCount">The total number of items.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="expected">The expected total number of pages.</param>
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    public void CalculateTotalPages_ItemCount_ReturnsExpectedTotalPages(
        int totalCount,
        int pageSize,
        int expected)
    {
        // Act
        var result = PaginationHelper.CalculateTotalPages(totalCount, pageSize);

        // Assert
        result.Should().Be(expected);
    }
}
