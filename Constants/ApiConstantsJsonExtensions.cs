using System;
using System.Text.Json;

namespace SystemdServiceMonitor.Constants;

/// <summary>
/// Provides JSON serialization and deserialization helpers for ApiConstants.
/// </summary>
public static class ApiConstantsJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes the ApiConstants to a JSON string.
    /// Note: Due to ApiConstants being a static class, this serializes a representative object.
    /// </summary>
    public static string ToJson(bool indented = false)
    {
        var options = indented ? Options : new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        var constants = new
        {
            ApiConstants.ApiVersion,
            ApiConstants.DefaultPageSize,
            ApiConstants.MaxPageSize,
            ApiConstants.DefaultRateLimit,
            ApiConstants.DefaultCacheTtlSeconds,
            ApiConstants.MaxLogLines,
            ApiConstants.DefaultLogLines
        };
        return JsonSerializer.Serialize(constants, options);
    }

    /// <summary>
    /// Deserializes JSON string to a dynamic object representing ApiConstants structure.
    /// </summary>
    public static dynamic? FromJson(string json)
    {
        return JsonSerializer.Deserialize<dynamic>(json, Options);
    }

    /// <summary>
    /// Tries to deserialize JSON string to a dynamic object representing ApiConstants structure.
    /// </summary>
    public static bool TryFromJson(string json, out dynamic? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<dynamic>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
