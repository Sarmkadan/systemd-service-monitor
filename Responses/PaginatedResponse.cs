// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Responses;

/// <summary>
/// API response wrapper for paginated data.
/// Extends ApiResponse with pagination metadata for large result sets.
/// </summary>
/// <typeparam name="T">The type of items in the paginated list</typeparam>
public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Calculated total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Indicates whether there are more pages available.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Indicates whether there are previous pages available.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// The index of the first item on the current page (1-based).
    /// </summary>
    public int StartIndex => (PageNumber - 1) * PageSize + 1;

    /// <summary>
    /// The index of the last item on the current page.
    /// </summary>
    public int EndIndex => Math.Min(PageNumber * PageSize, TotalCount);
}
