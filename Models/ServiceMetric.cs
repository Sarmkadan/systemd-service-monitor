// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents a single metric measurement for a service at a point in time.
/// </summary>
public class ServiceMetric
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent ServiceInfo.
    /// </summary>
    public Guid ServiceInfoId { get; set; }

    /// <summary>
    /// Name of the service unit.
    /// </summary>
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// The type of metric (cpu, memory, threads, etc.).
    /// </summary>
    public MetricType MetricType { get; set; } = MetricType.CpuUsage;

    /// <summary>
    /// The numeric value of the metric.
    /// </summary>
    public decimal Value { get; set; } = 0;

    /// <summary>
    /// Unit of measurement (%, MB, ms, count, etc.).
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Minimum value recorded in the time window.
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Maximum value recorded in the time window.
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Average value in the time window.
    /// </summary>
    public decimal? AvgValue { get; set; }

    /// <summary>
    /// Process ID associated with this metric.
    /// </summary>
    public int ProcessId { get; set; } = 0;

    /// <summary>
    /// Number of samples aggregated into this metric.
    /// </summary>
    public int SampleCount { get; set; } = 1;

    /// <summary>
    /// Tags for categorizing and filtering metrics.
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = [];

    /// <summary>
    /// Timestamp of the measurement.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the measurement window in seconds.
    /// </summary>
    public int DurationSeconds { get; set; } = 60;

    public override string ToString() =>
        $"{UnitName}: {MetricType}={Value}{Unit}";
}

/// <summary>
/// Types of metrics that can be collected for a service.
/// </summary>
public enum MetricType
{
    CpuUsage,
    MemoryUsage,
    ThreadCount,
    FileDescriptorCount,
    RestartCount,
    RequestsPerSecond,
    ErrorCount,
    ResponseTime,
    DiskIo,
    NetworkIn,
    NetworkOut,
    Custom
}
