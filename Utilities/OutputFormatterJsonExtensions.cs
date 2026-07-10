using System;
using System.Text.Json;

namespace SystemdServiceMonitor.Utilities
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing data using the same JSON formatting
    /// as the <see cref="OutputFormatter"/> utility class.
    /// </summary>
    public static class OutputFormatterJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };

        /// <summary>
        /// Converts the object to a JSON string representation using the same formatting options
        /// as <see cref="OutputFormatter.FormatAsJson{T}(System.Collections.Generic.IEnumerable{T},bool)"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson<T>(this T value, bool indented = false) where T : class
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? _jsonOptionsIndented : _jsonOptions);
        }

        /// <summary>
        /// Parses a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="json">The JSON string to parse.</param>
        /// <returns>An instance of type T deserialized from the JSON string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized into type T.</exception>
        public static T? FromJson<T>(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to parse a JSON string into an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="value">Receives the deserialized object if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the JSON was successfully parsed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson<T>(string json, out T? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                value = default;
                return false;
            }
        }

        private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };
    }
}