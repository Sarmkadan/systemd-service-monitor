#nullable enable

namespace SystemdServiceMonitor.Responses;

/// <summary>
/// Provides validation helpers for <see cref="ApiResponse{T}"/> instances.
/// </summary>
public static class ApiResponseValidation
{
    /// <summary>
    /// Validates an <see cref="ApiResponse{T}"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The response to validate.</param>
    /// <returns>An enumerable of validation problem descriptions, or an empty list if the response is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this ApiResponse<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Success
        if (!value.Success)
        {
            problems.Add("Response indicates failure (Success = false) but no error details provided.");
        }

        // Validate Message
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message is null, empty, or whitespace.");
        }

        // Validate ErrorDetails
        if (value.Success && !string.IsNullOrEmpty(value.ErrorDetails))
        {
            problems.Add("ErrorDetails should be null or empty for successful responses.");
        }

        if (!value.Success && string.IsNullOrEmpty(value.ErrorDetails))
        {
            problems.Add("ErrorDetails is null or empty for a failed response.");
        }

        // Validate Timestamp (should be a reasonable Unix timestamp)
        if (value.Timestamp <= 0)
        {
            problems.Add("Timestamp is not a valid Unix timestamp (must be positive).");
        }

        // Validate TraceId
        if (string.IsNullOrWhiteSpace(value.TraceId))
        {
            problems.Add("TraceId is null, empty, or whitespace.");
        }
        else if (value.TraceId.Length < 32)
        {
            problems.Add("TraceId is suspiciously short (less than 32 characters).");
        }

        // Validate Data based on Success flag
        if (!value.Success && value.Data != null)
        {
            problems.Add("Data should be null for failed responses.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ApiResponse{T}"/> instance is valid.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The response to check.</param>
    /// <returns>True if the response is valid; otherwise, false.</returns>
    public static bool IsValid<T>(this ApiResponse<T> value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ApiResponse{T}"/> instance is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed error message if it is not.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The response to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the response is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid<T>(this ApiResponse<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ApiResponse is invalid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}