// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Enums;

/// <summary>
/// Represents the state of a systemd service unit.
/// </summary>
public enum ServiceState
{
    Active,
    Inactive,
    Activating,
    Deactivating,
    Failed,
    Reloading,
    Unknown
}

/// <summary>
/// Defines the restart policy for a service.
/// </summary>
public enum RestartPolicy
{
    No,
    OnSuccess,
    OnFailure,
    OnAbnormal,
    OnWatchdog,
    OnExitStatus,
    Always
}

/// <summary>
/// Represents the sub-state of a systemd service.
/// </summary>
public enum ServiceSubState
{
    Dead,
    Start,
    Running,
    Exited,
    Waiting,
    Elapsed,
    Condition,
    Failed,
    Listening,
    Stopped,
    Unknown
}

/// <summary>
/// Restart strategy when a service operation is triggered.
/// </summary>
public enum RestartStrategy
{
    /// <summary>
    /// Stop the service immediately without waiting for dependent units.
    /// </summary>
    Immediate,

    /// <summary>
    /// Gracefully stop dependent units first.
    /// </summary>
    Graceful,

    /// <summary>
    /// Restart with minimal downtime.
    /// </summary>
    RollingRestart
}
