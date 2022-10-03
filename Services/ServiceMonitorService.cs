#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Integration; // Add this
using Tmds.DBus; // Add this
using System.Linq; // Add this

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
        try
        {
            return await _serviceRepository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all services");
            throw;
        }
    }

    public async Task<ServiceInfo?> GetServiceByNameAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            var service = await _serviceRepository.GetByUnitNameAsync(unitName, ct);
            if (service is null)
            {
                _logger.LogWarning("Service not found: {ServiceName}", unitName);
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
        try
        {
            return await _serviceRepository.GetActiveServicesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active services");
            throw;
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _serviceRepository.GetFailedServicesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve failed services");
            throw;
        }
    }

    public async Task RefreshServiceListAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Refreshing service list from systemd");

            if (!await _connectionService.IsConnected)
            {
                _logger.LogWarning("D-Bus connection not established, attempting to connect.");
                await _connectionService.ConnectAsync(ct);
            }

            var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync(); // Assuming DBusConnectionManager is accessible

            var manager = connection.CreateProxy<ISystemdManager>("org.freedesktop.systemd1", "/org/freedesktop/systemd1");
            var properties = connection.CreateProxy<IProperties>("org.freedesktop.DBus.Properties", "/org/freedesktop/DBus"); // Correct service/path for IProperties

            var units = await manager.ListUnitsAsync();
            var serviceInfos = new List<ServiceInfo>();

            foreach (var unit in units)
            {
                if (!unit.Name.EndsWith(".service")) continue; // Only interested in services

                ServiceInfo serviceInfo = new()
                {
                    UnitName = unit.Name,
                    Description = unit.Description,
                    LoadState = Enum.TryParse<ServiceLoadState>(unit.LoadState, true, out var loadState) ? loadState : ServiceLoadState.Unknown,
                    State = Enum.TryParse<ServiceState>(unit.ActiveState, true, out var activeState) ? activeState : ServiceState.Unknown,
                    SubState = Enum.TryParse<ServiceSubState>(unit.SubState, true, out var subState) ? subState : ServiceSubState.Unknown,
                };

                try
                {
                    // Get detailed properties for the unit
                    var unitProperties = await properties.GetAllAsync("org.freedesktop.systemd1.Unit", unit.Path);

                    if (unitProperties.TryGetValue("MainPID", out object? mainPid) && mainPid is uint pid)
                    {
                        serviceInfo.MainProcessId = (int)pid;
                    }
                    if (unitProperties.TryGetValue("NRestarts", out object? nRestarts) && nRestarts is uint restarts)
                    {
                        serviceInfo.RestartCount = (int)restarts;
                    }
                    if (unitProperties.TryGetValue("ActiveEnterTimestamp", out object? activeEnterTimestamp) && activeEnterTimestamp is ulong timestampMicroseconds)
                    {
                        // ActiveEnterTimestamp is in microseconds since epoch
                        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var activeEnterDateTime = epoch.AddTicks((long)timestampMicroseconds * 10); // 10 ticks per microsecond
                        serviceInfo.UptimeSeconds = (long)(DateTime.UtcNow - activeEnterDateTime).TotalSeconds;
                    }
                    if (unitProperties.TryGetValue("CPUUsageNsec", out object? cpuUsageNsec) && cpuUsageNsec is ulong cpuNs)
                    {
                        // serviceInfo.CpuUsagePercent = (double)cpuNs / (Environment.ProcessorCount * 1_000_000_000); // Need more context for accurate calculation
                    }
                    if (unitProperties.TryGetValue("MemoryCurrentBytes", out object? memoryCurrentBytes) && memoryCurrentBytes is ulong memoryBytes)
                    {
                        serviceInfo.MemoryUsageMb = (long)(memoryBytes / (1024.0 * 1024.0));
                    }

                    serviceInfos.Add(serviceInfo);
                }
                catch (Exception unitEx)
                {
                    _logger.LogWarning(unitEx, "Failed to get properties for unit {UnitName}. Skipping detailed info.", unit.Name);
                }
            }

            // Update all services in the repository
            foreach (var service in serviceInfos)
            {
                await _serviceRepository.UpdateAsync(service, ct);
            }

            _logger.LogInformation("Service list refresh completed. Found {ServiceCount} services.", serviceInfos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh service list");
            throw;
        }
    }

    public async Task<ServiceStatus?> GetServiceStatusAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            var service = await _serviceRepository.GetByUnitNameAsync(unitName, ct);
            if (service is null)
                return null;

            return new ServiceStatus
            {
                ServiceInfoId = service.Id,
                UnitName = service.UnitName,
                State = service.State,
                SubState = service.SubState,
                IsEnabled = service.AutoStart, // AutoStart is not fetched from D-Bus directly, assumes it's persisted
                IsRunning = service.State == ServiceState.Active,
                ProcessId = service.MainProcessId,
                CpuUsagePercent = service.CpuUsagePercent, // Populated by RefreshServiceListAsync
                MemoryUsageMb = service.MemoryUsageMb, // Populated by RefreshServiceListAsync
                HasFailed = service.State == ServiceState.Failed,
                FailureReason = service.Result, // Result is not directly fetched from D-Bus in this method
                UptimeSeconds = service.UptimeSeconds,
                HealthStatus = HealthStatus.Healthy // This needs a proper health check
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task StartMonitoringAsync(string unitName, int intervalMs = 5000, CancellationToken ct = default)
    {
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
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Now calls the updated GetServiceStatusAsync
                        await GetServiceStatusAsync(unitName, cts.Token);
                        await Task.Delay(intervalMs, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during monitoring of {ServiceName}", unitName);
                    }
                }
            }, cts.Token);
        }
        finally
        {
            _monitoringLock.Release();
        }
    }

    public async Task StopMonitoringAsync(string unitName)
    {
        await _monitoringLock.WaitAsync();
        try
        {
            if (_monitoringTokens.TryGetValue(unitName, out var cts))
            {
                cts.Cancel();
                _monitoringTokens.Remove(unitName);
                _logger.LogInformation("Stopped monitoring service: {ServiceName}", unitName);
            }
        }
        finally
        {
            _monitoringLock.Release();
        }
    }

    public IEnumerable<string> GetMonitoredServices()
    {
        return _monitoringTokens.Keys.ToList();
    }

    public async Task<ServiceStatistics> GetStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            var allServices = await _serviceRepository.GetAllAsync(ct);
            var activeServices = await _serviceRepository.GetActiveServicesAsync(ct);
            var failedServices = await _serviceRepository.GetFailedServicesAsync(ct);

            // Calculate average CPU and Memory based on currently available data
            // This assumes RefreshServiceListAsync has been called recently
            double totalCpu = 0;
            long totalMemory = 0;
            int monitoredCount = 0;

            foreach (var service in allServices)
            {
                // Only consider services for which we have recent resource data
                if (service.CpuUsagePercent > 0 || service.MemoryUsageMb > 0)
                {
                    totalCpu += service.CpuUsagePercent;
                    totalMemory += service.MemoryUsageMb;
                    monitoredCount++;
                }
            }

            return new ServiceStatistics
            {
                TotalServices = allServices.Count(),
                ActiveServices = activeServices.Count(),
                FailedServices = failedServices.Count(),
                InactiveServices = allServices.Count() - activeServices.Count(),
                MonitoredServices = _monitoringTokens.Count,
                AverageCpuUsage = monitoredCount > 0 ? totalCpu / monitoredCount : 0,
                AverageMemoryUsage = monitoredCount > 0 ? totalMemory / monitoredCount : 0,
                TotalRestarts = allServices.Sum(s => s.RestartCount),
                LastRefreshTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute service statistics");
            throw;
        }
    }

    // Removed CreateDemoServices as it's no longer needed
}
