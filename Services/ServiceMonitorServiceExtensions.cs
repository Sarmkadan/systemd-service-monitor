#nullable enable

using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Extension methods for ServiceMonitorService providing additional utility functionality.
/// </summary>
public static class ServiceMonitorServiceExtensions
{
    /// <summary>
    /// Filters services by their current state.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="state">The state to filter by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtered collection of services matching the specified state</returns>
    public static async Task<IEnumerable<ServiceInfo>> GetServicesByStateAsync(
        this ServiceMonitorService serviceMonitor,
        ServiceState state,
        CancellationToken ct = default)
    {
        var allServices = await serviceMonitor.GetAllServicesAsync(ct);
        return allServices.Where(s => s.State == state);
    }

    /// <summary>
    /// Filters services by their current sub-state.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="subState">The sub-state to filter by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtered collection of services matching the specified sub-state</returns>
    public static async Task<IEnumerable<ServiceInfo>> GetServicesBySubStateAsync(
        this ServiceMonitorService serviceMonitor,
        ServiceSubState subState,
        CancellationToken ct = default)
    {
        var allServices = await serviceMonitor.GetAllServicesAsync(ct);
        return allServices.Where(s => s.SubState == subState);
    }

    /// <summary>
    /// Gets a service by name with fallback to refresh if not found.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="unitName">The service unit name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The service info or null if not found</returns>
    public static async Task<ServiceInfo?> GetServiceByNameWithRefreshAsync(
        this ServiceMonitorService serviceMonitor,
        string unitName,
        CancellationToken ct = default)
    {
        var service = await serviceMonitor.GetServiceByNameAsync(unitName, ct);

        if (service is null)
        {
            await serviceMonitor.RefreshServiceListAsync(ct);
            service = await serviceMonitor.GetServiceByNameAsync(unitName, ct);
        }

        return service;
    }

    /// <summary>
    /// Gets the status of multiple services in a single call.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="unitNames">Collection of service unit names</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of service statuses</returns>
    public static async Task<IEnumerable<ServiceStatus>> GetMultipleServiceStatusesAsync(
        this ServiceMonitorService serviceMonitor,
        IEnumerable<string> unitNames,
        CancellationToken ct = default)
    {
        var statuses = new List<ServiceStatus>();

        foreach (var unitName in unitNames)
        {
            var status = await serviceMonitor.GetServiceStatusAsync(unitName, ct);
            if (status is not null)
            {
                statuses.Add(status);
            }
        }

        return statuses;
    }

    /// <summary>
    /// Checks if a specific service is currently being monitored.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="unitName">The service unit name</param>
    /// <returns>True if the service is being monitored, false otherwise</returns>
    public static bool IsServiceMonitored(
        this ServiceMonitorService serviceMonitor,
        string unitName)
    {
        return serviceMonitor.GetMonitoredServices().Contains(unitName);
    }

    /// <summary>
    /// Gets statistics filtered by service state.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="state">The state to filter statistics by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Statistics for services matching the specified state</returns>
    public static async Task<ServiceStatistics> GetStatisticsByStateAsync(
        this ServiceMonitorService serviceMonitor,
        ServiceState state,
        CancellationToken ct = default)
    {
        var services = await serviceMonitor.GetServicesByStateAsync(state, ct);
        var serviceList = services.ToList();

        double totalCpu = 0;
        long totalMemory = 0;
        int monitoredCount = 0;
        int totalRestarts = 0;

        foreach (var service in serviceList)
        {
            if (service.CpuUsagePercent > 0 || service.MemoryUsageMb > 0)
            {
                totalCpu += service.CpuUsagePercent;
                totalMemory += service.MemoryUsageMb;
                monitoredCount++;
            }
            totalRestarts += service.RestartCount;
        }

        return new ServiceStatistics
        {
            TotalServices = serviceList.Count,
            ActiveServices = state == ServiceState.Active ? serviceList.Count : 0,
            FailedServices = state == ServiceState.Failed ? serviceList.Count : 0,
            InactiveServices = state == ServiceState.Inactive ? serviceList.Count : 0,
            MonitoredServices = serviceMonitor.GetMonitoredServices().Count(name =>
                serviceList.Any(s => s.UnitName == name)),
            AverageCpuUsage = monitoredCount > 0 ? (decimal)(totalCpu / monitoredCount) : 0,
            AverageMemoryUsage = monitoredCount > 0 ? totalMemory / monitoredCount : 0,
            TotalRestarts = totalRestarts,
            LastRefreshTime = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets a summary of all services with computed status information.
    /// </summary>
    /// <param name="serviceMonitor">The service monitor instance</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of service info with computed status information</returns>
    public static async Task<IEnumerable<ServiceInfo>> GetServicesWithStatusAsync(
        this ServiceMonitorService serviceMonitor,
        CancellationToken ct = default)
    {
        var services = (await serviceMonitor.GetAllServicesAsync(ct)).ToList();
        var monitoredServices = serviceMonitor.GetMonitoredServices().ToHashSet();

        foreach (var service in services)
        {
            service.AutoStart = monitoredServices.Contains(service.UnitName);
        }

        return services;
    }
}