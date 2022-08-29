// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for monitoring systemd services.
/// </summary>
public interface IServiceMonitorService
{
    /// <summary>
    /// Retrieves list of all systemd services.
    /// </summary>
    Task<IEnumerable<ServiceInfo>> GetAllServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves information for a specific service by unit name.
    /// </summary>
    Task<ServiceInfo?> GetServiceByNameAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Retrieves active services only.
    /// </summary>
    Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves failed services only.
    /// </summary>
    Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Refreshes service information from systemd.
    /// </summary>
    Task RefreshServiceListAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets current status snapshot for a service.
    /// </summary>
    Task<ServiceStatus?> GetServiceStatusAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Monitors a service continuously at specified interval.
    /// </summary>
    Task StartMonitoringAsync(string unitName, int intervalMs = 5000, CancellationToken ct = default);

    /// <summary>
    /// Stops monitoring a specific service.
    /// </summary>
    Task StopMonitoringAsync(string unitName);

    /// <summary>
    /// Gets currently monitored services.
    /// </summary>
    IEnumerable<string> GetMonitoredServices();

    /// <summary>
    /// Gets service statistics and aggregated data.
    /// </summary>
    Task<ServiceStatistics> GetStatisticsAsync(CancellationToken ct = default);
}

/// <summary>
/// Aggregated statistics about all monitored services.
/// </summary>
public class ServiceStatistics
{
    public int TotalServices { get; set; }
    public int ActiveServices { get; set; }
    public int FailedServices { get; set; }
    public int InactiveServices { get; set; }
    public int MonitoredServices { get; set; }
    public decimal AverageCpuUsage { get; set; }
    public decimal AverageMemoryUsage { get; set; }
    public long TotalRestarts { get; set; }
    public DateTime LastRefreshTime { get; set; }
}
