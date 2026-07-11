#nullable enable

using System.ComponentModel;
using System.Reflection;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Provides extension methods for enum types.
/// Includes utilities for enum value descriptions, parsing, conversion, and flag checking.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description attribute of an enum value.
    /// Falls back to the enum value name if no description is defined.
    /// </summary>
    /// <param name="enumValue">The enum value to get the description for.</param>
    /// <returns>The description attribute value if defined; otherwise, the enum value name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumValue"/> is null.</exception>
    public static string GetDescription<T>(this T enumValue) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        var field = enumValue.GetType().GetField(enumValue.ToString());
        return field is null
            ? enumValue.ToString()
            : field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? enumValue.ToString();
    }

    /// <summary>
    /// Converts a string to an enum value with optional case-insensitive parsing.
    /// Returns null if the string cannot be converted.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="ignoreCase">Whether to perform case-insensitive parsing.</param>
    /// <returns>The parsed enum value, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T? TryParseEnum<T>(this string value, bool ignoreCase = true) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);

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
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>An enumerable of all enum values.</returns>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is not an enum type.</exception>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Gets all enum values with their descriptions.
    /// Returns a dictionary mapping descriptions to enum values.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>A dictionary mapping descriptions to enum values.</returns>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is not an enum type.</exception>
    /// <exception cref="ArgumentNullException">Thrown when any enum value is null.</exception>
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
    /// Checks if an enum value has a specific flag set (for flag enums).
    /// </summary>
    /// <param name="value">The enum value to check.</param>
    /// <param name="flag">The flag to check for.</param>
    /// <returns>True if the flag is set; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> or <paramref name="flag"/> is null.</exception>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(flag);

        return value.HasFlag(flag);
    }

    /// <summary>
    /// Gets the numeric value of an enum.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="enumValue">The enum value to convert.</param>
    /// <returns>The numeric representation of the enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumValue"/> is null.</exception>
    /// <exception cref="OverflowException">Thrown when the enum value cannot be represented as the underlying type.</exception>
    public static object GetNumericValue<T>(this T enumValue) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        return Convert.ChangeType(enumValue, underlyingType);
    }

    /// <summary>
    /// Converts an enum to a human-readable format.
    /// Example: ServiceState.Active -> "Active", or with spaces: "Service Active"
    /// </summary>
    /// <param name="enumValue">The enum value to convert.</param>
    /// <param name="addSpaces">Whether to add spaces before capital letters.</param>
    /// <returns>A human-readable string representation of the enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumValue"/> is null.</exception>
    public static string ToFriendlyString<T>(this T enumValue, bool addSpaces = false) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        var description = enumValue.GetDescription();

        return !addSpaces
            ? description
            : System.Text.RegularExpressions.Regex.Replace(
                description,
                "([A-Z])",
                " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
}