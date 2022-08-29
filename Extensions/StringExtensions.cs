// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for string operations.
/// Provides utilities for validation, formatting, and manipulation commonly used in the application.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to the specified maximum length and adds ellipsis if truncated.
    /// </summary>
    public static string Truncate(this string? value, int maxLength = 100, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - ellipsis.Length) + ellipsis;
    }

    /// <summary>
    /// Checks if a string is null, empty, or contains only whitespace.
    /// </summary>
    public static bool IsNullOrWhiteSpaceEx(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Converts a string to Pascal case (UpperCamelCase).
    /// Example: "hello_world" -> "HelloWorld"
    /// </summary>
    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var words = value.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();

        foreach (var word in words)
        {
            if (word.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant());
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a string to camel case (lowerCamelCase).
    /// Example: "hello_world" -> "helloWorld"
    /// </summary>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var pascalCase = value.ToPascalCase();
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase.Substring(1);
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// Example: "HelloWorld" -> "hello_world"
    /// </summary>
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]) && i > 0)
            {
                sb.Append('_');
            }
            sb.Append(char.ToLowerInvariant(value[i]));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Validates that a string matches a valid service name pattern (alphanumeric, hyphen, dot).
    /// </summary>
    public static bool IsValidServiceName(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z0-9._\-]+\.service$|^[a-zA-Z0-9._\-]+$");
    }

    /// <summary>
    /// Sanitizes a string for safe logging by removing or escaping sensitive patterns.
    /// </summary>
    public static string SanitizeForLogging(this string? value, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Remove common sensitive patterns
        var sanitized = Regex.Replace(value, @"(password|token|api[-_]?key|secret)\s*[=:]\s*\S+", "***REDACTED***", RegexOptions.IgnoreCase);

        return sanitized.Truncate(maxLength);
    }

    /// <summary>
    /// Converts a comma-separated string into a list of trimmed values.
    /// </summary>
    public static List<string> ToList(this string? value, char delimiter = ',')
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value.Split(delimiter)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    /// <summary>
    /// Checks if a string contains any of the specified search patterns.
    /// </summary>
    public static bool ContainsAny(this string? value, params string[] searchPatterns)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return searchPatterns.Any(pattern => value.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Repeats a string the specified number of times.
    /// </summary>
    public static string Repeat(this string value, int count)
    {
        if (count <= 0)
            return string.Empty;

        var sb = new StringBuilder(value.Length * count);
        for (int i = 0; i < count; i++)
        {
            sb.Append(value);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Wraps long lines to the specified width.
    /// Useful for formatting log messages or help text.
    /// </summary>
    public static string WrapText(this string text, int maxWidth = 80)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
            return text;

        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            if ((currentLine.Length + word.Length + 1) > maxWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
            }

            if (currentLine.Length > 0)
                currentLine.Append(' ');

            currentLine.Append(word);
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets the Levenshtein distance between two strings (measure of similarity).
    /// Used for fuzzy matching and typo detection.
    /// </summary>
    public static int LevenshteinDistance(this string s1, string s2)
    {
        if (s1.Length == 0)
            return s2.Length;
        if (s2.Length == 0)
            return s1.Length;

        var distances = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            distances[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            distances[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                distances[i, j] = Math.Min(
                    Math.Min(
                        distances[i - 1, j] + 1,
                        distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }

        return distances[s1.Length, s2.Length];
    }
}
