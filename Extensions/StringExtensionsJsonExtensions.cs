using System.Text.Json;

namespace Extensions
{
    /// <summary>
    /// Provides JSON serialization/deserialization helpers for <see cref="StringExtensions"/>.
    /// </summary>
    public static class StringExtensionsJsonExtensions
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
        /// <param name="value">The <see cref="StringExtensions"/> instance to serialize.</param>
        /// <param name="indented">If true, the output JSON will be indented.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        public static string ToJson(this StringExtensions value, bool indented = false)
        {
            // Use a copy of the cached options if indentation is requested.
            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="StringExtensions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="StringExtensions"/> instance, or null if the JSON represents null.</returns>
        public static StringExtensions? FromJson(string json)
        {
            return JsonSerializer.Deserialize<StringExtensions>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="StringExtensions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized value if successful; otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise false.</returns>
        public static bool TryFromJson(string json, out StringExtensions? value)
        {
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
}
