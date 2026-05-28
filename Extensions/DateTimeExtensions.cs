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
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        return timeSpan.TotalSeconds < 60
            ? $"{(int)timeSpan.TotalSeconds} seconds ago"
            : timeSpan.TotalMinutes < 60
            ? $"{(int)timeSpan.TotalMinutes} minutes ago"
            : timeSpan.TotalHours < 24
            ? $"{(int)timeSpan.TotalHours} hours ago"
            : timeSpan.TotalDays < 30
            ? $"{(int)timeSpan.TotalDays} days ago"
            : timeSpan.TotalDays < 365
            ? $"{(int)(timeSpan.TotalDays / 30)} months ago"
            : $"{(int)(timeSpan.TotalDays / 365)} years ago";
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
    public static bool IsWithinRange(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        return dateTime >= startDate && dateTime <= endDate;
    }

    /// <summary>
    /// Rounds DateTime to the nearest specified interval.
    /// Useful for grouping log entries or metrics by time buckets.
    /// </summary>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
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
    public static string ToHumanReadableString(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds}s";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h";
        return $"{(int)timeSpan.TotalDays}d";
    }
}
