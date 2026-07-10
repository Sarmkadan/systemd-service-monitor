using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides JSON serialization and deserialization helpers for RestartPolicyConfig.
/// </summary>
public static class RestartPolicyConfigJsonExtensions
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes a RestartPolicyConfig to a JSON string.
    /// </summary>
    /// <param name="value">The RestartPolicyConfig instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the RestartPolicyConfig.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this RestartPolicyConfig value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(Options) { WriteIndented = true }
            : Options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a RestartPolicyConfig instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A RestartPolicyConfig instance, or null if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static RestartPolicyConfig? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<RestartPolicyConfig>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a RestartPolicyConfig instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized RestartPolicyConfig instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out RestartPolicyConfig? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<RestartPolicyConfig>(json, Options);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}