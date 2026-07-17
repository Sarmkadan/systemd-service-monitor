#nullable enable

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Provides validation methods for <see cref="DateTime"/> values to ensure they are valid
/// for use in monitoring and logging operations.
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates that a <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate. Must not be <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dateTime"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DateTime? dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        return Validate(dateTime.Value);
    }

    /// <summary>
    /// Validates that a <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate. Must not be <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        var problems = new List<string>();

        if (dateTime == DateTime.MinValue)
        {
            problems.Add("DateTime.MinValue is not a valid value for monitoring operations");
        }
        else if (dateTime == DateTime.MaxValue)
        {
            problems.Add("DateTime.MaxValue is not a valid value for monitoring operations");
        }
        else if (dateTime.Kind == DateTimeKind.Local)
        {
            problems.Add("DateTime should be in UTC or Unspecified kind for consistent behavior");
        }
        else if (dateTime.Year < 2000 || dateTime.Year > 2100)
        {
            problems.Add("DateTime year is outside reasonable range (2000-2100)");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the DateTime is valid; otherwise, false.</returns>
    public static bool IsValid(this DateTime dateTime) => Validate(dateTime).Count == 0;

    /// <summary>
    /// Determines whether the specified nullable <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime to check.</param>
    /// <returns>True if the DateTime is valid or null; otherwise, false.</returns>
    public static bool IsValid(this DateTime? dateTime) => dateTime == null || Validate(dateTime.Value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dateTime"/> contains invalid values.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dateTime"/> is null.</exception>
    public static void EnsureValid(this DateTime? dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        EnsureValid(dateTime.Value);
    }

    /// <summary>
    /// Ensures that the specified <see cref="DateTime"/> contains valid values for use with DateTime operations.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="dateTime"/> contains invalid values.
    /// The exception message includes all validation problems found.</exception>
    public static void EnsureValid(this DateTime dateTime)
    {
        var problems = Validate(dateTime);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime contains invalid values. Problems: {string.Join(", ", problems)}");
        }
    }
}