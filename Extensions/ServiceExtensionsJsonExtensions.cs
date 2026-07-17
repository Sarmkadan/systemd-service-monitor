#nullable enable

using System;
using System.Text.Json;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Provides System.Text.Json serialization extensions for service configuration types.
/// Note: ServiceExtensions is a static class and cannot be directly serialized.
/// This class provides JSON helpers for service configuration patterns used with ServiceExtensions.
/// </summary>
public static class ServiceExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to a JSON string with camelCase property naming.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a dictionary with string keys.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A dictionary, or null if the JSON is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static System.Collections.Generic.Dictionary<string, object?>? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a dictionary.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value if successful; otherwise null.</param>
    /// <returns>True if deserialization succeeds; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out System.Collections.Generic.Dictionary<string, object?>? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}