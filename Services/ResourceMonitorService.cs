#nullable enable

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

    // For disk IOPS calculation
    private ulong _lastTotalDiskOps;
    private DateTime? _lastDiskStatsTime;

    // Per-service CPU usage calculation
    private readonly Dictionary<string, (ulong LastCpuTime, DateTime LastMeasurementTime)> _lastServiceCpuStats = new();

    // Proc filesystem paths
    private const string ProcUptimePath = "/proc/uptime";
    private const string ProcLoadavgPath = "/proc/loadavg";
    private const string ProcMeminfoPath = "/proc/meminfo";
    private const string ProcStatPath = "/proc/stat";
    private const string ProcDiskstatsPath = "/proc/diskstats";
    private const string ProcDirectory = "/proc/";
    private const string RootDrivePath = "/";

    // Proc file content keys
    private const string MemTotalKey = "MemTotal:";
    private const string MemAvailableKey = "MemAvailable:";
    private const string CachedKey = "Cached:";
    private const string CpuLinePrefix = "cpu ";
    private const string LoopDevicePrefix = "loop";
    private const string RamDevicePrefix = "ram";

    // Cgroup paths and keys
    private const string CgroupSystemSlicePath = "/sys/fs/cgroup/system.slice/";
    private const string MemoryCurrentFile = "memory.current";
    private const string MemoryUsageInBytesFile = "memory.usage_in_bytes";
    private const string CpuStatFile = "cpu.stat";
    private const string CpuAcctUsageFile = "cpuacct.usage";
    private const string UsageUsecKey = "usage_usec";
    private const string ThreadsKey = "Threads:";
    private const string FdSizeKey = "FDSize:";

    // Unit conversion factors
    private const long BytesPerKb = 1024;
    private const long BytesPerMb = 1024L * 1024L;
    private const long BytesPerGb = 1024L * 1024L * 1024L;
    private const decimal PercentMultiplier = 100m;
    private const double PercentMultiplierDouble = 100.0;
    private const ulong MicrosecondsPerMillisecond = 1000;
    private const double NanosecondsPerMillisecond = 1_000_000;

    // Proc file field counts and indices
    private const int CpuStatFieldCount = 8;
    private const int DiskStatsFieldCount = 10;
    private const int DiskStatsDeviceNameIndex = 2;
    private const int DiskStatsReadsIndex = 3;
    private const int DiskStatsWritesIndex = 7;

    // Alert thresholds and windows
    private const decimal HighCpuUsageThresholdPercent = 80m;
    private const long HighMemoryUsageThresholdMb = 1000;
    private const int DuplicateAlertWindowMinutes = 5;
    private const int DefaultMonitoringIntervalMs = 5000;
    private const decimal ZeroPercent = 0m;
    private const decimal HundredPercent = 100m;

    public ResourceMonitorService(ILogger<ResourceMonitorService> logger, SystemdOptions options, ISystemdConnectionService connectionService, IServiceMonitorService serviceMonitorService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connectionService);
        ArgumentNullException.ThrowIfNull(serviceMonitorService);
        _logger = logger;
        _options = options;
        _connectionService = connectionService;
        _serviceMonitorService = serviceMonitorService;
    }

    public async Task<SystemResource> GetSystemResourcesAsync(CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            _logger.LogDebug("Collecting system resource metrics");

            SystemResource resources = new()
            {
                RecordedAt = DateTime.UtcNow,
                CpuCoreCount = Environment.ProcessorCount
            };

            // Uptime from /proc/uptime
            if (File.Exists(ProcUptimePath))
            {
                var uptimeContent = await File.ReadAllTextAsync(ProcUptimePath, ct);
                var parts = uptimeContent.Split(' ');
                if (parts.Length > 0 && double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double uptimeSeconds))
                {
                    resources.SystemUptimeSeconds = (long)uptimeSeconds;
                }
            }

            // Load Averages from /proc/loadavg
            if (File.Exists(ProcLoadavgPath))
            {
                var loadavgContent = await File.ReadAllTextAsync(ProcLoadavgPath, ct);
                var parts = loadavgContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    if (decimal.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load1)) resources.CpuLoad1Min = load1;
                    if (decimal.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load5)) resources.CpuLoad5Min = load5;
                    if (decimal.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var load15)) resources.CpuLoad15Min = load15;
                }
            }

            // Memory from /proc/meminfo
            if (File.Exists(ProcMeminfoPath))
            {
                var meminfoContent = await File.ReadAllLinesAsync(ProcMeminfoPath, ct);
                long totalMemKb = 0, availableMemKb = 0, cachedMemKb = 0;

                foreach (var line in meminfoContent)
                {
                    if (line.StartsWith(MemTotalKey))
                        totalMemKb = ParseMemInfoLine(line);
                    else if (line.StartsWith(MemAvailableKey))
                        availableMemKb = ParseMemInfoLine(line);
                    else if (line.StartsWith(CachedKey))
                        cachedMemKb = ParseMemInfoLine(line);
                }

                resources.TotalMemoryMb = totalMemKb / BytesPerKb;
                resources.AvailableMemoryMb = availableMemKb / BytesPerKb;
                resources.CachedMemoryMb = cachedMemKb / BytesPerKb;
                resources.UsedMemoryMb = resources.TotalMemoryMb - resources.AvailableMemoryMb;
                if (resources.TotalMemoryMb > 0)
                {
                    resources.MemoryUsagePercent = (decimal)resources.UsedMemoryMb / resources.TotalMemoryMb * PercentMultiplier;
                }
            }

            // CPU Usage from /proc/stat
            // This requires two readings for accurate percentage. For a single call,
            // we'll calculate instantaneous usage if enough time has passed since last measurement.
            if (File.Exists(ProcStatPath))
            {
                var statContent = await File.ReadAllLinesAsync(ProcStatPath, ct);
                var cpuLine = statContent.FirstOrDefault(line => line.StartsWith(CpuLinePrefix));
                if (cpuLine != null)
                {
                    var parts = cpuLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= CpuStatFieldCount) // user, nice, system, idle, iowait, irq, softirq, steal
                    {
                        ulong user = ulong.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        ulong nice = ulong.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                        ulong system = ulong.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
                        ulong idle = ulong.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture);
                        ulong iowait = ulong.Parse(parts[5], System.Globalization.CultureInfo.InvariantCulture);
                        ulong irq = ulong.Parse(parts[6], System.Globalization.CultureInfo.InvariantCulture);
                        ulong softirq = ulong.Parse(parts[7], System.Globalization.CultureInfo.InvariantCulture);

                        ulong currentTotalCpuTime = user + nice + system + idle + iowait + irq + softirq;
                        ulong currentIdleCpuTime = idle + iowait; // idle + I/O wait are considered idle

                        if (_lastTotalCpuTime > 0 && currentTotalCpuTime > _lastTotalCpuTime)
                        {
                            ulong totalDiff = currentTotalCpuTime - _lastTotalCpuTime;
                            ulong idleDiff = currentIdleCpuTime - _lastIdleCpuTime;

                            if (totalDiff > 0)
                            {
                                resources.CpuUsagePercent = (decimal)(PercentMultiplierDouble * (totalDiff - idleDiff) / totalDiff);
                            }
                        }

                        _lastTotalCpuTime = currentTotalCpuTime;
                        _lastIdleCpuTime = currentIdleCpuTime;
                        _lastCpuMeasurementTime = DateTime.UtcNow;
                    }
                }
            }

            // Running Processes from /proc
            resources.RunningProcesses = Directory.GetDirectories(ProcDirectory)
                                            .Count(d => int.TryParse(Path.GetFileName(d), out _));

            // Disk Usage for root filesystem
            try
            {
                var rootDrive = new DriveInfo(RootDrivePath);
                if (rootDrive.IsReady)
                {
                    resources.TotalDiskGb = rootDrive.TotalSize / BytesPerGb;
                    resources.AvailableDiskGb = rootDrive.AvailableFreeSpace / BytesPerGb;
                    resources.UsedDiskGb = resources.TotalDiskGb - resources.AvailableDiskGb;
                    if (resources.TotalDiskGb > 0)
                    {
                        resources.DiskUsagePercent = (decimal)resources.UsedDiskGb / resources.TotalDiskGb * PercentMultiplier;
                    }
                }
            }
            catch (Exception diskEx)
            {
                _logger.LogWarning(diskEx, "Could not get disk info for root filesystem");
            }
            
            // Disk IOPS from /proc/diskstats: sum completed reads + writes across physical disks,
            // divided by elapsed time since the last measurement.
            if (File.Exists(ProcDiskstatsPath))
            {
                ulong currentTotalOps = 0;
                var diskstatsLines = await File.ReadAllLinesAsync(ProcDiskstatsPath, ct);
                foreach (var line in diskstatsLines)
                {
                    var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    // Fields: major minor name reads_completed ... writes_completed ...
                    if (fields.Length < DiskStatsFieldCount) continue;

                    var deviceName = fields[DiskStatsDeviceNameIndex];
                    // Skip partitions (e.g. sda1) and loop/ram devices, keep whole disks only.
                    if (deviceName.StartsWith(LoopDevicePrefix) || deviceName.StartsWith(RamDevicePrefix)) continue;
                    if (deviceName.Length > 0 && char.IsDigit(deviceName[^1])) continue;

                    if (ulong.TryParse(fields[DiskStatsReadsIndex], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var readsCompleted) &&
                        ulong.TryParse(fields[DiskStatsWritesIndex], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var writesCompleted))
                    {
                        currentTotalOps += readsCompleted + writesCompleted;
                    }
                }

                var now = DateTime.UtcNow;
                if (_lastDiskStatsTime.HasValue && currentTotalOps >= _lastTotalDiskOps)
                {
                    var elapsedSeconds = (now - _lastDiskStatsTime.Value).TotalSeconds;
                    if (elapsedSeconds > 0)
                    {
                        resources.DiskIopsPerSecond = (long)((currentTotalOps - _lastTotalDiskOps) / elapsedSeconds);
                    }
                }

                _lastTotalDiskOps = currentTotalOps;
                _lastDiskStatsTime = now;
            }

            _logger.LogDebug("System resource collection completed in {Duration}ms", (DateTime.UtcNow - startTime).TotalMilliseconds);
            return resources;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Failed to collect system resources after {Duration}ms", duration.TotalMilliseconds);
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
        ArgumentNullException.ThrowIfNull(unitName);
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
        ArgumentNullException.ThrowIfNull(unitName);
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
        ArgumentNullException.ThrowIfNull(unitName);
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
            string cgroupPath = $"{CgroupSystemSlicePath}{unitName}/";

            // Memory Usage from cgroup
            string memoryUsagePath = Path.Combine(cgroupPath, MemoryCurrentFile); // For cgroup v2
            if (!File.Exists(memoryUsagePath))
            {
                memoryUsagePath = Path.Combine(cgroupPath, MemoryUsageInBytesFile); // For cgroup v1
            }

            if (File.Exists(memoryUsagePath))
            {
                if (long.TryParse(await File.ReadAllTextAsync(memoryUsagePath, ct), out long memoryBytes))
                {
                    metrics.MemoryUsageMb = memoryBytes / BytesPerMb;
                }
            }

            // CPU Usage from cgroup
            // For cgroup v2, cpu.stat gives usage_usec and system_usec
            // For cgroup v1, cpuacct.usage gives total usage in nanoseconds
            string cpuStatPath = Path.Combine(cgroupPath, CpuStatFile); // cgroup v2
            string cpuAcctUsagePath = Path.Combine(cgroupPath, CpuAcctUsageFile); // cgroup v1

            ulong currentServiceCpuTime = 0;
            if (File.Exists(cpuStatPath))
            {
                var cpuStatContent = await File.ReadAllLinesAsync(cpuStatPath, ct);
                var usageUsecLine = cpuStatContent.FirstOrDefault(line => line.StartsWith(UsageUsecKey));
                if (usageUsecLine != null && ulong.TryParse(usageUsecLine.Split(' ')[1], out ulong usageUsec))
                {
                    currentServiceCpuTime = usageUsec * MicrosecondsPerMillisecond; // Convert microsec to nanosec for consistency
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
                        double totalCpuTimeAvailable = timeDifference.TotalMilliseconds * NanosecondsPerMillisecond; // 1ms = 1,000,000ns

                        if (totalCpuTimeAvailable > 0)
                        {
                            // Normalise by the number of logical CPU cores so that a service
                            // fully saturating one core on a 4-core host reports 25 %, and a
                            // service saturating all cores reports 100 %.  Without the core
                            // divisor the raw cgroup value can exceed 100 % on multi-core hosts.
                            double cpuPercent = cpuTimeDifference
                                / (totalCpuTimeAvailable * Environment.ProcessorCount) * PercentMultiplierDouble;
                            metrics.CpuUsagePercent = Math.Clamp((decimal)cpuPercent, ZeroPercent, HundredPercent);
                        }
                    }
                }
                _lastServiceCpuStats[unitName] = (currentServiceCpuTime, DateTime.UtcNow);
            }



            // Thread Count and File Descriptor Count from /proc/<pid>/status
            string procStatusPath = $"{ProcDirectory}{mainPid}/status";
            if (File.Exists(procStatusPath))
            {
                var statusContent = await File.ReadAllLinesAsync(procStatusPath, ct);
                foreach (var line in statusContent)
                {
                    if (line.StartsWith(ThreadsKey))
                    {
                        if (int.TryParse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], out int threads))
                        {
                            metrics.ThreadCount = threads;
                        }
                    }
                    else if (line.StartsWith(FdSizeKey)) // Not always available or precise in /proc/pid/status
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

    public async Task StartContinuousMonitoringAsync(int intervalMs = DefaultMonitoringIntervalMs, CancellationToken ct = default)
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
                        if (metric.CpuUsagePercent > HighCpuUsageThresholdPercent)
                            await AddAlertAsync(new ResourceAlert
                            {
                                UnitName = metric.UnitName,
                                AlertType = ResourceAlertType.HighCpuUsage,
                                Message = $"CPU usage at {metric.CpuUsagePercent}%",
                                CurrentValue = (decimal)metric.CpuUsagePercent,
                                Threshold = HighCpuUsageThresholdPercent
                            });

                        if (metric.MemoryUsageMb > HighMemoryUsageThresholdMb)
                            await AddAlertAsync(new ResourceAlert
                            {
                                UnitName = metric.UnitName,
                                AlertType = ResourceAlertType.HighMemoryUsage,
                                Message = $"Memory usage at {metric.MemoryUsageMb} MB",
                                CurrentValue = metric.MemoryUsageMb,
                                Threshold = HighMemoryUsageThresholdMb
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
                a.AlertTime > DateTime.UtcNow.AddMinutes(-DuplicateAlertWindowMinutes));

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
