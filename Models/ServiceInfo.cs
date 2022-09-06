#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents detailed information about a systemd service unit.
/// </summary>
public class ServiceInfo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Unique identifier of the service within systemd.
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the service.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The path to the service unit configuration file.
    /// </summary>
    public string UnitFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the service (active, inactive, failed, etc.).
    /// </summary>
    public ServiceState State { get; set; } = ServiceState.Unknown;

    /// <summary>
    /// Detailed sub-state information about the service.
    /// </summary>
    public ServiceSubState SubState { get; set; } = ServiceSubState.Unknown;

    /// <summary>
    /// Process ID of the main service process (0 if not running).
    /// </summary>
    public int MainProcessId { get; set; } = 0;

    /// <summary>
    /// Result of the last execution (success, exit code, signal, etc.).
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// The restart policy applied when the service terminates.
    /// </summary>
    public RestartPolicy RestartPolicy { get; set; } = RestartPolicy.No;

    /// <summary>
    /// Whether the service should be automatically started on boot.
    /// </summary>
    public bool AutoStart { get; set; } = false;

    /// <summary>
    /// Whether systemd should restart the service when it exits unexpectedly.
    /// </summary>
    public bool Restart { get; set; } = false;

    /// <summary>
    /// List of units that must be started before this service.
    /// </summary>
    public List<string> Dependencies { get; set; } = [];

    /// <summary>
    /// List of units that depend on this service.
    /// </summary>
    public List<string> Dependents { get; set; } = [];

    /// <summary>
    /// Timestamp when the service was last started.
    /// </summary>
    public DateTime? LastStartTime { get; set; }

    /// <summary>
    /// Timestamp when the service was last stopped.
    /// </summary>
    public DateTime? LastStopTime { get; set; }

    /// <summary>
    /// Current uptime of the service in seconds (0 if not running).
    /// </summary>
    public long UptimeSeconds { get; set; } = 0;

    /// <summary>
    /// Number of times the service has been restarted.
    /// </summary>
    public int RestartCount { get; set; } = 0;

    /// <summary>
    /// Working directory of the service process.
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;

    /// <summary>
    /// User account under which the service runs.
    /// </summary>
    public string RunAsUser { get; set; } = string.Empty;

    /// <summary>
    /// Group account under which the service runs.
    /// </summary>
    public string RunAsGroup { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() => $"{UnitName} ({State})";
}
