#nullable enable

using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Detailed restart policy configuration for a service.
/// </summary>
public class RestartPolicyConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent ServiceInfo.
    /// </summary>
    public Guid ServiceInfoId { get; set; }

    /// <summary>
    /// The restart policy type (No, OnSuccess, OnFailure, etc.).
    /// </summary>
    public RestartPolicy PolicyType { get; set; } = RestartPolicy.No;

    /// <summary>
    /// Delay in seconds before restarting the service.
    /// </summary>
    public int RestartDelaySec { get; set; } = 0;

    /// <summary>
    /// Maximum number of restart attempts.
    /// </summary>
    public int MaxRestarts { get; set; } = -1; // -1 means unlimited

    /// <summary>
    /// Time window in seconds for counting restart attempts.
    /// </summary>
    public int RestartWindowSec { get; set; } = 60;

    /// <summary>
    /// Start limit interval for burst restart protection.
    /// </summary>
    public int StartLimitIntervalSec { get; set; } = 10;

    /// <summary>
    /// Maximum number of starts within the start limit interval.
    /// </summary>
    public int StartLimitBurst { get; set; } = 5;

    /// <summary>
    /// Timeout for the ExecStart command in seconds.
    /// </summary>
    public int TimeoutStartSec { get; set; } = 90;

    /// <summary>
    /// Timeout for the ExecStop command in seconds.
    /// </summary>
    public int TimeoutStopSec { get; set; } = 90;

    /// <summary>
    /// Restart strategy when restarting (Immediate, Graceful, RollingRestart).
    /// </summary>
    public RestartStrategy RestartStrategy { get; set; } = RestartStrategy.Graceful;

    /// <summary>
    /// Indicates if automatic restarts are currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Optional custom restart command to execute before restarting.
    /// </summary>
    public string? PreRestartCommand { get; set; }

    /// <summary>
    /// Optional custom command to execute after restarting.
    /// </summary>
    public string? PostRestartCommand { get; set; }

    /// <summary>
    /// Notify external systems when restart occurs.
    /// </summary>
    public bool NotifyOnRestart { get; set; } = false;

    /// <summary>
    /// Keep tracking restart history.
    /// </summary>
    public bool TrackRestartHistory { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"RestartPolicy: {PolicyType}, MaxRestarts={MaxRestarts}, Delay={RestartDelaySec}s";
}
