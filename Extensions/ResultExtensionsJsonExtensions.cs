#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Provides System.Text.Json serialization extensions for API response types used by ResultExtensions.
/// </summary>
public static class ResultExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes an <see cref="ApiResponse{T}"/> instance to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="value">The API response to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the API response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<T>(this ApiResponse<T> value, bool indented = false) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Serializes a <see cref="PaginatedResponse{T}"/> instance to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated response.</typeparam>
    /// <param name="value">The paginated response to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the paginated response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson<T>(this PaginatedResponse<T> value, bool indented = false) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="ApiResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of data expected in the response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized API response instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ApiResponse<T>? FromJson<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PaginatedResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of items expected in the paginated response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized paginated response instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PaginatedResponse<T>? FromJsonPaginated<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PaginatedResponse<T>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="ApiResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of data expected in the response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized API response instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson<T>(string json, out ApiResponse<T>? value) where T : class
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PaginatedResponse{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of items expected in the paginated response.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized paginated response instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJsonPaginated<T>(string json, out PaginatedResponse<T>? value) where T : class
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<PaginatedResponse<T>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}