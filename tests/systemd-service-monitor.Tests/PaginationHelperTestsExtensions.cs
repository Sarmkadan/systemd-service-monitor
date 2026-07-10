#nullable enable

using FluentAssertions;
using SystemdServiceMonitor.Utilities;

/// <summary>
/// Extension methods for PaginationHelperTests to provide additional testing utilities.
/// </summary>
public static class PaginationHelperTestsExtensions
{
    /// <summary>
    /// Creates a PaginationMetadata object with the specified parameters.
    /// </summary>
    public static PaginationMetadata CreateMetadata(
        this PaginationHelperTests _,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return PaginationHelper.GetMetadata(pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Validates that the pagination metadata is correct for a given scenario.
    /// </summary>
    public static void ValidateMetadata(
        this PaginationHelperTests _,
        PaginationMetadata metadata,
        int expectedPageNumber,
        int expectedPageSize,
        int expectedTotalCount,
        int expectedTotalPages,
        bool expectedHasPreviousPage,
        bool expectedHasNextPage,
        int expectedStartIndex,
        int expectedEndIndex)
    {
        metadata.PageNumber.Should().Be(expectedPageNumber);
        metadata.PageSize.Should().Be(expectedPageSize);
        metadata.TotalCount.Should().Be(expectedTotalCount);
        metadata.TotalPages.Should().Be(expectedTotalPages);
        metadata.HasPreviousPage.Should().Be(expectedHasPreviousPage);
        metadata.HasNextPage.Should().Be(expectedHasNextPage);
        metadata.StartIndex.Should().Be(expectedStartIndex);
        metadata.EndIndex.Should().Be(expectedEndIndex);
    }

    /// <summary>
    /// Calculates the expected total pages for a given total count and page size.
    /// </summary>
    public static int CalculateExpectedTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0)
            return 0;

        return (totalCount + pageSize - 1) / pageSize;
    }

    /// <summary>
    /// Calculates the expected start and end indices for a page.
    /// </summary>
    public static (int StartIndex, int EndIndex) CalculateExpectedIndices(
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var startIndex = (pageNumber - 1) * pageSize + 1;
        var endIndex = Math.Min(pageNumber * pageSize, totalCount);
        return (startIndex, endIndex);
    }
}
