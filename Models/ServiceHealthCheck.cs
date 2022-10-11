#nullable enable

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents a health check configuration and result for a service.
/// </summary>
public class ServiceHealthCheck
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent ServiceInfo.
    /// </summary>
    public Guid ServiceInfoId { get; set; }

    /// <summary>
    /// Name identifier for this health check.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of health check (Http, TCP, Process, Custom).
    /// </summary>
    public HealthCheckType CheckType { get; set; } = HealthCheckType.Process;

    /// <summary>
    /// Description of what this health check validates.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint or target for the health check.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method for HTTP health checks (GET, POST, etc.).
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Expected HTTP status code for success.
    /// </summary>
    public int? ExpectedHttpStatus { get; set; } = 200;

    /// <summary>
    /// Timeout for the health check in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Interval between health check runs in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Number of consecutive failures before marking service as unhealthy.
    /// </summary>
    public int UnhealthyThreshold { get; set; } = 3;

    /// <summary>
    /// Number of consecutive successes before marking service as healthy.
    /// </summary>
    public int HealthyThreshold { get; set; } = 2;

    /// <summary>
    /// Whether this health check is currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Current health status based on latest checks.
    /// </summary>
    public HealthStatus CurrentStatus { get; set; } = HealthStatus.Unknown;

    /// <summary>
    /// Result message from the last health check run.
    /// </summary>
    public string LastCheckMessage { get; set; } = string.Empty;

    /// <summary>
    /// Response time in milliseconds from the last check.
    /// </summary>
    public long LastCheckResponseMs { get; set; } = 0;

    /// <summary>
    /// Timestamp of the last health check execution.
    /// </summary>
    public DateTime? LastCheckTime { get; set; }

    /// <summary>
    /// Number of consecutive failed checks.
    /// </summary>
    public int ConsecutiveFailures { get; set; } = 0;

    /// <summary>
    /// Number of consecutive successful checks.
    /// </summary>
    public int ConsecutiveSuccesses { get; set; } = 0;

    /// <summary>
    /// Total checks executed since creation.
    /// </summary>
    public long TotalChecks { get; set; } = 0;

    /// <summary>
    /// Total successful checks.
    /// </summary>
    public long SuccessfulChecks { get; set; } = 0;

    /// <summary>
    /// Success rate percentage (0-100).
    /// </summary>
    public decimal SuccessRatePercent { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"HealthCheck: {Name} ({CheckType}) - Status: {CurrentStatus}";
}

/// <summary>
/// Types of health checks that can be performed on a service.
/// </summary>
public enum HealthCheckType
{
    Http,
    Tcp,
    Process,
    Script,
    Custom
}
