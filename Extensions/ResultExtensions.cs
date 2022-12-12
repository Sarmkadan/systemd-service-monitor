#nullable enable

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
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="data">The data to wrap in a successful response</param>
    /// <param name="message">Optional success message (defaults to "Operation successful")</param>
    /// <returns>A successful <see cref="ApiResponse{T}"/> with the provided data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/></exception>
    public static ApiResponse<T> ToSuccess<T>(this T data, string message = "Operation successful") where T : class
    {
        ArgumentNullException.ThrowIfNull(data);

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
    /// <typeparam name="T">The expected data type for the response</typeparam>
    /// <param name="message">Error message describing the failure</param>
    /// <param name="errorDetails">Optional additional error details</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/> with error information</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/></exception>
    public static ApiResponse<T> ToError<T>(this string message, string? errorDetails = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(message);

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
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="items">The collection to paginate</param>
    /// <param name="pageNumber">The 1-based page number (defaults to 1)</param>
    /// <param name="pageSize">Number of items per page (defaults to 50)</param>
    /// <param name="message">Success message for the response</param>
    /// <returns>A <see cref="PaginatedResponse{T}"/> with pagination metadata</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than 1</exception>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> items,
        int pageNumber = 1,
        int pageSize = 50,
        string message = "Retrieved successfully") where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

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
    /// <typeparam name="TSource">The source data type</typeparam>
    /// <typeparam name="TResult">The result data type</typeparam>
    /// <param name="response">The source response to map</param>
    /// <param name="mapper">Function to transform the data</param>
    /// <returns>A new <see cref="ApiResponse{TResult}"/> with mapped data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="mapper"/> is <see langword="null"/></exception>
    public static ApiResponse<TResult> Map<TSource, TResult>(
        this ApiResponse<TSource> response,
        Func<TSource?, TResult?> mapper) where TSource : class where TResult : class
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(mapper);

        return new ApiResponse<TResult>
        {
            Data = response.Data is not null ? mapper(response.Data) : null,
            Success = response.Success,
            Message = response.Message,
            ErrorDetails = response.ErrorDetails,
            TraceId = response.TraceId
        };
    }

    /// <summary>
    /// Applies a transformation to response data if successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="response">The response to process</param>
    /// <param name="action">Action to apply to the data if successful</param>
    /// <returns>The original response for fluent chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="action"/> is <see langword="null"/></exception>
    public static ApiResponse<T> OnSuccess<T>(
        this ApiResponse<T> response,
        Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(action);

        if (response.Success && response.Data is not null)
        {
            action(response.Data);
        }
        return response;
    }

    /// <summary>
    /// Applies an action if the response failed.
    /// </summary>
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="response">The response to process</param>
    /// <param name="action">Action to apply with the error message</param>
    /// <returns>The original response for fluent chaining</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="action"/> is <see langword="null"/></exception>
    public static ApiResponse<T> OnFailure<T>(
        this ApiResponse<T> response,
        Action<string> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(action);

        if (!response.Success)
        {
            action(response.Message);
        }
        return response;
    }

    /// <summary>
    /// Checks if response contains data.
    /// </summary>
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="response">The response to check</param>
    /// <returns><see langword="true"/> if the response is successful and contains data; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static bool HasData<T>(this ApiResponse<T> response) where T : class
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.Success && response.Data is not null;
    }

    /// <summary>
    /// Gets the data or throws an exception if not successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="response">The response to process</param>
    /// <returns>The data from the response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="InvalidOperationException">Response is not successful or contains no data</exception>
    public static T GetDataOrThrow<T>(this ApiResponse<T> response) where T : class
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.Success)
        {
            throw new InvalidOperationException($"Response failed: {response.Message}");
        }

        return response.Data ?? throw new InvalidOperationException("No data in response");
    }

    /// <summary>
    /// Gets the data or returns a default value if not successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the response</typeparam>
    /// <param name="response">The response to process</param>
    /// <param name="defaultValue">Optional default value to return on failure</param>
    /// <returns>The data if successful, otherwise the default value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    public static T? GetDataOrDefault<T>(this ApiResponse<T> response, T? defaultValue = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(response);

        return response.Success ? response.Data : defaultValue;
    }
}
