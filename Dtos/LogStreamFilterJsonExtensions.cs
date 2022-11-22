using System.Text.Json;

namespace SystemdServiceMonitor.Dtos;

/// <summary>Provides JSON serialization/deserialization helpers for <see cref="LogStreamFilter"/>.</summary>
public static class LogStreamFilterJsonExtensions
{
    // Cached JsonSerializerOptions with camelCase naming policy.
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="LogStreamFilter"/> instance to serialize.</param>
    /// <param name="indented">If true, the output JSON will be indented.</param>
    /// <returns>A JSON representation of <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this LogStreamFilter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Use a copy of the cached options if indentation is requested.
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="LogStreamFilter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="LogStreamFilter"/> instance, or null if the JSON represents null.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static LogStreamFilter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<LogStreamFilter>(json, _options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="LogStreamFilter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized value if successful; otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out LogStreamFilter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}