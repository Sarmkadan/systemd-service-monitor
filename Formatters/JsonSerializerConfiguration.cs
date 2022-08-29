// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace SystemdServiceMonitor.Formatters;

/// <summary>
/// Centralized JSON serialization configuration.
/// Ensures consistent JSON formatting across the entire application.
/// </summary>
public static class JsonSerializerConfiguration
{
    /// <summary>
    /// Gets the standard JSON serializer options for API responses.
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return options;
    }

    /// <summary>
    /// Gets compact JSON serializer options (no indentation).
    /// </summary>
    public static JsonSerializerOptions GetCompactOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        return options;
    }

    /// <summary>
    /// Gets verbose JSON serializer options (with all details).
    /// </summary>
    public static JsonSerializerOptions GetVerboseOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        return options;
    }

    /// <summary>
    /// Serializes an object to JSON string using default options.
    /// </summary>
    public static string Serialize<T>(T obj) where T : class
    {
        return JsonSerializer.Serialize(obj, GetDefaultOptions());
    }

    /// <summary>
    /// Serializes an object to JSON string with custom options.
    /// </summary>
    public static string Serialize<T>(T obj, JsonSerializerOptions options) where T : class
    {
        return JsonSerializer.Serialize(obj, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    public static T? Deserialize<T>(string json) where T : class
    {
        return JsonSerializer.Deserialize<T>(json, GetDefaultOptions());
    }

    /// <summary>
    /// Deserializes a JSON string with custom options.
    /// </summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions options) where T : class
    {
        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Custom JSON converter for DateTime that handles ISO 8601 format with proper UTC handling.
    /// </summary>
    public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && reader.TryGetDateTime(out var dateTime))
            {
                return dateTime.ToUniversalTime();
            }

            throw new JsonException("Invalid datetime format");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString("o"));
        }
    }

    /// <summary>
    /// Custom JSON converter for TimeSpan.
    /// Serializes TimeSpan as ISO 8601 duration string.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeSpan.Parse(value ?? "00:00:00");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(XmlConvert.ToString(value));
        }
    }

    /// <summary>
    /// Registers custom converters with JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions WithCustomConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new UtcDateTimeJsonConverter());
        options.Converters.Add(new TimeSpanJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
