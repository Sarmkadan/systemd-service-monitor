#nullable enable

using System;
using System.Linq;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Extension methods for <see cref="ServiceLog"/> providing utility functionality
/// for filtering, formatting, and analyzing service log entries.
/// </summary>
/// <remarks>
/// All methods validate input parameters and throw appropriate exceptions for null or invalid values.
/// </remarks>
public static class ServiceLogExtensions
{
    /// <summary>
    /// Determines whether this log entry represents an error or higher severity level.
    /// </summary>
    /// <param name="log">The service log entry to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>True if the log level is Error, Critical, Alert, or Emergency; otherwise false.</returns>
    public static bool IsErrorOrHigher(this ServiceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return log.Level <= SyslogLevel.Error;
    }

    /// <summary>
    /// Determines whether this log entry represents a warning or higher severity level.
    /// </summary>
    /// <param name="log">The service log entry to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>True if the log level is Warning, Error, Critical, Alert, or Emergency; otherwise false.</returns>
    public static bool IsWarningOrHigher(this ServiceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return log.Level <= SyslogLevel.Warning;
    }

    /// <summary>
    /// Gets a formatted string representation of the log entry including all key metadata.
    /// </summary>
    /// <param name="log">The service log entry to format.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>A detailed formatted string with timestamp, unit name, level, and message.</returns>
    public static string ToDetailedString(this ServiceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = log.Level.ToString().ToUpperInvariant();
        var unit = string.IsNullOrEmpty(log.UnitName) ? "unknown" : log.UnitName;

        var result = $"[{timestamp}] [{level}] [{unit}] {log.Message}";

        if (!string.IsNullOrEmpty(log.ErrNo))
        {
            result += $" | ERRNO: {log.ErrNo}";
        }

        if (!string.IsNullOrEmpty(log.MessageId))
        {
            result += $" | MSGID: {log.MessageId}";
        }

        if (log.ProcessId > 0)
        {
            result += $" | PID: {log.ProcessId}";
        }

        if (log.Metadata.Count > 0)
        {
            var metadata = string.Join(", ", log.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            result += $" | METADATA: {{{metadata}}}";
        }

        return result;
    }

    /// <summary>
    /// Checks if this log entry matches the specified severity level.
    /// </summary>
    /// <param name="log">The service log entry to check.</param>
    /// <param name="level">The severity level to match against.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>True if the log level matches the specified level; otherwise false.</returns>
    public static bool HasLevel(this ServiceLog log, SyslogLevel level)
    {
        ArgumentNullException.ThrowIfNull(log);
        return log.Level == level;
    }

    /// <summary>
    /// Gets a short summary string suitable for display in monitoring dashboards.
    /// </summary>
    /// <param name="log">The service log entry to summarize.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>A concise summary string with the most important information.</returns>
    public static string ToSummaryString(this ServiceLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var level = log.Level.ToString().ToUpperInvariant()[..1]; // First letter only
        var unit = string.IsNullOrEmpty(log.UnitName) ? "unknown" : log.UnitName;
        var message = log.Message.Split('\n').FirstOrDefault() ?? log.Message;

        if (message.Length > 100)
        {
            message = message[..97] + "...";
        }

        return $"[{level}] {unit}: {message}";
    }
}