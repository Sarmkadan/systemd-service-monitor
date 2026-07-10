# ResourceMonitorService

A service for monitoring system and service resource consumption, providing both one-off measurements and continuous collection of metrics such as CPU, memory, and derived resource alerts.

## API

### `ResourceMonitorService`
Initializes a new instance of the resource monitoring service. No parameters are required for construction; the service uses default system probes and configuration.

### `public async Task<SystemResource> GetSystemResourcesAsync()`
Retrieves a snapshot of overall system resource usage, including total CPU, memory, and disk metrics. Returns a `SystemResource` object with aggregated values. Throws `IOException` if the underlying system probe fails to read metrics.

### `public async Task<decimal> GetServiceCpuUsageAsync()`
Computes the CPU usage percentage consumed by the current service process. Returns a value between `0.0` and `100.0`. Throws `InvalidOperationException` if the process ID cannot be resolved or if CPU metrics are unavailable.

### `public async Task<long> GetServiceMemoryUsageAsync()`
Returns the current memory usage in bytes for the service process. Throws `InvalidOperationException` if the process memory cannot be measured.

### `public async Task<ServiceResourceMetrics> GetServiceResourceMetricsAsync()`
Gathers combined CPU and memory metrics for the service into a single `ServiceResourceMetrics` object. Returns a populated metrics object. Throws `InvalidOperationException` if any underlying metric probe fails.

### `public async Task<IEnumerable<ServiceResourceMetrics>> CollectAllMetricsAsync()`
Collects resource metrics for all monitored services in the system. Returns an enumerable of `ServiceResourceMetrics`, one per service. Throws `IOException` if system-level monitoring fails.

### `public async Task StartContinuousMonitoringAsync()`
Begins periodic collection and aggregation of resource metrics at a default interval. Subsequent calls have no effect. Does not throw; failures are logged internally.

### `public async Task StopContinuousMonitoringAsync()`
Stops the periodic collection started by `StartContinuousMonitoringAsync()`. Subsequent calls have no effect. Does not throw.

### `public async Task<IEnumerable<ResourceAlert>> GetResourceAlertsAsync()`
Retrieves a list of active resource alerts based on configured thresholds. Returns an enumerable of `ResourceAlert` objects describing exceeded limits. Throws `InvalidOperationException` if alert thresholds are misconfigured.

## Usage
