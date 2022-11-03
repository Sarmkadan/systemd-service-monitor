# ServiceStatusUpdateWorker

The `ServiceStatusUpdateWorker` is a core component within the `systemd-service-monitor` project responsible for periodically polling the status of managed systemd services and propagating updates to the monitoring system. It operates on a configurable interval, implementing exponential backoff strategies during failure scenarios to prevent resource exhaustion, while utilizing a time-based cache to optimize repeated queries and support batched processing of status changes.

## API

The following members constitute the public interface of the `ServiceStatusUpdateWorker`:

### `public ServiceStatusUpdateWorker`
Initializes a new instance of the `ServiceStatusUpdateWorker` class.
*   **Parameters**: None (assumes default configuration or dependency injection context).
*   **Return Value**: A new instance of the worker.
*   **Exceptions**: May throw initialization exceptions if underlying system dependencies (e.g., D-Bus connections) are unavailable at construction time.

### `public int UpdateIntervalMs`
Gets or sets the frequency, in milliseconds, at which the worker attempts to poll service statuses under normal operating conditions.
*   **Purpose**: Defines the standard heartbeat for status checks.
*   **Value**: An integer representing milliseconds. Must be greater than zero.
*   **Exceptions**: Setting a value less than or equal to zero may result in an `ArgumentOutOfRangeException` or undefined behavior depending on the runtime validation strategy.

### `public int ErrorBackoffMs`
Gets or sets the delay, in milliseconds, applied between retry attempts when a status polling operation fails.
*   **Purpose**: Prevents tight retry loops during transient system errors or systemd daemon unavailability.
*   **Value**: An integer representing milliseconds.
*   **Exceptions**: Setting a negative value will throw an `ArgumentOutOfRangeException`.

### `public TimeSpan CacheTtl`
Gets or sets the duration for which fetched service status data is considered valid before a fresh query is required.
*   **Purpose**: Reduces IPC overhead by serving stale-but-valid data for short durations during high-frequency access patterns.
*   **Value**: A `TimeSpan` structure representing the time-to-live.
*   **Exceptions**: Setting a negative `TimeSpan` will throw an `ArgumentOutOfRangeException`.

### `public int BatchSize`
Gets or sets the maximum number of service status updates to process or transmit in a single operation cycle.
*   **Purpose**: Controls throughput and memory usage when handling large fleets of services, ensuring the worker yields control periodically.
*   **Value**: An integer representing the count of items per batch.
*   **Exceptions**: Setting a value less than one will throw an `ArgumentOutOfRangeException`.

### `public bool VerboseLogging`
Gets or sets a value indicating whether detailed diagnostic information should be emitted to the logging infrastructure.
*   **Purpose**: Enables granular tracing of polling cycles, cache hits/misses, and backoff calculations for debugging purposes.
*   **Value**: `true` to enable verbose output; `false` for standard operational logging.
*   **Exceptions**: None.

## Usage

### Example 1: Basic Configuration
The following example demonstrates instantiating the worker and configuring standard polling intervals and caching behavior for a production environment.

```csharp
using System;
using SystemdServiceMonitor;

public class Program
{
    public static void Main()
    {
        var worker = new ServiceStatusUpdateWorker();
        
        // Set standard polling to every 5 seconds
        worker.UpdateIntervalMs = 5000;
        
        // Configure error backoff to 30 seconds to reduce load during outages
        worker.ErrorBackoffMs = 30000;
        
        // Cache results for 2 seconds to mitigate rapid duplicate queries
        worker.CacheTtl = TimeSpan.FromSeconds(2);
        
        // Process updates in batches of 50 services
        worker.BatchSize = 50;
        
        // Keep logging minimal in production
        worker.VerboseLogging = false;

        // Worker would typically be started by a hosting framework here
        Console.WriteLine("ServiceStatusUpdateWorker configured for production.");
    }
}
```

### Example 2: Debugging and Diagnostics
This example illustrates configuring the worker for a development or troubleshooting scenario where high-frequency updates and detailed logs are required.

```csharp
using System;
using SystemdServiceMonitor;

public class DiagnosticRunner
{
    public static void RunDiagnostics()
    {
        var worker = new ServiceStatusUpdateWorker();

        // Aggressive polling for real-time observation
        worker.UpdateIntervalMs = 500;
        
        // Minimal backoff to observe recovery speed immediately
        worker.ErrorBackoffMs = 1000;
        
        // Disable caching to ensure every read hits the systemd bus
        worker.CacheTtl = TimeSpan.Zero;
        
        // Small batch size to isolate processing latency per service
        worker.BatchSize = 5;
        
        // Enable verbose logging to trace internal state transitions
        worker.VerboseLogging = true;

        Console.WriteLine("ServiceStatusUpdateWorker configured for diagnostics.");
    }
}
```

## Notes

*   **Thread Safety**: The public properties of `ServiceStatusUpdateWorker` are not guaranteed to be thread-safe for concurrent writes. If configuration values need to be updated dynamically while the worker is running, external synchronization (e.g., a `lock` statement) must be employed to prevent race conditions where partial updates could lead to inconsistent polling states.
*   **Zero Cache TTL**: Setting `CacheTtl` to `TimeSpan.Zero` disables caching entirely. While useful for debugging, this significantly increases Inter-Process Communication (IPC) overhead with the systemd daemon and should be avoided in high-scale production deployments.
*   **Backoff Logic**: The `ErrorBackoffMs` value typically represents the base delay. Implementations often utilize exponential backoff multipliers based on consecutive failure counts; therefore, the actual delay between retries may exceed this configured value during sustained outages.
*   **Batching Behavior**: The `BatchSize` property influences how updates are grouped. If the number of pending updates is less than `BatchSize`, the worker processes the available items immediately without waiting to fill the batch, ensuring low latency for small update sets.
*   **Interval Validation**: Values for `UpdateIntervalMs` and `ErrorBackoffMs` must be positive. Setting these to zero or negative values will likely halt the worker loop or throw an exception upon the next configuration validation cycle.
