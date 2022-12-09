#nullable enable

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for DateTime operations commonly used in monitoring and logging.
/// Provides utilities for formatting, calculating durations, and time range operations.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Calculates the time elapsed since the given datetime.
    /// Returns a human-readable string like "2 hours ago" or "3 days ago".
    /// </summary>
    /// <param name="dateTime">The reference DateTime to calculate relative time from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is in the future.</exception>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        if (dateTime > DateTime.UtcNow)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime cannot be in the future.");
        }

        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < 60
            ? $"{(int)timeSpan.TotalSeconds} second{(timeSpan.TotalSeconds == 1 ? "" : "s")} ago"
            : timeSpan.TotalMinutes < 60
                ? $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes == 1 ? "" : "s")} ago"
                : timeSpan.TotalHours < 24
                    ? $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours == 1 ? "" : "s")} ago"
                    : timeSpan.TotalDays < 30
                        ? $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays == 1 ? "" : "s")} ago"
                        : timeSpan.TotalDays < 365
                            ? $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays / 30 == 1 ? "" : "s")} ago"
                            : $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays / 365 == 1 ? "" : "s")} ago";
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch).
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts DateTime to Unix timestamp in milliseconds.
    /// </summary>
    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Converts Unix timestamp (seconds) back to DateTime in UTC.
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTime.UnixEpoch.AddSeconds(timestamp);
    }

    /// <summary>
    /// Converts Unix timestamp (milliseconds) back to DateTime in UTC.
    /// </summary>
    public static DateTime FromUnixTimestampMilliseconds(long timestamp)
    {
        return DateTime.UnixEpoch.AddMilliseconds(timestamp);
    }

    /// <summary>
    /// Formats DateTime as ISO 8601 string suitable for API responses.
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("o");
    }

    /// <summary>
    /// Returns true if the given datetime is within the specified time range.
    /// </summary>
    /// <param name="dateTime">The DateTime to check.</param>
    /// <param name="startDate">The start of the time range (inclusive).</param>
    /// <param name="endDate">The end of the time range (inclusive).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startDate"/> is after <paramref name="endDate"/>.</exception>
    public static bool IsWithinRange(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentOutOfRangeException(nameof(startDate), "Start date cannot be after end date.");
        }

        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Rounds DateTime to the nearest specified interval.
    /// Useful for grouping log entries or metrics by time buckets.
    /// </summary>
    /// <param name="dateTime">The DateTime to round.</param>
    /// <param name="interval">The time interval to round to.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="interval"/> is zero or negative.</exception>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");
        }

        var totalTicks = dateTime.Ticks / (double)interval.Ticks;
        var roundedTicks = Math.Round(totalTicks) * interval.Ticks;
        return new DateTime((long)roundedTicks);
    }

    /// <summary>
    /// Returns the start of the day for the given datetime.
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Returns the end of the day for the given datetime.
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Returns the start of the hour for the given datetime.
    /// </summary>
    public static DateTime StartOfHour(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
    }

    /// <summary>
    /// Returns the end of the hour for the given datetime.
    /// </summary>
    public static DateTime EndOfHour(this DateTime dateTime)
    {
        return dateTime.StartOfHour().AddHours(1).AddTicks(-1);
    }

    /// <summary>
    /// Formats a timespan as a human-readable string.
    /// </summary>
    /// <param name="timeSpan">The TimeSpan to format.</param>
    public static string ToHumanReadableString(this TimeSpan timeSpan)
    {
        return timeSpan.TotalSeconds < 60
            ? $"{(int)timeSpan.TotalSeconds}s"
            : timeSpan.TotalMinutes < 60
                ? $"{(int)timeSpan.TotalMinutes}m"
                : timeSpan.TotalHours < 24
                    ? $"{(int)timeSpan.TotalHours}h"
                    : $"{(int)timeSpan.TotalDays}d";
    }
}