# ServiceMetric

`ServiceMetric` represents a single collected data point for a monitored systemd service. It captures both instantaneous resource measurements (CPU, memory, network, disk I/O) and aggregated statistical values (min, max, average) over a sampling window, along with identifying metadata such as the service unit name, process ID, and optional tags.

## API

### Properties

- **`public Guid Id`**  
  Unique identifier for this metric record. Used as the primary key when persisting metrics to a data store.

- **`public Guid ServiceInfoId`**  
  Foreign key linking this metric to its parent `ServiceInfo` entity. Establishes the relationship between a monitored service definition and its collected metrics.

- **`public string UnitName`**  
  The systemd unit name (e.g., `nginx.service`, `postgresql.service`) from which this metric was collected. Non-nullable.

- **`public MetricType MetricType`**  
  Enumerated value classifying the kind of measurement this record represents. Determines which value fields are relevant and how the data should be interpreted by downstream consumers.

- **`public decimal Value`**  
  The primary measured value for this metric. The semantics depend on `MetricType`—for instantaneous metrics it holds the current reading; for aggregated metrics it may hold the latest sample or a computed central value.

- **`public string Unit`**  
  Unit of measurement for `Value` (e.g., `%`, `MB`, `bytes/s`). Non-nullable string that provides context for display and alerting thresholds.

- **`public decimal? MinValue`**  
  Minimum value observed during the sampling period. Nullable; present only when `SampleCount` is greater than zero and the metric type supports range aggregation.

- **`public decimal? MaxValue`**  
  Maximum value observed during the sampling period. Nullable; present only when `SampleCount` is greater than zero and the metric type supports range aggregation.

- **`public decimal? AvgValue`**  
  Arithmetic mean of all samples collected during the sampling period. Nullable; present only when `SampleCount` is greater than zero and the metric type supports average computation.

- **`public int ProcessId`**  
  Operating system process identifier of the main service process at the time of collection. Zero or negative values indicate the process ID could not be determined (e.g., service was inactive).

- **`public int SampleCount`**  
  Number of individual measurements aggregated into this record. A value of `1` indicates a single instantaneous reading; higher values indicate a summarised window.

- **`public Dictionary<string, string> Tags`**  
  Arbitrary key-value pairs attached to this metric for categorisation, filtering, and routing. Never null; may be empty. Typical keys include `host`, `environment`, `region`, or custom labels.

- **`public DateTime Timestamp`**  
  UTC timestamp indicating when this metric was recorded or when the sampling window ended. Used for time-series ordering and retention policies.

- **`public string? ServiceName`**  
  Human-readable display name of the service, if available. Nullable; may differ from `UnitName` when a friendly alias has been configured.

- **`public double CpuPercentage`**  
  CPU usage of the service process as a percentage of one logical core (0.0–100.0+ on multi-threaded workloads). Collected from `/proc` or equivalent OS interfaces.

- **`public double MemoryUsageMb`**  
  Resident memory usage of the service process in megabytes. Represents the RSS (Resident Set Size) at collection time.

- **`public long NetworkBytesIn`**  
  Cumulative bytes received over the network by the service process since collection began. For instantaneous metrics this is the delta since the previous reading.

- **`public long NetworkBytesOut`**  
  Cumulative bytes transmitted over the network by the service process since collection began. For instantaneous metrics this is the delta since the previous reading.

- **`public long DiskReadBytesPerSec`**  
  Disk read throughput in bytes per second, measured over the sampling interval. Zero when disk I/O accounting is unavailable.

- **`public long DiskWriteBytesPerSec`**  
  Disk write throughput in bytes per second, measured over the sampling interval. Zero when disk I/O accounting is unavailable.

## Usage

### Example 1: Creating and persisting a CPU metric

```csharp
var metric = new ServiceMetric
{
    Id = Guid.NewGuid(),
    ServiceInfoId = serviceInfo.Id,
    UnitName = "postgresql.service",
    MetricType = MetricType.Cpu,
    Value = 12.5m,
    Unit = "%",
    MinValue = 2.1m,
    MaxValue = 45.8m,
    AvgValue = 18.3m,
    ProcessId = 1542,
    SampleCount = 60,
    Tags = new Dictionary<string, string>
    {
        ["host"] = "db-server-01",
        ["environment"] = "production"
    },
    Timestamp = DateTime.UtcNow,
    ServiceName = "PostgreSQL Database",
    CpuPercentage = 12.5,
    MemoryUsageMb = 0,
    NetworkBytesIn = 0,
    NetworkBytesOut = 0,
    DiskReadBytesPerSec = 0,
    DiskWriteBytesPerSec = 0
};

await dbContext.ServiceMetrics.AddAsync(metric);
await dbContext.SaveChangesAsync();
```

### Example 2: Querying recent metrics and evaluating alert conditions

```csharp
var threshold = 500.0; // MB
var windowStart = DateTime.UtcNow.AddMinutes(-15);

var highMemoryMetrics = await dbContext.ServiceMetrics
    .Where(m => m.MetricType == MetricType.Memory
                && m.Timestamp >= windowStart
                && m.Value > threshold)
    .OrderByDescending(m => m.Timestamp)
    .ToListAsync();

foreach (var metric in highMemoryMetrics)
{
    Console.WriteLine(
        $"ALERT: {metric.ServiceName ?? metric.UnitName} " +
        $"used {metric.Value} {metric.Unit} at {metric.Timestamp:O} " +
        $"(PID {metric.ProcessId}, samples: {metric.SampleCount})");

    if (metric.Tags.TryGetValue("host", out var host))
        Console.WriteLine($"  Host: {host}");
}
```

## Notes

- **Nullable aggregation fields**: `MinValue`, `MaxValue`, and `AvgValue` are null when `SampleCount` is zero or when the metric type does not support range aggregation. Consumers must null-check these fields before performing arithmetic or comparisons.
- **Resource fields and metric type**: `CpuPercentage`, `MemoryUsageMb`, `NetworkBytesIn`, `NetworkBytesOut`, `DiskReadBytesPerSec`, and `DiskWriteBytesPerSec` are always populated regardless of `MetricType`. Filtering by `MetricType` is the caller's responsibility when only specific resource data is meaningful.
- **ProcessId validity**: A `ProcessId` of zero or a negative value indicates the service process was not running or could not be identified at collection time. Alerting logic should treat such values as indeterminate rather than actionable.
- **Timestamp precision**: `Timestamp` uses `DateTime` (not `DateTimeOffset`). The project convention assumes all timestamps are UTC. Storing or comparing local-time values will produce incorrect ordering and retention behaviour.
- **Tags dictionary**: The `Tags` dictionary is never null, but individual lookups may fail. Use `TryGetValue` to avoid `KeyNotFoundException`. Modifications to the dictionary after the metric has been attached to a tracking context may not be detected by change-tracking mechanisms unless the entire dictionary reference is replaced.
- **Thread safety**: This type is a plain data model (POCO) with no internal synchronisation. Concurrent reads from multiple threads are safe if the instance is not being mutated. Concurrent writes or mixed read/write access must be guarded externally by the caller.
- **Network counters**: `NetworkBytesIn` and `NetworkBytesOut` represent cumulative counters when `SampleCount` is 1, and deltas when aggregated over multiple samples. Downstream consumers calculating rates must divide by the sampling interval duration, which is not stored in this record and must be derived from consecutive `Timestamp` values.
