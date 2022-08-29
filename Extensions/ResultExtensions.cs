// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for working with API response results.
/// Provides fluent API for building and transforming responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a value to a successful API response.
    /// </summary>
    public static ApiResponse<T> ToSuccess<T>(this T data, string message = "Operation successful") where T : class
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed API response with error details.
    /// </summary>
    public static ApiResponse<T> ToError<T>(this string message, string? errorDetails = null) where T : class
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            ErrorDetails = errorDetails
        };
    }

    /// <summary>
    /// Converts a collection to a paginated response.
    /// </summary>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> items,
        int pageNumber = 1,
        int pageSize = 50,
        string message = "Retrieved successfully") where T : class
    {
        var itemList = items.ToList();
        var totalCount = itemList.Count;
        var paginatedItems = itemList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<T>
        {
            Data = paginatedItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Maps an API response of one type to another type.
    /// </summary>
    public static ApiResponse<TResult> Map<TSource, TResult>(
        this ApiResponse<TSource> response,
        Func<TSource?, TResult?> mapper) where TSource : class where TResult : class
    {
        return new ApiResponse<TResult>
        {
            Data = response.Data != null ? mapper(response.Data) : null,
            Success = response.Success,
            Message = response.Message,
            ErrorDetails = response.ErrorDetails,
            TraceId = response.TraceId
        };
    }

    /// <summary>
    /// Applies a transformation to response data if successful.
    /// </summary>
    public static ApiResponse<T> OnSuccess<T>(
        this ApiResponse<T> response,
        Action<T> action) where T : class
    {
        if (response.Success && response.Data != null)
        {
            action(response.Data);
        }
        return response;
    }

    /// <summary>
    /// Applies an action if the response failed.
    /// </summary>
    public static ApiResponse<T> OnFailure<T>(
        this ApiResponse<T> response,
        Action<string> action) where T : class
    {
        if (!response.Success)
        {
            action(response.Message);
        }
        return response;
    }

    /// <summary>
    /// Checks if response contains data.
    /// </summary>
    public static bool HasData<T>(this ApiResponse<T> response) where T : class
    {
        return response.Success && response.Data != null;
    }

    /// <summary>
    /// Gets the data or throws an exception if not successful.
    /// </summary>
    public static T GetDataOrThrow<T>(this ApiResponse<T> response) where T : class
    {
        if (!response.Success)
        {
            throw new InvalidOperationException($"Response failed: {response.Message}");
        }

        return response.Data ?? throw new InvalidOperationException("No data in response");
    }

    /// <summary>
    /// Gets the data or returns a default value if not successful.
    /// </summary>
    public static T? GetDataOrDefault<T>(this ApiResponse<T> response, T? defaultValue = null) where T : class
    {
        return response.Success ? response.Data : defaultValue;
    }
}
