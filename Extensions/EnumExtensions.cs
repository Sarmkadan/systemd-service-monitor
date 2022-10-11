#nullable enable

using System.ComponentModel;
using System.Reflection;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for enum types.
/// Provides utilities for enum value descriptions, parsing, and conversion.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description attribute of an enum value.
    /// Falls back to the enum value name if no description is defined.
    /// </summary>
    public static string GetDescription<T>(this T enumValue) where T : Enum
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (field is null)
            return enumValue.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? enumValue.ToString();
    }

    /// <summary>
    /// Converts a string to an enum value with optional case-insensitive parsing.
    /// Returns null if the string cannot be converted.
    /// </summary>
    public static T? TryParseEnum<T>(this string value, bool ignoreCase = true) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all values of an enum type.
    /// </summary>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Gets all enum values with their descriptions.
    /// Returns a dictionary mapping descriptions to enum values.
    /// </summary>
    public static Dictionary<string, T> GetValuesWithDescriptions<T>() where T : Enum
    {
        var result = new Dictionary<string, T>();
        foreach (var value in GetValues<T>())
        {
            result[value.GetDescription()] = value;
        }
        return result;
    }

    /// <summary>
    /// Checks if an enum value has a specific flag (for flag enums).
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        return value.HasFlag(flag);
    }

    /// <summary>
    /// Gets the numeric value of an enum.
    /// </summary>
    public static object GetNumericValue<T>(this T enumValue) where T : Enum
    {
        return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(typeof(T)));
    }

    /// <summary>
    /// Converts an enum to a human-readable format.
    /// Example: ServiceState.Active -> "Active", or with spaces: "Service Active"
    /// </summary>
    public static string ToFriendlyString<T>(this T enumValue, bool addSpaces = false) where T : Enum
    {
        var description = enumValue.GetDescription();

        if (!addSpaces)
            return description;

        // Insert spaces before capital letters
        var result = System.Text.RegularExpressions.Regex.Replace(
            description,
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();

        return result;
    }
}
