# ServiceStatus

`ServiceStatus` represents a snapshot of the current state, performance metrics, and health information of a tracked systemd service at a specific point in time. It serves as the primary data transfer object for telemetry captured by the `systemd-service-monitor` system, allowing for the aggregation, reporting, and alerting based on service lifecycle and resource utilization.

## API

### Properties

*   **Id** (`Guid`)
    The unique identifier for this specific status snapshot.

*   **ServiceInfoId** (`Guid`)
    The unique identifier linking this status to the underlying service definition or configuration.

*   **UnitName** (`string`)
    The name of the systemd unit (e.g., `nginx.service`).

*   **State** (`ServiceState`)
    The high-level systemd state of the service (e.g., `loaded`, `active`).

*   **SubState** (`ServiceSubState`)
    The detailed systemd sub-state of the service (e.g., `running`, `exited`).

*   **IsEnabled** (`bool`)
    Indicates whether the service is configured to start automatically at boot.

*   **IsRunning** (`bool`)
    A convenience flag indicating whether the service process is currently active.

*   **ProcessId** (`int`)
    The PID of the main service process. Value is `0` if the service is not currently running.

*   **CpuUsagePercent** (`decimal`)
    The current CPU utilization of the service process, expressed as a percentage.

*   **MemoryUsageMb** (`long`)
    The current resident set size (RSS) memory consumption of the service process, in megabytes.

*   **HasFailed** (`bool`)
    Indicates if the service is in a failed or error state.

*   **FailureReason** (`string`)
    A human-readable explanation of the cause if `HasFailed` is true.

*   **ExitCode** (`int`)
    The process exit code, if the service has stopped or crashed.

*   **UptimeSeconds** (`long`)
    The total number of seconds the service has been running since the last start.

*   **HealthStatus** (`HealthStatus`)
    An enumerated value representing the computed health status of the service (e.g., `Healthy`, `Degraded`, `Critical`).

*   **HealthMessage** (`string`)
    Additional context or diagnostic information provided to clarify the `HealthStatus`.

*   **RecordedAt** (`DateTime`)
    The precise timestamp indicating when this snapshot was captured.

## Usage

### Example 1: Displaying Service Health

This example demonstrates how to iterate through a list of statuses and output the current health to the console.

```csharp
public void PrintServiceReport(IEnumerable<ServiceStatus> statuses)
{
    foreach (var status in statuses)
    {
        Console.WriteLine($"Unit: {status.UnitName}");
        Console.WriteLine($"  Status: {status.HealthStatus} ({status.HealthMessage})");
        Console.WriteLine($"  CPU: {status.CpuUsagePercent}%, Mem: {status.MemoryUsageMb}MB");
        Console.WriteLine("-------------------------");
    }
}
```

### Example 2: Analyzing Failed Services

This example illustrates filtering a collection of status updates to identify and log failed services.

```csharp
public void LogServiceFailures(IEnumerable<ServiceStatus> statuses)
{
    var failedServices = statuses.Where(s => s.HasFailed);

    foreach (var failure in failedServices)
    {
        Console.Error.WriteLine($"ALERT: Service '{failure.UnitName}' failed!");
        Console.Error.WriteLine($"  Reason: {failure.FailureReason}");
        Console.Error.WriteLine($"  Exit Code: {failure.ExitCode}");
        Console.Error.WriteLine($"  Captured at: {failure.RecordedAt}");
    }
}
```

## Notes

*   **Thread Safety**: `ServiceStatus` is a data container (POCO) and does not inherently implement thread-safe access mechanisms. If instances are shared across multiple threads, appropriate synchronization should be implemented by the consuming code.
*   **Nullability**: The properties `FailureReason` and `HealthMessage` may contain `null` if no failure is present or no additional health context is available. Consuming code should handle potential null values gracefully.
*   **Process Information**: When `IsRunning` is `false`, `ProcessId` and `CpuUsagePercent` may return `0`. Always check `IsRunning` before relying on process-specific telemetry.
*   **Timestamping**: `RecordedAt` uses the time of snapshot capture, not the time of processing. Be aware of potential clock skews if aggregating data across different nodes.
