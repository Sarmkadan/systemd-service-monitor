#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for monitoring system and service resource usage.
/// </summary>
public interface IResourceMonitorService
{
    /// <summary>
    /// Gets current system resource metrics.
    /// </summary>
    Task<SystemResource> GetSystemResourcesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets CPU usage for a specific service.
    /// </summary>
    Task<decimal> GetServiceCpuUsageAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Gets memory usage for a specific service in MB.
    /// </summary>
    Task<long> GetServiceMemoryUsageAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Gets detailed resource metrics for a service.
    /// </summary>
    Task<ServiceResourceMetrics> GetServiceResourceMetricsAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Collects resource metrics for all services.
    /// </summary>
    Task<IEnumerable<ServiceResourceMetrics>> CollectAllMetricsAsync(CancellationToken ct = default);

    /// <summary>
    /// Starts continuous resource monitoring.
    /// </summary>
    Task StartContinuousMonitoringAsync(int intervalMs = 5000, CancellationToken ct = default);

    /// <summary>
    /// Stops continuous resource monitoring.
    /// </summary>
    Task StopContinuousMonitoringAsync();

    /// <summary>
    /// Gets alerts for resource exhaustion.
    /// </summary>
    Task<IEnumerable<ResourceAlert>> GetResourceAlertsAsync(CancellationToken ct = default);
}

/// <summary>
/// Comprehensive resource metrics for a service.
/// </summary>
public class ServiceResourceMetrics
{
    public string UnitName { get; set; } = string.Empty;
    public decimal CpuUsagePercent { get; set; }
    public long MemoryUsageMb { get; set; }
    public int ThreadCount { get; set; }
    public int FileDescriptorCount { get; set; }
    public long NetworkBytesIn { get; set; }
    public long NetworkBytesOut { get; set; }
    public long DiskBytesRead { get; set; }
    public long DiskBytesWritten { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Resource exhaustion alert.
/// </summary>
public class ResourceAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UnitName { get; set; } = string.Empty;
    public ResourceAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal Threshold { get; set; }
    public DateTime AlertTime { get; set; } = DateTime.UtcNow;
}

public enum ResourceAlertType
{
    HighCpuUsage,
    HighMemoryUsage,
    DiskSpaceLow,
    ProcessCountHigh,
    NetworkLatencyHigh
}
