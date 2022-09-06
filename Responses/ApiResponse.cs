#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Responses;

/// <summary>
/// Standard API response wrapper for all REST endpoints.
/// Provides consistent response format for success and error cases.
/// </summary>
/// <typeparam name="T">The type of data contained in the response</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// The actual data being returned (null for error responses).
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// A human-readable message describing the result or error.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (stack trace, exception message, etc.).
    /// Only populated in error responses.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Unix timestamp of when the response was generated.
    /// </summary>
    public long Timestamp { get; set; } = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

    /// <summary>
    /// Unique identifier for tracking this specific request/response in logs.
    /// </summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
}
