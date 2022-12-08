#nullable enable

using System;
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
    /// <param name="_">The test instance (discard parameter).</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A new <see cref="PaginationMetadata"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageNumber"/>, <paramref name="pageSize"/>, or <paramref name="totalCount"/> is negative.</exception>
    public static PaginationMetadata CreateMetadata(
        this PaginationHelperTests _,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pageNumber);
        ArgumentOutOfRangeException.ThrowIfNegative(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        return PaginationHelper.GetMetadata(pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Validates that the pagination metadata matches the expected values.
    /// </summary>
    /// <param name="_">The test instance (discard parameter).</param>
    /// <param name="metadata">The pagination metadata to validate.</param>
    /// <param name="expectedPageNumber">The expected page number.</param>
    /// <param name="expectedPageSize">The expected page size.</param>
    /// <param name="expectedTotalCount">The expected total item count.</param>
    /// <param name="expectedTotalPages">The expected total page count.</param>
    /// <param name="expectedHasPreviousPage">The expected value for HasPreviousPage.</param>
    /// <param name="expectedHasNextPage">The expected value for HasNextPage.</param>
    /// <param name="expectedStartIndex">The expected start index (1-based).</param>
    /// <param name="expectedEndIndex">The expected end index (1-based).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is null.</exception>
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
        ArgumentNullException.ThrowIfNull(metadata);

        metadata.PageNumber.Should().Be(expectedPageNumber);
        metadata.PageSize.Should().Be(expectedPageSize);
        metadata.TotalCount.Should().Be(expectedTotalCount);
        metadata.TotalPages.Should().Be(expectedTotalPages);
        metadata.HasPreviousPage.Should().Be(expectedHasPreviousPage);
        metadata.HasNextPage.Should().Be(expectedHasNextPage);
        metadata.StartIndex.Should().Be(expectedStartIndex);
        metadata.EndIndex.Should().Be(expectedEndIndex);
    }

}
