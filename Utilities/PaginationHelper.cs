// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Helper class for pagination operations.
/// Provides utilities for page calculations, validation, and navigation.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Default page size when not specified.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// Maximum allowed page size to prevent abuse.
    /// </summary>
    public const int MaxPageSize = 10000;

    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// </summary>
    public static (int PageNumber, int PageSize) ValidatePaginationParams(
        int? pageNumber = null,
        int? pageSize = null)
    {
        var validPageNumber = Math.Max(1, pageNumber ?? 1);
        var validPageSize = pageSize ?? DefaultPageSize;

        // Clamp page size between 1 and MaxPageSize
        validPageSize = Math.Clamp(validPageSize, 1, MaxPageSize);

        return (validPageNumber, validPageSize);
    }

    /// <summary>
    /// Calculates the skip count for LINQ Skip() operation.
    /// </summary>
    public static int CalculateSkip(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Calculates the total number of pages.
    /// </summary>
    public static int CalculateTotalPages(int totalCount, int pageSize)
    {
        if (pageSize <= 0)
            return 0;

        return (totalCount + pageSize - 1) / pageSize;
    }

    /// <summary>
    /// Checks if there are more pages after the current page.
    /// </summary>
    public static bool HasNextPage(int pageNumber, int totalPages)
    {
        return pageNumber < totalPages;
    }

    /// <summary>
    /// Checks if there are previous pages before the current page.
    /// </summary>
    public static bool HasPreviousPage(int pageNumber)
    {
        return pageNumber > 1;
    }

    /// <summary>
    /// Gets the page number for the next page.
    /// Returns the current page number if there is no next page.
    /// </summary>
    public static int GetNextPageNumber(int pageNumber, int totalPages)
    {
        return HasNextPage(pageNumber, totalPages) ? pageNumber + 1 : pageNumber;
    }

    /// <summary>
    /// Gets the page number for the previous page.
    /// Returns 1 if already on the first page.
    /// </summary>
    public static int GetPreviousPageNumber(int pageNumber)
    {
        return Math.Max(1, pageNumber - 1);
    }

    /// <summary>
    /// Paginates a collection of items.
    /// </summary>
    public static List<T> Paginate<T>(
        this IEnumerable<T> items,
        int pageNumber = 1,
        int pageSize = DefaultPageSize)
    {
        var (validPageNumber, validPageSize) = ValidatePaginationParams(pageNumber, pageSize);
        var skip = CalculateSkip(validPageNumber, validPageSize);

        return items.Skip(skip).Take(validPageSize).ToList();
    }

    /// <summary>
    /// Gets pagination metadata for a collection.
    /// </summary>
    public static PaginationMetadata GetMetadata(
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var (validPageNumber, validPageSize) = ValidatePaginationParams(pageNumber, pageSize);
        var totalPages = CalculateTotalPages(totalCount, validPageSize);

        return new PaginationMetadata
        {
            PageNumber = validPageNumber,
            PageSize = validPageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = HasNextPage(validPageNumber, totalPages),
            HasPreviousPage = HasPreviousPage(validPageNumber),
            StartIndex = CalculateSkip(validPageNumber, validPageSize) + 1,
            EndIndex = Math.Min(validPageNumber * validPageSize, totalCount)
        };
    }

    /// <summary>
    /// Generates page numbers for pagination UI (e.g., showing 5 pages around the current page).
    /// </summary>
    public static List<int> GetPageNumbers(
        int currentPage,
        int totalPages,
        int pagesToShow = 5)
    {
        var pageNumbers = new List<int>();
        var halfPages = pagesToShow / 2;

        var startPage = Math.Max(1, currentPage - halfPages);
        var endPage = Math.Min(totalPages, startPage + pagesToShow - 1);

        // Adjust start page if we're near the end
        if (endPage - startPage + 1 < pagesToShow)
        {
            startPage = Math.Max(1, endPage - pagesToShow + 1);
        }

        for (int i = startPage; i <= endPage; i++)
        {
            pageNumbers.Add(i);
        }

        return pageNumbers;
    }
}

/// <summary>
/// Metadata about pagination.
/// </summary>
public class PaginationMetadata
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
}
