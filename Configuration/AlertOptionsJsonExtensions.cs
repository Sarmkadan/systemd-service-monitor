using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization helpers for <see cref="AlertOptions"/>.
/// </summary>
public static class AlertOptionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the <see cref="AlertOptions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The options to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the options.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this AlertOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AlertOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="AlertOptions"/> instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized.</exception>
    public static AlertOptions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<AlertOptions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an <see cref="AlertOptions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out AlertOptions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<AlertOptions>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}