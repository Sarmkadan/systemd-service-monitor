#nullable enable

using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for controlling systemd services (start, stop, restart).
/// </summary>
public interface IServiceControlService
{
    /// <summary>
    /// Starts a service.
    /// </summary>
    Task<bool> StartServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Stops a service.
    /// </summary>
    Task<bool> StopServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Restarts a service.
    /// </summary>
    Task<bool> RestartServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Reloads a service configuration without stopping it.
    /// </summary>
    Task<bool> ReloadServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Enables a service for automatic startup.
    /// </summary>
    Task<bool> EnableServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Disables a service from automatic startup.
    /// </summary>
    Task<bool> DisableServiceAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Restarts with a specific strategy (Immediate, Graceful, RollingRestart).
    /// </summary>
    Task<bool> RestartWithStrategyAsync(string unitName, RestartStrategy strategy, CancellationToken ct = default);

    /// <summary>
    /// Performs a graceful shutdown with timeout.
    /// </summary>
    Task<bool> GracefulShutdownAsync(string unitName, int timeoutSeconds = 30, CancellationToken ct = default);

    /// <summary>
    /// Gets the status of a recent operation.
    /// </summary>
    Task<OperationResult?> GetLastOperationStatusAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Restarts multiple services concurrently and returns per-service results.
    /// </summary>
    Task<BulkOperationResult> BulkRestartAsync(IEnumerable<string> unitNames, int maxConcurrency = 3, CancellationToken ct = default);
}

/// <summary>
/// Aggregated result of a bulk service operation.
/// </summary>
public class BulkOperationResult
{
    public IReadOnlyList<OperationResult> Results { get; set; } = [];
    public int SuccessCount => Results.Count(r => r.Success);
    public int FailureCount => Results.Count(r => !r.Success);
    public bool AllSucceeded => FailureCount == 0;
}

/// <summary>
/// Represents the result of a service control operation.
/// </summary>
public class OperationResult
{
    public string UnitName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ExitCode { get; set; } = 0;
    public DateTime OperationTime { get; set; } = DateTime.UtcNow;
    public long DurationMs { get; set; } = 0;
}
