#nullable enable

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents a log entry from systemd journald for a service.
/// </summary>
public class ServiceLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the associated service.
    /// </summary>
    public Guid ServiceInfoId { get; set; }

    /// <summary>
    /// The service unit name this log entry originates from.
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the log entry (Debug, Info, Warning, Error, Critical).
    /// </summary>
    public SyslogLevel Level { get; set; } = SyslogLevel.Info;

    /// <summary>
    /// The actual log message content.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Process ID that generated this log entry.
    /// </summary>
    public int ProcessId { get; set; } = 0;

    /// <summary>
    /// User ID associated with the process.
    /// </summary>
    public int UserId { get; set; } = 0;

    /// <summary>
    /// System identifier (hostname).
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// The systemd unit name field from journald.
    /// </summary>
    public string CodeFile { get; set; } = string.Empty;

    /// <summary>
    /// Line number in source code where log originated.
    /// </summary>
    public int CodeLine { get; set; } = 0;

    /// <summary>
    /// Function name where log originated.
    /// </summary>
    public string CodeFunction { get; set; } = string.Empty;

    /// <summary>
    /// Systemd-specific error identifier if applicable.
    /// </summary>
    public string? ErrNo { get; set; }

    /// <summary>
    /// Message ID for correlation and filtering.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Monotonic sequence number assigned by journald.
    /// </summary>
    public ulong Sequence { get; set; } = 0;

    /// <summary>
    /// Boot ID to identify logs across reboots.
    /// </summary>
    public string BootId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this log entry was created by the application.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata from the log entry.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];

    public override string ToString() => $"[{Level}] {UnitName}: {Message}";
}

/// <summary>
/// Log severity levels following syslog standards.
/// </summary>
public enum SyslogLevel
{
    Emergency = 0,
    Alert = 1,
    Critical = 2,
    Error = 3,
    Warning = 4,
    Notice = 5,
    Info = 6,
    Debug = 7
}
