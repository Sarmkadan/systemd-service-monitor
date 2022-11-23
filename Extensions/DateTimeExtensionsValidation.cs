#nullable enable

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Provides validation methods for DateTime operations to ensure values are valid
/// before performing operations that could produce incorrect results.
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates that a DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if dateTime is null (when passed as nullable).</exception>
    public static IReadOnlyList<string> Validate(this DateTime? dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);

        return Validate(dateTime.Value);
    }

    /// <summary>
    /// Validates that a DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        var problems = new List<string>();

        // Validate ToRelativeTime - should not be null or empty
        var relativeTime = dateTime.ToRelativeTime();
        if (string.IsNullOrWhiteSpace(relativeTime))
        {
            problems.Add("ToRelativeTime() returned null or empty string");
        }

        // Validate ToUnixTimestamp - should produce reasonable Unix timestamp
        try
        {
            var unixTimestamp = dateTime.ToUnixTimestamp();
            if (unixTimestamp < 0)
            {
                problems.Add("ToUnixTimestamp() returned negative value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToUnixTimestamp() threw exception: {ex.Message}");
        }

        // Validate ToUnixTimestampMilliseconds - should produce reasonable Unix timestamp in ms
        try
        {
            var unixTimestampMs = dateTime.ToUnixTimestampMilliseconds();
            if (unixTimestampMs < 0)
            {
                problems.Add("ToUnixTimestampMilliseconds() returned negative value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToUnixTimestampMilliseconds() threw exception: {ex.Message}");
        }

        // Validate ToIso8601String - should produce valid ISO 8601 format
        var isoString = dateTime.ToIso8601String();
        if (string.IsNullOrWhiteSpace(isoString))
        {
            problems.Add("ToIso8601String() returned null or empty string");
        }
        else if (!DateTime.TryParse(isoString, out _))
        {
            problems.Add("ToIso8601String() returned invalid ISO 8601 format");
        }

        // Validate IsWithinRange - should work correctly with valid ranges
        try
        {
            var isWithinRange = dateTime.IsWithinRange(DateTime.MinValue, DateTime.MaxValue);
            // This should always be true for valid dates
            if (!isWithinRange)
            {
                problems.Add("IsWithinRange() returned false for valid date range");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsWithinRange() threw exception: {ex.Message}");
        }

        // Validate RoundToNearest - should not throw and should return DateTime
        try
        {
            var rounded = dateTime.RoundToNearest(TimeSpan.FromHours(1));
            if (rounded == default)
            {
                problems.Add("RoundToNearest() returned default DateTime");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"RoundToNearest() threw exception: {ex.Message}");
        }

        // Validate StartOfDay - should return valid date
        try
        {
            var startOfDay = dateTime.StartOfDay();
            if (startOfDay == default)
            {
                problems.Add("StartOfDay() returned default DateTime");
            }
            else if (startOfDay.TimeOfDay != TimeSpan.Zero)
            {
                problems.Add("StartOfDay() did not return start of day");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"StartOfDay() threw exception: {ex.Message}");
        }

        // Validate EndOfDay - should return valid date
        try
        {
            var endOfDay = dateTime.EndOfDay();
            if (endOfDay == default)
            {
                problems.Add("EndOfDay() returned default DateTime");
            }
            else if (endOfDay.TimeOfDay.Ticks != TimeSpan.TicksPerDay - 1)
            {
                problems.Add("EndOfDay() did not return end of day");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"EndOfDay() threw exception: {ex.Message}");
        }

        // Validate StartOfHour - should return valid date
        try
        {
            var startOfHour = dateTime.StartOfHour();
            if (startOfHour == default)
            {
                problems.Add("StartOfHour() returned default DateTime");
            }
            else if (startOfHour.Minute != 0 || startOfHour.Second != 0 || startOfHour.Millisecond != 0)
            {
                problems.Add("StartOfHour() did not return start of hour");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"StartOfHour() threw exception: {ex.Message}");
        }

        // Validate EndOfHour - should return valid date
        try
        {
            var endOfHour = dateTime.EndOfHour();
            if (endOfHour == default)
            {
                problems.Add("EndOfHour() returned default DateTime");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"EndOfHour() threw exception: {ex.Message}");
        }

        // Note: ToHumanReadableString is an extension method on TimeSpan, not DateTime
        // It is not a member of DateTimeExtensions class, so we skip validation for it

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <returns>True if the DateTime is valid; otherwise, false.</returns>
    public static bool IsValid(this DateTime dateTime) => Validate(dateTime).Count == 0;

    /// <summary>
    /// Determines whether the specified nullable DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime to check.</param>
    /// <returns>True if the DateTime is valid or null; otherwise, false.</returns>
    public static bool IsValid(this DateTime? dateTime) => dateTime == null || Validate(dateTime.Value).Count == 0;

    /// <summary>
    /// Ensures that the specified DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The DateTime to validate.</param>
    /// <exception cref="ArgumentException">Thrown if dateTime contains invalid values.</exception>
    public static void EnsureValid(this DateTime dateTime)
    {
        var problems = Validate(dateTime);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"DateTime contains invalid values. Problems: {string.Join(", ", problems)}");
    }

    /// <summary>
    /// Ensures that the specified nullable DateTime contains valid values for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="dateTime">The nullable DateTime to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if dateTime is null.</exception>
    /// <exception cref="ArgumentException">Thrown if dateTime contains invalid values.</exception>
    public static void EnsureValid(this DateTime? dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        EnsureValid(dateTime.Value);
    }
}