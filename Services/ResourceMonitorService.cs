// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of system and service resource monitoring.
/// </summary>
public class ResourceMonitorService : IResourceMonitorService
{
    private readonly ILogger<ResourceMonitorService> _logger;
    private readonly SystemdOptions _options;
    private CancellationTokenSource? _monitoringCts;
    private readonly List<ResourceAlert> _alerts = [];
    private readonly SemaphoreSlim _alertLock = new(1, 1);

    public ResourceMonitorService(ILogger<ResourceMonitorService> logger, SystemdOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<SystemResource> GetSystemResourcesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Collecting system resource metrics");

            // Placeholder: would fetch from /proc/stat, /proc/meminfo in production
            return new SystemResource
            {
                TotalMemoryMb = 16384,
                AvailableMemoryMb = 8192,
                UsedMemoryMb = 8192,
                CachedMemoryMb = 2048,
                CpuCoreCount = Environment.ProcessorCount,
                CpuLoad1Min = 0.5m,
                CpuLoad5Min = 0.6m,
                CpuLoad15Min = 0.7m,
                CpuUsagePercent = 25.5m,
                TotalDiskGb = 500,
                UsedDiskGb = 250,
                AvailableDiskGb = 250,
                DiskIopsPerSecond = 1500,
                RunningProcesses = 150,
                SystemUptimeSeconds = 2592000,
                MemoryUsagePercent = 50m,
                DiskUsagePercent = 50m,
                RecordedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect system resources");
            throw;
        }
    }

    public async Task<decimal> GetServiceCpuUsageAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            var metrics = await GetServiceResourceMetricsAsync(unitName, ct);
            return metrics.CpuUsagePercent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<long> GetServiceMemoryUsageAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            var metrics = await GetServiceResourceMetricsAsync(unitName, ct);
            return metrics.MemoryUsageMb;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<ServiceResourceMetrics> GetServiceResourceMetricsAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Collecting resource metrics for service: {ServiceName}", unitName);

            // Placeholder: would fetch from cgroup interface in production
            return await Task.FromResult(new ServiceResourceMetrics
            {
                UnitName = unitName,
                CpuUsagePercent = 15.5m,
                MemoryUsageMb = 256,
                ThreadCount = 8,
                FileDescriptorCount = 42,
                NetworkBytesIn = 1048576,
                NetworkBytesOut = 524288,
                DiskBytesRead = 10485760,
                DiskBytesWritten = 5242880,
                MeasuredAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceResourceMetrics>> CollectAllMetricsAsync(CancellationToken ct = default)
    {
        try
        {
            var metrics = new List<ServiceResourceMetrics>
            {
                new()
                {
                    UnitName = "nginx.service",
                    CpuUsagePercent = 10m,
                    MemoryUsageMb = 128,
                    ThreadCount = 4,
                    FileDescriptorCount = 32
                },
                new()
                {
                    UnitName = "docker.service",
                    CpuUsagePercent = 20m,
                    MemoryUsageMb = 512,
                    ThreadCount = 12,
                    FileDescriptorCount = 64
                }
            };

            return await Task.FromResult(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for all services");
            throw;
        }
    }

    public async Task StartContinuousMonitoringAsync(int intervalMs = 5000, CancellationToken ct = default)
    {
        if (_monitoringCts != null && !_monitoringCts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Resource monitoring is already running");
            return;
        }

        _monitoringCts = new CancellationTokenSource();
        _logger.LogInformation("Starting continuous resource monitoring (interval: {IntervalMs}ms)", intervalMs);

        _ = Task.Run(async () =>
        {
            while (!_monitoringCts.Token.IsCancellationRequested)
            {
                try
                {
                    var systemRes = await GetSystemResourcesAsync(_monitoringCts.Token);
                    var metrics = await CollectAllMetricsAsync(_monitoringCts.Token);

                    // Check for alerts
                    foreach (var metric in metrics)
                    {
                        if (metric.CpuUsagePercent > 80)
                            await AddAlertAsync(new ResourceAlert
                            {
                                UnitName = metric.UnitName,
                                AlertType = ResourceAlertType.HighCpuUsage,
                                Message = $"CPU usage at {metric.CpuUsagePercent}%",
                                CurrentValue = (decimal)metric.CpuUsagePercent,
                                Threshold = 80
                            });

                        if (metric.MemoryUsageMb > 1000)
                            await AddAlertAsync(new ResourceAlert
                            {
                                UnitName = metric.UnitName,
                                AlertType = ResourceAlertType.HighMemoryUsage,
                                Message = $"Memory usage at {metric.MemoryUsageMb} MB",
                                CurrentValue = metric.MemoryUsageMb,
                                Threshold = 1000
                            });
                    }

                    await Task.Delay(intervalMs, _monitoringCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during continuous monitoring");
                }
            }
        }, _monitoringCts.Token);
    }

    public async Task StopContinuousMonitoringAsync()
    {
        if (_monitoringCts != null && !_monitoringCts.Token.IsCancellationRequested)
        {
            _monitoringCts.Cancel();
            _logger.LogInformation("Stopped continuous resource monitoring");
        }
    }

    public async Task<IEnumerable<ResourceAlert>> GetResourceAlertsAsync(CancellationToken ct = default)
    {
        await _alertLock.WaitAsync(ct);
        try
        {
            return _alerts.ToList();
        }
        finally
        {
            _alertLock.Release();
        }
    }

    private async Task AddAlertAsync(ResourceAlert alert)
    {
        await _alertLock.WaitAsync();
        try
        {
            // Avoid duplicate alerts within 5 minutes
            var recentAlert = _alerts.FirstOrDefault(a =>
                a.UnitName == alert.UnitName &&
                a.AlertType == alert.AlertType &&
                a.AlertTime > DateTime.UtcNow.AddMinutes(-5));

            if (recentAlert == null)
            {
                _alerts.Add(alert);
                _logger.LogWarning("Resource alert: {Message}", alert.Message);
            }
        }
        finally
        {
            _alertLock.Release();
        }
    }
}
