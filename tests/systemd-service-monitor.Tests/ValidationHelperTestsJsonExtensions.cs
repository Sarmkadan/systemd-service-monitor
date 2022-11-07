#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemdServiceMonitor.Tests;

/// <summary>
/// JSON serialization/deserialization extensions for ValidationHelperTests.
/// </summary>
public static class ValidationHelperTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the ValidationHelperTests instance to a JSON string.
    /// </summary>
    /// <param name="value">The ValidationHelperTests instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ValidationHelperTests instance.</returns>
    public static string ToJson(this ValidationHelperTests value, bool indented = false)
    {
        if (value is null)
        {
            return "null";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ValidationHelperTests instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized ValidationHelperTests instance, or null if the JSON is null or empty.</returns>
    public static ValidationHelperTests? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return null;
        }

        return JsonSerializer.Deserialize<ValidationHelperTests>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ValidationHelperTests instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized ValidationHelperTests instance if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ValidationHelperTests? value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json) || json == "null")
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<ValidationHelperTests>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}