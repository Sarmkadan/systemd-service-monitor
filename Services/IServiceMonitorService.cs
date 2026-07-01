#nullable enable

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
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of all services.</returns>
    Task<IEnumerable<ServiceInfo>> GetAllServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves information for a specific service by unit name.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The service information, or null if not found.</returns>
    Task<ServiceInfo?> GetServiceByNameAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Retrieves active services only.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of active services.</returns>
    Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves failed services only.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of failed services.</returns>
    Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Refreshes service information from systemd.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshServiceListAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets current status snapshot for a service.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The service status snapshot, or null if not found.</returns>
    Task<ServiceStatus?> GetServiceStatusAsync(string unitName, CancellationToken ct = default);

    /// <summary>
    /// Monitors a service continuously at specified interval.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="intervalMs">The monitoring interval in milliseconds.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartMonitoringAsync(string unitName, int intervalMs = 5000, CancellationToken ct = default);

    /// <summary>
    /// Stops monitoring a specific service.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopMonitoringAsync(string unitName);

    /// <summary>
    /// Gets currently monitored services.
    /// </summary>
    /// <returns>A collection of unit names currently being monitored.</returns>
    IEnumerable<string> GetMonitoredServices();

    /// <summary>
    /// Gets service statistics and aggregated data.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The aggregated statistics of all monitored services.</returns>
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
