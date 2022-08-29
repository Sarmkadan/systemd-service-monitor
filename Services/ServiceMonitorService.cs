// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

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
            if (service == null)
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

            if (!await _connectionService.VerifyConnectionAsync(ct))
            {
                _logger.LogWarning("D-Bus connection lost, reconnecting");
                await _connectionService.ConnectAsync(ct);
            }

            // Placeholder: would fetch actual services from systemd via D-Bus
            var demoServices = CreateDemoServices();
            foreach (var service in demoServices)
            {
                await _serviceRepository.UpdateAsync(service, ct);
            }

            _logger.LogInformation("Service list refresh completed");
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
            if (service == null)
                return null;

            return new ServiceStatus
            {
                ServiceInfoId = service.Id,
                UnitName = service.UnitName,
                State = service.State,
                SubState = service.SubState,
                IsEnabled = service.AutoStart,
                IsRunning = service.State == ServiceState.Active,
                ProcessId = service.MainProcessId,
                CpuUsagePercent = 0,
                MemoryUsageMb = 0,
                HasFailed = service.State == ServiceState.Failed,
                FailureReason = service.Result,
                UptimeSeconds = service.UptimeSeconds,
                HealthStatus = HealthStatus.Healthy
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

            return new ServiceStatistics
            {
                TotalServices = allServices.Count(),
                ActiveServices = activeServices.Count(),
                FailedServices = failedServices.Count(),
                InactiveServices = allServices.Count() - activeServices.Count(),
                MonitoredServices = _monitoringTokens.Count,
                AverageCpuUsage = 0,
                AverageMemoryUsage = 0,
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

    private List<ServiceInfo> CreateDemoServices()
    {
        return
        [
            new ServiceInfo
            {
                UnitName = "nginx.service",
                Description = "A high performance web server and reverse proxy server",
                State = ServiceState.Active,
                SubState = ServiceSubState.Running,
                AutoStart = true,
                MainProcessId = 1234,
                UptimeSeconds = 86400,
                RestartCount = 0
            },
            new ServiceInfo
            {
                UnitName = "docker.service",
                Description = "Docker Application Container Engine",
                State = ServiceState.Active,
                SubState = ServiceSubState.Running,
                AutoStart = true,
                MainProcessId = 5678,
                UptimeSeconds = 172800,
                RestartCount = 1
            }
        ];
    }
}
