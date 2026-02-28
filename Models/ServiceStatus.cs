#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents the current status snapshot of a service at a point in time.
/// </summary>
public class ServiceStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent ServiceInfo.
    /// </summary>
    public Guid ServiceInfoId { get; set; }

    /// <summary>
    /// Name of the service unit.
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// Current operational state.
    /// </summary>
    public ServiceState State { get; set; } = ServiceState.Unknown;

    /// <summary>
    /// Detailed sub-state.
    /// </summary>
    public ServiceSubState SubState { get; set; } = ServiceSubState.Unknown;

    /// <summary>
    /// True if the service is enabled in systemd targets.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// True if the service is currently running.
    /// </summary>
    public bool IsRunning { get; set; } = false;

    /// <summary>
    /// The PID of the main service process.
    /// </summary>
    public int ProcessId { get; set; } = 0;

    /// <summary>
    /// CPU usage percentage (0-100).
    /// </summary>
    public decimal CpuUsagePercent { get; set; } = 0;

    /// <summary>
    /// Memory usage in MB.
    /// </summary>
    public long MemoryUsageMb { get; set; } = 0;

    /// <summary>
    /// Indicates whether the service has failed.
    /// </summary>
    public bool HasFailed { get; set; } = false;

    /// <summary>
    /// Description of any failure condition.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Exit code from the last execution (if applicable).
    /// </summary>
    public int ExitCode { get; set; } = 0;

    /// <summary>
    /// Seconds since the service was last started.
    /// </summary>
    public long UptimeSeconds { get; set; } = 0;

    /// <summary>
    /// Health check status (Healthy, Degraded, Unhealthy).
    /// </summary>
    public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;

    /// <summary>
    /// Message explaining the health status.
    /// </summary>
    public string HealthMessage { get; set; } = string.Empty;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Health status of a service based on health checks and monitoring.
/// </summary>
public enum HealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Degraded = 2,
    Unhealthy = 3
}
