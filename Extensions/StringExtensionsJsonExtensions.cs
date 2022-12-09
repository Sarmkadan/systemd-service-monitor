using System;
using System.Text.Json;

namespace Extensions
{
    /// <summary>
    /// Provides JSON serialization and deserialization utilities for common .NET types.
    /// </summary>
    public static class StringExtensionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions _optionsIndented = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
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

            var options = indented ? _optionsIndented : _options;
            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a dictionary with string keys.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A dictionary, or null if JSON is null or empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="JsonException">Thrown when JSON is invalid.</exception>
        public static System.Collections.Generic.Dictionary<string, object?>? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a dictionary.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized value if successful.</param>
        /// <returns>True if deserialization succeeds; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out System.Collections.Generic.Dictionary<string, object?>? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            value = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                value = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object?>>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}