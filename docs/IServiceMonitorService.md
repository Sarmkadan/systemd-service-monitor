# IServiceMonitorService

The `IServiceMonitorService` interface defines the contract for retrieving aggregated runtime statistics regarding systemd units managed by the monitoring subsystem. It exposes a snapshot of service health metrics, including counts of services in various states (active, failed, inactive), resource consumption averages, and operational history such as total restarts. This interface serves as the primary read-only data source for dashboards, alerting systems, and diagnostic tools within the `systemd-service-monitor` project.

## API

The following members represent the current state of the monitored service fleet. As this is an interface defining data properties, no parameters are accepted, and no exceptions are thrown during property access under normal operating conditions.

### `TotalServices`
*   **Type:** `int`
*   **Purpose:** Returns the total count of all systemd services currently known to the monitor, regardless of their current state.
*   **Parameters:** None.
*   **Return Value:** An integer representing the aggregate number of services.
*   **Exceptions:** None.

### `ActiveServices`
*   **Type:** `int`
*   **Purpose:** Returns the count of services currently in the "active" (running) state.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of active services.
*   **Exceptions:** None.

### `FailedServices`
*   **Type:** `int`
*   **Purpose:** Returns the count of services currently in the "failed" state, indicating an error condition or unsuccessful start.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of failed services.
*   **Exceptions:** None.

### `InactiveServices`
*   **Type:** `int`
*   **Purpose:** Returns the count of services currently in the "inactive" (stopped) state.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of inactive services.
*   **Exceptions:** None.

### `MonitoredServices`
*   **Type:** `int`
*   **Purpose:** Returns the count of services explicitly configured for active monitoring. This value may differ from `TotalServices` if certain system services are excluded from the monitoring scope.
*   **Parameters:** None.
*   **Return Value:** An integer representing the number of services under active observation.
*   **Exceptions:** None.

### `AverageCpuUsage`
*   **Type:** `decimal`
*   **Purpose:** Provides the calculated average CPU utilization percentage across all monitored active services.
*   **Parameters:** None.
*   **Return Value:** A decimal value representing the average CPU usage (e.g., `15.5m` for 15.5%).
*   **Exceptions:** None.

### `AverageMemoryUsage`
*   **Type:** `decimal`
*   **Purpose:** Provides the calculated average memory utilization across all monitored active services. The unit of measurement depends on the underlying implementation configuration (typically MB or percentage).
*   **Parameters:** None.
*   **Return Value:** A decimal value representing the average memory usage.
*   **Exceptions:** None.

### `TotalRestarts`
*   **Type:** `long`
*   **Purpose:** Returns the cumulative count of service restart events detected by the monitor since the monitoring session began or the counter was last reset.
*   **Parameters:** None.
*   **Return Value:** A long integer representing the total number of restarts.
*   **Exceptions:** None.

### `LastRefreshTime`
*   **Type:** `DateTime`
*   **Purpose:** Indicates the timestamp of the most recent data collection cycle from the systemd daemon.
*   **Parameters:** None.
*   **Return Value:** A `DateTime` object representing the local time of the last update.
*   **Exceptions:** None.

## Usage

### Example 1: Health Check Dashboard Logic
This example demonstrates how to consume the interface to determine overall system health and calculate a failure rate.

```csharp
public string GenerateHealthStatus(IServiceMonitorService monitor)
{
    if (monitor.TotalServices == 0)
    {
        return "No services configured for monitoring.";
    }

    var failureRate = (double)monitor.FailedServices / monitor.TotalServices;
    var statusBuilder = new StringBuilder();

    statusBuilder.AppendLine($"System Snapshot taken at: {monitor.LastRefreshTime:HH:mm:ss}");
    statusBuilder.AppendLine($"Active: {monitor.ActiveServices} | Failed: {monitor.FailedServices} | Inactive: {monitor.InactiveServices}");
    
    if (failureRate > 0.1)
    {
        statusBuilder.AppendLine("WARNING: Failure rate exceeds 10%.");
    }

    statusBuilder.AppendLine($"Avg CPU: {monitor.AverageCpuUsage}% | Avg Mem: {monitor.AverageMemoryUsage}");
    
    return statusBuilder.ToString();
}
```

### Example 2: Alerting on Restart Storms
This example illustrates using the `TotalRestarts` and `FailedServices` properties to detect potential instability requiring intervention.

```csharp
public async Task CheckServiceStabilityAsync(IServiceMonitorService monitor, ILogger logger)
{
    // Capture current state
    var failedCount = monitor.FailedServices;
    var restartCount = monitor.TotalRestarts;
    var lastUpdate = monitor.LastRefreshTime;

    // Logic to detect if data is stale
    if (DateTime.Now - lastUpdate > TimeSpan.FromMinutes(5))
    {
        logger.LogWarning("Service monitor data is stale. Last refresh: {Time}", lastUpdate);
        return;
    }

    if (failedCount > 0)
    {
        logger.LogError("Detected {Count} failed services. Immediate investigation required.", failedCount);
    }

    if (restartCount > 100)
    {
        logger.LogWarning("High restart activity detected. Total restarts: {Restarts}. Average CPU load: {Cpu}", 
            restartCount, monitor.AverageCpuUsage);
    }
}
```

## Notes

*   **Data Freshness:** The values exposed by this interface represent a point-in-time snapshot. Consumers should verify `LastRefreshTime` to ensure the data is current before making critical automation decisions, as the underlying systemd state may change rapidly.
*   **Thread Safety:** Implementations of `IServiceMonitorService` are expected to be thread-safe for read operations. However, since the properties reflect mutable external state, reading multiple properties sequentially (e.g., `TotalServices` then `ActiveServices`) does not guarantee a transactionally consistent view if a state change occurs between the accesses.
*   **Zero Division:** When calculating derived metrics such as failure rates or per-service averages based on `AverageCpuUsage`, consumers must handle cases where `TotalServices` or `MonitoredServices` is zero to avoid division by zero errors.
*   **Counter Resets:** The `TotalRestarts` property is a cumulative counter. In long-running processes, ensure the consuming logic accounts for potential integer overflow (though `long` mitigates this) or application restarts which may reset the counter depending on the persistence strategy of the concrete implementation.
