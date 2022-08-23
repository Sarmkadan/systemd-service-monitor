// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents system-level resource metrics collected at a point in time.
/// </summary>
public class SystemResource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Total memory available on the system in MB.
    /// </summary>
    public long TotalMemoryMb { get; set; } = 0;

    /// <summary>
    /// Available memory on the system in MB.
    /// </summary>
    public long AvailableMemoryMb { get; set; } = 0;

    /// <summary>
    /// Used memory on the system in MB.
    /// </summary>
    public long UsedMemoryMb { get; set; } = 0;

    /// <summary>
    /// Cached memory in MB.
    /// </summary>
    public long CachedMemoryMb { get; set; } = 0;

    /// <summary>
    /// Number of CPU cores available.
    /// </summary>
    public int CpuCoreCount { get; set; } = 0;

    /// <summary>
    /// Average CPU load over 1 minute.
    /// </summary>
    public decimal CpuLoad1Min { get; set; } = 0;

    /// <summary>
    /// Average CPU load over 5 minutes.
    /// </summary>
    public decimal CpuLoad5Min { get; set; } = 0;

    /// <summary>
    /// Average CPU load over 15 minutes.
    /// </summary>
    public decimal CpuLoad15Min { get; set; } = 0;

    /// <summary>
    /// Total CPU usage percentage (0-100).
    /// </summary>
    public decimal CpuUsagePercent { get; set; } = 0;

    /// <summary>
    /// Total disk space in GB.
    /// </summary>
    public long TotalDiskGb { get; set; } = 0;

    /// <summary>
    /// Used disk space in GB.
    /// </summary>
    public long UsedDiskGb { get; set; } = 0;

    /// <summary>
    /// Available disk space in GB.
    /// </summary>
    public long AvailableDiskGb { get; set; } = 0;

    /// <summary>
    /// Disk I/O operations per second.
    /// </summary>
    public long DiskIopsPerSecond { get; set; } = 0;

    /// <summary>
    /// Total network bytes received.
    /// </summary>
    public long NetworkBytesIn { get; set; } = 0;

    /// <summary>
    /// Total network bytes transmitted.
    /// </summary>
    public long NetworkBytesOut { get; set; } = 0;

    /// <summary>
    /// Number of running processes.
    /// </summary>
    public int RunningProcesses { get; set; } = 0;

    /// <summary>
    /// System uptime in seconds.
    /// </summary>
    public long SystemUptimeSeconds { get; set; } = 0;

    /// <summary>
    /// Load average as percentage (0-100 range based on cores).
    /// </summary>
    public decimal LoadAveragePercent { get; set; } = 0;

    /// <summary>
    /// Memory usage as percentage.
    /// </summary>
    public decimal MemoryUsagePercent { get; set; } = 0;

    /// <summary>
    /// Disk usage as percentage.
    /// </summary>
    public decimal DiskUsagePercent { get; set; } = 0;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() =>
        $"System: CPU={CpuUsagePercent:F1}%, MEM={MemoryUsagePercent:F1}%, DISK={DiskUsagePercent:F1}%";
}
