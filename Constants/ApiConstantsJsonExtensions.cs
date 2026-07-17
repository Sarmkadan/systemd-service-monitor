using System;
using System.Text.Json;

namespace SystemdServiceMonitor.Constants;

/// <summary>
/// Provides JSON serialization and deserialization helpers for API constants.
/// </summary>
public static class ApiConstantsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static JsonSerializerOptions JsonSerializerOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the ApiConstants to a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representing the API constants.</returns>
    public static string ToJson(bool indented = false)
    {
        var options = indented ? Options : JsonSerializerOptions();

        var constants = new
        {
            ApiVersion,
            DefaultPageSize,
            MaxPageSize,
            DefaultRateLimit,
            DefaultCacheTtlSeconds,
            MaxLogLines,
            DefaultLogLines
        };
        return JsonSerializer.Serialize(constants, options);
    }

    /// <summary>
    /// Deserializes JSON string to a strongly-typed object representing ApiConstants structure.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A strongly-typed object containing the API constants.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static ApiConstantsDto? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<ApiConstantsDto>(json, Options);
    }

    /// <summary>
    /// Attempts to deserialize JSON string to a strongly-typed object representing ApiConstants structure.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized object if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out ApiConstantsDto? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<ApiConstantsDto>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Data Transfer Object for API constants serialization.
    /// </summary>
    public sealed class ApiConstantsDto
    {
        public string ApiVersion { get; set; } = string.Empty;
        public int DefaultPageSize { get; set; }
        public int MaxPageSize { get; set; }
        public int DefaultRateLimit { get; set; }
        public int DefaultCacheTtlSeconds { get; set; }
        public int MaxLogLines { get; set; }
        public int DefaultLogLines { get; set; }
    }

    public static string ApiVersion => "1.0";
    public static int DefaultPageSize => 10;
    public static int MaxPageSize => 100;
    public static int DefaultRateLimit => 10;
    public static int DefaultCacheTtlSeconds => 60;
    public static int MaxLogLines => 1000;
    public static int DefaultLogLines => 100;
}
