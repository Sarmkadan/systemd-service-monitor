#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Exceptions; // Add this
using System.IO; // Add this
using System.Diagnostics; // Add this for Stopwatch

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of system and service resource monitoring.
/// </summary>
public class ResourceMonitorService : IResourceMonitorService
{
    private readonly ILogger<ResourceMonitorService> _logger;
    private readonly SystemdOptions _options;
    private readonly ISystemdConnectionService _connectionService;
    private readonly IServiceMonitorService _serviceMonitorService; // Add this
    private CancellationTokenSource? _monitoringCts;
    private readonly List<ResourceAlert> _alerts = [];
    private readonly SemaphoreSlim _alertLock = new(1, 1);

    // For CPU usage calculation (system-wide)
    private ulong _lastTotalCpuTime;
    private ulong _lastIdleCpuTime;
    private DateTime _lastCpuMeasurementTime;

    // Per-service CPU usage calculation
    private readonly Dictionary<string, (ulong LastCpuTime, DateTime LastMeasurementTime)> _lastServiceCpuStats = new();

    public ResourceMonitorService(ILogger<ResourceMonitorService> logger, SystemdOptions options, ISystemdConnectionService connectionService, IServiceMonitorService serviceMonitorService)
    {
        _logger = logger;
        _options = options;
        _connectionService = connectionService;
        _serviceMonitorService = serviceMonitorService;
    }

    public async Task<SystemResource> GetSystemResourcesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("Collecting system resource metrics");

            SystemResource resources = new()
            {
                RecordedAt = DateTime.UtcNow,
                CpuCoreCount = Environment.ProcessorCount
            };

            // Uptime from /proc/uptime
            if (File.Exists("/proc/uptime"))
            {
                var uptimeContent = await File.ReadAllTextAsync("/proc/uptime", ct);
                var parts = uptimeContent.Split(' ');
                if (parts.Length > 0 && double.TryParse(parts[0], out double uptimeSeconds))
                {
                    resources.SystemUptimeSeconds = (long)uptimeSeconds;
                }
            }

            // Load Averages from /proc/loadavg
            if (File.Exists("/proc/loadavg"))
            {
                var loadavgContent = await File.ReadAllTextAsync("/proc/loadavg", ct);
                var parts = loadavgContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    if (decimal.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load1)) resources.CpuLoad1Min = load1;
                    if (decimal.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load5)) resources.CpuLoad5Min = load5;
                    if (decimal.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load15)) resources.CpuLoad15Min = load15;
                }
            }

            // Memory from /proc/meminfo
            if (File.Exists("/proc/meminfo"))
            {
                var meminfoContent = await File.ReadAllLinesAsync("/proc/meminfo", ct);
                long totalMemKb = 0, availableMemKb = 0, cachedMemKb = 0;

                foreach (var line in meminfoContent)
                {
                    if (line.StartsWith("MemTotal:"))
                        totalMemKb = ParseMemInfoLine(line);
                    else if (line.StartsWith("MemAvailable:"))
                        availableMemKb = ParseMemInfoLine(line);
                    else if (line.StartsWith("Cached:"))
                        cachedMemKb = ParseMemInfoLine(line);
                }

                resources.TotalMemoryMb = totalMemKb / 1024;
                resources.AvailableMemoryMb = availableMemKb / 1024;
                resources.CachedMemoryMb = cachedMemKb / 1024;
                resources.UsedMemoryMb = resources.TotalMemoryMb - resources.AvailableMemoryMb;
                if (resources.TotalMemoryMb > 0)
                {
                    resources.MemoryUsagePercent = (decimal)resources.UsedMemoryMb / resources.TotalMemoryMb * 100;
                }
            }

            // CPU Usage from /proc/stat
            // This requires two readings for accurate percentage. For a single call,
            // we'll calculate instantaneous usage if enough time has passed since last measurement.
            if (File.Exists("/proc/stat"))
            {
                var statContent = await File.ReadAllLinesAsync("/proc/stat", ct);
                var cpuLine = statContent.FirstOrDefault(line => line.StartsWith("cpu "));
                if (cpuLine != null)
                {
                    var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 8) // user, nice, system, idle, iowait, irq, softirq, steal
                    {
                        ulong user = ulong.Parse(parts[1]);
                        ulong nice = ulong.Parse(parts[2]);
                        ulong system = ulong.Parse(parts[3]);
                        ulong idle = ulong.Parse(parts[4]);
                        ulong iowait = ulong.Parse(parts[5]);
                        ulong irq = ulong.Parse(parts[6]);
                        ulong softirq = ulong.Parse(parts[7]);

                        ulong currentTotalCpuTime = user + nice + system + idle + iowait + irq + softirq;
                        ulong currentIdleCpuTime = idle + iowait; // idle + I/O wait are considered idle

                        if (_lastTotalCpuTime > 0 && currentTotalCpuTime > _lastTotalCpuTime)
                        {
                            ulong totalDiff = currentTotalCpuTime - _lastTotalCpuTime;
                            ulong idleDiff = currentIdleCpuTime - _lastIdleCpuTime;

                            if (totalDiff > 0)
                            {
                                resources.CpuUsagePercent = (decimal)(100.0 * (totalDiff - idleDiff) / totalDiff);
                            }
                        }

                        _lastTotalCpuTime = currentTotalCpuTime;
                        _lastIdleCpuTime = currentIdleCpuTime;
                        _lastCpuMeasurementTime = DateTime.UtcNow;
                    }
                }
            }

            // Running Processes from /proc
            resources.RunningProcesses = Directory.GetDirectories("/proc/")
                                            .Count(d => int.TryParse(Path.GetFileName(d), out _));

            // Disk Usage for root filesystem
            try
            {
                var rootDrive = new DriveInfo("/");
                if (rootDrive.IsReady)
                {
                    resources.TotalDiskGb = rootDrive.TotalSize / (1024L * 1024L * 1024L);
                    resources.AvailableDiskGb = rootDrive.AvailableFreeSpace / (1024L * 1024L * 1024L);
                    resources.UsedDiskGb = resources.TotalDiskGb - resources.AvailableDiskGb;
                    if (resources.TotalDiskGb > 0)
                    {
                        resources.DiskUsagePercent = (decimal)resources.UsedDiskGb / resources.TotalDiskGb * 100;
                    }
                }
            }
            catch (Exception diskEx)
            {
                _logger.LogWarning(diskEx, "Could not get disk info for root filesystem");
            }
            
            // Disk IOPS is hard to get reliably without specialized tools or parsing complex /proc files
            resources.DiskIopsPerSecond = 0; // Keeping as placeholder

            return resources;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect system resources");
            throw new ServiceMonitorException("Failed to collect system resources", ex);
        }
    }

    private long ParseMemInfoLine(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && long.TryParse(parts[1], out long value))
        {
            return value;
        }
        return 0;
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

            ServiceResourceMetrics metrics = new()
            {
                UnitName = unitName,
                MeasuredAt = DateTime.UtcNow
            };

            var serviceInfo = await _serviceMonitorService.GetServiceByNameAsync(unitName, ct);
            if (serviceInfo is null)
            {
                _logger.LogWarning("Service {ServiceName} not found for resource metrics collection.", unitName);
                return metrics; // Return empty metrics
            }

            int mainPid = serviceInfo.MainProcessId;
            if (mainPid <= 0)
            {
                _logger.LogWarning("Main PID for service {ServiceName} not found. Cannot collect detailed resource metrics.", unitName);
                return metrics;
            }

            // Cgroup path for systemd services
            // Example: /sys/fs/cgroup/system.slice/nginx.service/
            string cgroupPath = $"/sys/fs/cgroup/system.slice/{unitName}/";

            // Memory Usage from cgroup
            string memoryUsagePath = Path.Combine(cgroupPath, "memory.current"); // For cgroup v2
            if (!File.Exists(memoryUsagePath))
            {
                memoryUsagePath = Path.Combine(cgroupPath, "memory.usage_in_bytes"); // For cgroup v1
            }

            if (File.Exists(memoryUsagePath))
            {
                if (long.TryParse(await File.ReadAllTextAsync(memoryUsagePath, ct), out long memoryBytes))
                {
                    metrics.MemoryUsageMb = memoryBytes / (1024L * 1024L);
                }
            }

            // CPU Usage from cgroup
            // For cgroup v2, cpu.stat gives usage_usec and system_usec
            // For cgroup v1, cpuacct.usage gives total usage in nanoseconds
            string cpuStatPath = Path.Combine(cgroupPath, "cpu.stat"); // cgroup v2
            string cpuAcctUsagePath = Path.Combine(cgroupPath, "cpuacct.usage"); // cgroup v1

            ulong currentServiceCpuTime = 0;
            if (File.Exists(cpuStatPath))
            {
                var cpuStatContent = await File.ReadAllLinesAsync(cpuStatPath, ct);
                var usageUsecLine = cpuStatContent.FirstOrDefault(line => line.StartsWith("usage_usec"));
                if (usageUsecLine != null && ulong.TryParse(usageUsecLine.Split(' ')[1], out ulong usageUsec))
                {
                    currentServiceCpuTime = usageUsec * 1000; // Convert microsec to nanosec for consistency
                }
            }
            else if (File.Exists(cpuAcctUsagePath))
            {
                if (ulong.TryParse(await File.ReadAllTextAsync(cpuAcctUsagePath, ct), out ulong cpuNs))
                {
                    currentServiceCpuTime = cpuNs;
                }
            }

            if (currentServiceCpuTime > 0)
            {
                if (_lastServiceCpuStats.TryGetValue(unitName, out var lastStats))
                {
                    ulong lastCpuTime = lastStats.LastCpuTime;
                    DateTime lastMeasurementTime = lastStats.LastMeasurementTime;

                    if (currentServiceCpuTime > lastCpuTime && DateTime.UtcNow > lastMeasurementTime)
                    {
                        ulong cpuTimeDifference = currentServiceCpuTime - lastCpuTime;
                        TimeSpan timeDifference = DateTime.UtcNow - lastMeasurementTime;

                        // CPU usage calculation: (CPU time used by service / total CPU time available in period) * 100
                        // totalCpuTimeAvailable represents 100% of one CPU core in nanoseconds for the time difference
                        double totalCpuTimeAvailable = timeDifference.TotalMilliseconds * 1_000_000; // 1ms = 1,000,000ns

                        if (totalCpuTimeAvailable > 0)
                        {
                            // Normalise by the number of logical CPU cores so that a service
                            // fully saturating one core on a 4-core host reports 25 %, and a
                            // service saturating all cores reports 100 %.  Without the core
                            // divisor the raw cgroup value can exceed 100 % on multi-core hosts.
                            double cpuPercent = cpuTimeDifference
                                / (totalCpuTimeAvailable * Environment.ProcessorCount) * 100.0;
                            metrics.CpuUsagePercent = Math.Clamp((decimal)cpuPercent, 0m, 100m);
                        }
                    }
                }
                _lastServiceCpuStats[unitName] = (currentServiceCpuTime, DateTime.UtcNow);
            }



            // Thread Count and File Descriptor Count from /proc/<pid>/status
            string procStatusPath = $"/proc/{mainPid}/status";
            if (File.Exists(procStatusPath))
            {
                var statusContent = await File.ReadAllLinesAsync(procStatusPath, ct);
                foreach (var line in statusContent)
                {
                    if (line.StartsWith("Threads:"))
                    {
                        if (int.TryParse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], out int threads))
                        {
                            metrics.ThreadCount = threads;
                        }
                    }
                    else if (line.StartsWith("FDSize:")) // Not always available or precise in /proc/pid/status
                    {
                        if (int.TryParse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], out int fdSize))
                        {
                            metrics.FileDescriptorCount = fdSize;
                        }
                    }
                }
            }

            // Network I/O and Disk I/O are more complex to get per-service and will remain 0 for now.
            metrics.NetworkBytesIn = 0;
            metrics.NetworkBytesOut = 0;
            metrics.DiskBytesRead = 0;
            metrics.DiskBytesWritten = 0;
            
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for service: {ServiceName}", unitName);
            throw new ServiceMonitorException($"Failed to collect resource metrics for service '{unitName}'", ex);
        }
    }

    public async Task<IEnumerable<ServiceResourceMetrics>> CollectAllMetricsAsync(CancellationToken ct = default)
    {
        try
        {
            var allServices = await _serviceMonitorService.GetAllServicesAsync(ct);
            var collectedMetrics = new List<ServiceResourceMetrics>();

            foreach (var service in allServices)
            {
                var metrics = await GetServiceResourceMetricsAsync(service.UnitName, ct);
                collectedMetrics.Add(metrics);
            }

            return collectedMetrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect metrics for all services");
            throw new ServiceMonitorException("Failed to collect metrics for all services", ex);
        }
    }

    public async Task StartContinuousMonitoringAsync(int intervalMs = 5000, CancellationToken ct = default)
    {
        if (_monitoringCts is not null && !_monitoringCts.Token.IsCancellationRequested)
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
        if (_monitoringCts is not null && !_monitoringCts.Token.IsCancellationRequested)
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

            if (recentAlert is null)
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
