#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Dtos;

/// <summary>Specifies which log entries the real-time stream should include.</summary>
public sealed class LogStreamFilter
{
    /// <summary>
    /// Restricts the stream to a single systemd unit name.
    /// When <c>null</c> or empty, entries from all monitored services are included.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Case-insensitive substring matched against each log message.
    /// When <c>null</c> or empty, no text filtering is applied.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Lowest syslog severity included in the stream.
    /// Entries whose <see cref="SyslogLevel"/> value is less than or equal to this threshold are emitted.
    /// Lower numeric values represent higher severity (Emergency = 0, Debug = 7).
    /// <c>null</c> disables level filtering entirely.
    /// </summary>
    public SyslogLevel? MinLevel { get; init; }

    /// <summary>
    /// Number of recent log entries replayed from the repository before live tailing begins.
    /// Clamped to [0, 500].
    /// </summary>
    public int BufferSize { get; init; } = 50;

    /// <summary>
    /// Milliseconds between repository polls during live tail.
    /// Clamped to [500, 30 000].
    /// </summary>
    public int PollingIntervalMs { get; init; } = 2000;
}

/// <summary>A single log entry emitted by the real-time stream.</summary>
public sealed class LogStreamEntry
{
    /// <summary>
    /// <c>true</c> when this entry comes from the initial historical buffer;
    /// <c>false</c> when it is a live event observed after streaming started.
    /// </summary>
    public bool IsBuffered { get; init; }

    /// <summary>UTC timestamp recorded by journald when the entry was produced.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>systemd unit name that produced the log line.</summary>
    public string UnitName { get; init; } = string.Empty;

    /// <summary>Syslog severity level of the entry.</summary>
    public SyslogLevel Level { get; init; }

    /// <summary>Human-readable log message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>PID of the process that wrote this entry.</summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Projects a <see cref="ServiceLog"/> repository record into a <see cref="LogStreamEntry"/>.
    /// </summary>
    /// <param name="log">Source repository record.</param>
    /// <param name="isBuffered">
    /// <c>true</c> if the entry belongs to the historical replay buffer;
    /// <c>false</c> if it is a newly observed live event.
    /// </param>
    public static LogStreamEntry FromServiceLog(ServiceLog log, bool isBuffered) => new()
    {
        IsBuffered  = isBuffered,
        Timestamp   = log.Timestamp,
        UnitName    = log.UnitName,
        Level       = log.Level,
        Message     = log.Message,
        ProcessId   = log.ProcessId,
    };
}
