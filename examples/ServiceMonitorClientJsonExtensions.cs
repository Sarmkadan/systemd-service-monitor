#nullable enable

using System;
using System.Text.Json;

namespace SystemdMonitorExamples
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for <see cref="ServiceMonitorClient"/>.
    /// </summary>
    public static class ServiceMonitorClientJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes the <see cref="ServiceMonitorClient"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The client instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>A JSON string representation of the client.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ServiceMonitorClient value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? _jsonOptions with { WriteIndented = true } : _jsonOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ServiceMonitorClient"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A deserialized client instance, or null if deserialization fails.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
        public static ServiceMonitorClient? FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<ServiceMonitorClient>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="ServiceMonitorClient"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized client instance if successful.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
        public static bool TryFromJson(string json, out ServiceMonitorClient? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<ServiceMonitorClient>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}