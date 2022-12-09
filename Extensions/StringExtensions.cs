using System;
using System.Globalization;
using System.Text;

namespace Extensions
{
    /// <summary>
    /// Provides common extension methods for string operations.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether the specified string is null or empty.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the value is null or empty; otherwise, <see langword="false"/>.</returns>
        public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Determines whether the specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the value is null, empty, or whitespace; otherwise, <see langword="false"/>.</returns>
        public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

        /// <summary>
        /// Trims whitespace from both ends of the string.
        /// Returns null if the input is null.
        /// </summary>
        /// <param name="value">The string to trim.</param>
        /// <returns>The trimmed string, or null if the input was null.</returns>
        public static string? TrimSafe(this string? value)
        {
            return value?.Trim();
        }

        /// <summary>
        /// Trims whitespace from both ends of the string and returns an empty string if the result is null or whitespace.
        /// </summary>
        /// <param name="value">The string to normalize.</param>
        /// <returns>An empty string if the input is null, empty, or whitespace; otherwise the trimmed string.</returns>
        public static string NormalizeEmpty(this string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Converts a string to a boolean value using common representations.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails.</param>
        /// <returns>The boolean value, or <paramref name="defaultValue"/> if conversion fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool ToBoolean(this string value, bool defaultValue = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.Trim().ToLowerInvariant() switch
            {
                "true" or "yes" or "y" or "1" or "on" => true,
                "false" or "no" or "n" or "0" or "off" => false,
                _ => defaultValue
            };
        }

        /// <summary>
        /// Converts a string to an integer using the invariant culture.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails.</param>
        /// <returns>The integer value, or <paramref name="defaultValue"/> if conversion fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static int ToInt32(this string value, int defaultValue = 0)
        {
            ArgumentNullException.ThrowIfNull(value);

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Converts a string to a long using the invariant culture.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails.</param>
        /// <returns>The long value, or <paramref name="defaultValue"/> if conversion fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static long ToInt64(this string value, long defaultValue = 0L)
        {
            ArgumentNullException.ThrowIfNull(value);

            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Converts a string to a double using the invariant culture.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="defaultValue">The value to return if conversion fails.</param>
        /// <returns>The double value, or <paramref name="defaultValue"/> if conversion fails.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static double ToDouble(this string value, double defaultValue = 0.0)
        {
            ArgumentNullException.ThrowIfNull(value);

            return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Indicates whether the specified string is equal to another string using ordinal comparison.
        /// </summary>
        /// <param name="value">The string to compare.</param>
        /// <param name="other">The string to compare with.</param>
        /// <returns><see langword="true"/> if the strings are equal; otherwise, <see langword="false"/>.</returns>
        public static bool EqualsOrdinal(this string value, string? other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        /// <summary>
        /// Indicates whether the specified string is equal to another string using ordinal ignore case comparison.
        /// </summary>
        /// <param name="value">The string to compare.</param>
        /// <param name="other">The string to compare with.</param>
        /// <returns><see langword="true"/> if the strings are equal ignoring case; otherwise, <see langword="false"/>.</returns>
        public static bool EqualsOrdinalIgnoreCase(this string value, string? other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string using ordinal comparison.
        /// </summary>
        /// <param name="value">The string to search in.</param>
        /// <param name="substring">The substring to search for.</param>
        /// <returns><see langword="true"/> if the substring is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool ContainsOrdinal(this string value, string substring)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(substring);

            return value.Contains(substring, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a value indicating whether a specified substring occurs within this string using ordinal ignore case comparison.
        /// </summary>
        /// <param name="value">The string to search in.</param>
        /// <param name="substring">The substring to search for.</param>
        /// <returns><see langword="true"/> if the substring is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static bool ContainsOrdinalIgnoreCase(this string value, string substring)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(substring);

            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced
        /// using ordinal comparison.
        /// </summary>
        /// <param name="value">The original string.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue"/>.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced by <paramref name="newValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> or <paramref name="oldValue"/> is null.</exception>
        public static string ReplaceOrdinal(this string value, string oldValue, string newValue)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(oldValue);

            return value.Replace(oldValue, newValue, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced
        /// using ordinal ignore case comparison.
        /// </summary>
        /// <param name="value">The original string.</param>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace all occurrences of <paramref name="oldValue"/>.</param>
        /// <returns>A string that is equivalent to the current string except that all instances of <paramref name="oldValue"/> are replaced by <paramref name="newValue"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> or <paramref name="oldValue"/> is null.</exception>
        public static string ReplaceOrdinalIgnoreCase(this string value, string oldValue, string newValue)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(oldValue);

            return value.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Truncates the string to the specified maximum length.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the resulting string.</param>
        /// <returns>The truncated string, or the original string if it's shorter than <paramref name="maxLength"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 0.</exception>
        public static string Truncate(this string value, int maxLength)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        /// <summary>
        /// Truncates the string to the specified maximum length and appends an ellipsis if truncated.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the resulting string including the ellipsis.</param>
        /// <returns>The truncated string with ellipsis, or the original string if it's shorter than <paramref name="maxLength"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 0.</exception>
        public static string TruncateWithEllipsis(this string value, int maxLength)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

            if (value.Length <= maxLength)
            {
                return value;
            }

            var length = maxLength - 3; // Account for the ellipsis
            return length > 0 ? value[..length] + "..." : "...";
        }

        /// <summary>
        /// Splits a string into lines, handling both Windows and Unix line endings.
        /// </summary>
        /// <param name="value">The string to split.</param>
        /// <returns>An enumerable of lines.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static IEnumerable<string> ToLines(this string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            using var reader = new System.IO.StringReader(value);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of characters specified in an array from the current string.
        /// </summary>
        /// <param name="value">The string to trim.</param>
        /// <param name="trimChars">An array of Unicode characters to remove.</param>
        /// <returns>The trimmed string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string Trim(this string value, params char[] trimChars)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Trim(trimChars);
        }

        /// <summary>
        /// Indicates whether the string is null, empty, or consists only of ASCII whitespace characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the string is null, empty, or ASCII whitespace; otherwise, <see langword="false"/>.</returns>
        public static bool IsAsciiWhitespace(this string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            foreach (var c in value)
            {
                if (c > ' ')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
