#nullable enable

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Integration;
using Tmds.DBus;
using System.Linq;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of systemd service monitoring.
/// </summary>
public class ServiceMonitorService : IServiceMonitorService
{
    private readonly ILogger<ServiceMonitorService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly IServiceRepository _serviceRepository;
    private readonly Dictionary<string, CancellationTokenSource> _monitoringTokens = [];
    private readonly SemaphoreSlim _monitoringLock = new(1, 1);

    public ServiceMonitorService(
        ILogger<ServiceMonitorService> logger,
        ISystemdConnectionService connectionService,
        IServiceRepository serviceRepository)
    {
        _logger = logger;
        _connectionService = connectionService;
        _serviceRepository = serviceRepository;
    }

    public async Task<IEnumerable<ServiceInfo>> GetAllServicesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetAllServicesAsync");
        var result = await _serviceRepository.GetAllAsync(ct);
        _logger.LogInformation("Retrieved {Count} services", result.Count());
        return result;
    }

    public async Task<ServiceInfo?> GetServiceByNameAsync(string unitName, CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetServiceByNameAsync for {UnitName}", unitName);
        try
        {
            var service = await _serviceRepository.GetByUnitNameAsync(unitName, ct);
            if (service is null)
            {
                _logger.LogWarning("Service not found: {ServiceName}", unitName);
            }
            else
            {
                _logger.LogInformation("Service found: {ServiceName}", unitName);
            }
            return service;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetActiveServicesAsync");
        var result = await _serviceRepository.GetActiveServicesAsync(ct);
        _logger.LogInformation("Retrieved {Count} active services", result.Count());
        return result;
    }

    public async Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetFailedServicesAsync");
        var result = await _serviceRepository.GetFailedServicesAsync(ct);
        _logger.LogInformation("Retrieved {Count} failed services", result.Count());
        return result;
    }

    public async Task RefreshServiceListAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Refreshing service list from systemd");

        if (!_connectionService.IsConnected)
        {
            _logger.LogWarning("D-Bus connection not established, attempting to connect.");
            await _connectionService.ConnectAsync(ct);
        }

        _logger.LogDebug("Establishing D-Bus connection for service list refresh");

        var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync();

        var manager = connection.CreateProxy<ISystemdManager>("org.freedesktop.systemd1", "/org/freedesktop/systemd1");
        var properties = connection.CreateProxy<IProperties>("org.freedesktop.DBus.Properties", "/org/freedesktop/DBus");

        var units = await manager.ListUnitsAsync();
        var serviceInfos = new List<ServiceInfo>();

        foreach (var unit in units)
        {
            if (!unit.Name.EndsWith(".service")) continue; // Only interested in services

            var serviceInfo = new ServiceInfo
            {
                UnitName = unit.Name,
                Description = unit.Description,
                LoadState = Enum.TryParse<ServiceLoadState>(unit.LoadState, true, out var loadState) ? loadState : ServiceLoadState.Unknown,
                State = Enum.TryParse<ServiceState>(unit.ActiveState, true, out var activeState) ? activeState : ServiceState.Unknown,
                SubState = Enum.TryParse<ServiceSubState>(unit.SubState, true, out var subState) ? subState : ServiceSubState.Unknown,
            };

            try
            {
                var unitProperties = await properties.GetAllAsync("org.freedesktop.systemd1.Unit");

                if (unitProperties.TryGetValue("MainPID", out object? mainPidVal) && mainPidVal is uint pidUint)
                    serviceInfo.MainProcessId = (int)pidUint;

                if (unitProperties.TryGetValue("NRestarts", out object? nRestartsVal) && nRestartsVal is uint restartsUint)
                    serviceInfo.RestartCount = (int)restartsUint;

                if (unitProperties.TryGetValue("ActiveEnterTimestamp", out object? tsVal) && tsVal is ulong tsMicro)
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var activeAt = epoch.AddTicks((long)tsMicro * 10);
                    serviceInfo.UptimeSeconds = (long)(DateTime.UtcNow - activeAt).TotalSeconds;
                }

                if (unitProperties.TryGetValue("MemoryCurrentBytes", out object? memVal) && memVal is ulong memBytes)
                    serviceInfo.MemoryUsageMb = (long)(memBytes / (1024.0 * 1024.0));

                serviceInfos.Add(serviceInfo);
                _logger.LogDebug("Processed unit {UnitName}", unit.Name);
            }
            catch (Exception unitEx)
            {
                _logger.LogWarning(unitEx, "Failed to get properties for unit {UnitName}. Skipping detailed info.", unit.Name);
            }
        }

        foreach (var service in serviceInfos)
        {
            await _serviceRepository.UpdateAsync(service, ct);
        }

        _logger.LogInformation("Service list refresh completed. Found {ServiceCount} services.", serviceInfos.Count);
        _logger.LogDebug("Updated {ServiceCount} service records in repository", serviceInfos.Count);
    }

    public async Task<ServiceStatus?> GetServiceStatusAsync(string unitName, CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetServiceStatusAsync for {UnitName}", unitName);
        var service = await _serviceRepository.GetByUnitNameAsync(unitName, ct);
        if (service is null)
        {
            _logger.LogWarning("Service not found when retrieving status: {UnitName}", unitName);
            return null;
        }

        var status = new ServiceStatus
        {
            ServiceInfoId = service.Id,
            UnitName = service.UnitName,
            State = service.State,
            SubState = service.SubState,
            IsEnabled = service.AutoStart,
            IsRunning = service.State == ServiceState.Active,
            ProcessId = service.MainProcessId,
            CpuUsagePercent = (decimal)service.CpuUsagePercent,
            MemoryUsageMb = service.MemoryUsageMb,
            HasFailed = service.State == ServiceState.Failed,
            FailureReason = service.Result,
            UptimeSeconds = service.UptimeSeconds,
            HealthStatus = HealthStatus.Healthy
        };

        _logger.LogInformation("Retrieved status for service {UnitName}: State={State}, SubState={SubState}",
            unitName, service.State, service.SubState);

        return status;
    }

    public async Task StartMonitoringAsync(string unitName, int intervalMs = 5000, CancellationToken ct = default)
    {
        _logger.LogDebug("Entering StartMonitoringAsync for {UnitName}", unitName);
        await _monitoringLock.WaitAsync(ct);
        try
        {
            if (_monitoringTokens.ContainsKey(unitName))
            {
                _logger.LogWarning("Service is already being monitored: {ServiceName}", unitName);
                return;
            }

            var cts = new CancellationTokenSource();
            _monitoringTokens[unitName] = cts;

            _logger.LogInformation("Started monitoring service: {ServiceName} (interval: {IntervalMs}ms)", unitName, intervalMs);

            _ = Task.Run(async () =>
            {
                var monitoringStartTime = DateTime.UtcNow;
                _logger.LogDebug("Monitoring task started for {ServiceName} at {StartTime}", unitName, monitoringStartTime);

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await GetServiceStatusAsync(unitName, cts.Token);
                        await Task.Delay(intervalMs, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Monitoring task cancelled for service: {ServiceName}", unitName);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during monitoring of {ServiceName}", unitName);
                    }
                }

                _logger.LogInformation("Monitoring task completed for service: {ServiceName} (duration: {Duration}ms)",
                    unitName, (DateTime.UtcNow - monitoringStartTime).TotalMilliseconds);
            }, cts.Token);
        }
        finally
        {
            _monitoringLock.Release();
        }
    }

    public async Task StopMonitoringAsync(string unitName)
    {
        _logger.LogDebug("Entering StopMonitoringAsync for {UnitName}", unitName);
        await _monitoringLock.WaitAsync();
        try
        {
            if (_monitoringTokens.TryGetValue(unitName, out var cts))
            {
                var wasMonitoring = _monitoringTokens.ContainsKey(unitName);
                cts.Cancel();
                _monitoringTokens.Remove(unitName);
                _logger.LogInformation("Stopped monitoring service: {ServiceName}", unitName);
                if (!wasMonitoring)
                {
                    _logger.LogWarning("Service {ServiceName} was not being monitored but stop was requested", unitName);
                }
            }
            else
            {
                _logger.LogWarning("Attempted to stop monitoring for unknown service: {ServiceName}", unitName);
            }
        }
        finally
        {
            _monitoringLock.Release();
        }
    }

    public IEnumerable<string> GetMonitoredServices()
    {
        _logger.LogDebug("Retrieving list of monitored services");
        return _monitoringTokens.Keys.ToList();
    }

    public async Task<ServiceStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Entering GetStatisticsAsync");
        try
        {
            var allServices = await _serviceRepository.GetAllAsync(ct);
            var activeServices = await _serviceRepository.GetActiveServicesAsync(ct);
            var failedServices = await _serviceRepository.GetFailedServicesAsync(ct);

            double totalCpu = 0;
            long totalMemory = 0;
            int monitoredCount = 0;

            foreach (var service in allServices)
            {
                if (service.CpuUsagePercent > 0 || service.MemoryUsageMb > 0)
                {
                    totalCpu += service.CpuUsagePercent;
                    totalMemory += service.MemoryUsageMb;
                    monitoredCount++;
                }
            }

            var stats = new ServiceStatistics
            {
                TotalServices = allServices.Count(),
                ActiveServices = activeServices.Count(),
                FailedServices = failedServices.Count(),
                InactiveServices = allServices.Count() - activeServices.Count(),
                MonitoredServices = _monitoringTokens.Count,
                AverageCpuUsage = monitoredCount > 0 ? (decimal)(totalCpu / monitoredCount) : 0,
                AverageMemoryUsage = monitoredCount > 0 ? totalMemory / monitoredCount : 0,
                TotalRestarts = allServices.Sum(s => s.RestartCount),
                LastRefreshTime = DateTime.UtcNow
            };

            _logger.LogInformation("Statistics computed: Total={Total}, Active={Active}, Failed={Failed}, AvgCpu={AvgCpu}, AvgMem={AvgMem}",
                stats.TotalServices, stats.ActiveServices, stats.FailedServices,
                stats.AverageCpuUsage, stats.AverageMemoryUsage);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute service statistics");
            throw;
        }
    }
}
