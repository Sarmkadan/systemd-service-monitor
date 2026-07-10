# ServiceMonitorClient

`ServiceMonitorClient` provides a client-side API for interacting with the systemd-service-monitor service. It encapsulates HTTP communication with the monitoring backend, offering typed methods to query service states, retrieve logs, inspect system resources, and issue lifecycle commands (start, stop, restart). The class also exposes properties that reflect the health and identity of the client connection itself.

## API

### Constructors

**`public ServiceMonitorClient`**

Creates a new instance of the client. The specific constructor overloads (e.g., accepting a base URL, an `HttpClient` instance, or configuration options) are determined by the project’s dependency injection and configuration patterns. Consult the project source for exact signatures.

### Methods

**`public async Task<List<ServiceInfo>> GetServicesAsync`**

Retrieves a list of all monitored systemd services and their summary information.

- **Returns:** A `List<ServiceInfo>` where each entry contains the service’s `Name`, `DisplayName`, `State`, and `IsActive` status.
- **Throws:** `HttpRequestException` on network failure; `TaskCanceledException` on timeout; may throw deserialization exceptions if the response payload is malformed.

**`public async Task<ServiceDetails> GetServiceDetailsAsync`**

Fetches detailed information for a specific service.

- **Parameters:** Accepts a service identifier (typically the unit name as a `string`; consult the project source for the exact parameter name and type).
- **Returns:** A `ServiceDetails` object containing `Name`, `State`, `Description`, `IsActive`, `UptimeSeconds`, and `Pid`.
- **Throws:** `ArgumentException` if the identifier is null or empty; `HttpRequestException` on network failure; a not-found exception (type defined by the project) if the service does not exist.

**`public async Task<bool> StartServiceAsync`**

Sends a start command to the specified service.

- **Parameters:** Accepts a service identifier (typically the unit name as a `string`).
- **Returns:** `true` if the service was started successfully or is already running; `false` if the operation was rejected by the backend.
- **Throws:** `ArgumentException` for invalid input; `HttpRequestException` on network failure; may throw an authorization exception if the backend enforces access control.

**`public async Task<bool> StopServiceAsync`**

Sends a stop command to the specified service.

- **Parameters:** Accepts a service identifier (typically the unit name as a `string`).
- **Returns:** `true` if the service was stopped successfully or is already inactive; `false` if the operation was rejected.
- **Throws:** `ArgumentException` for invalid input; `HttpRequestException` on network failure; may throw an authorization exception.

**`public async Task<bool> RestartServiceAsync`**

Sends a restart command to the specified service.

- **Parameters:** Accepts a service identifier (typically the unit name as a `string`).
- **Returns:** `true` if the restart sequence completed successfully; `false` if the operation was rejected.
- **Throws:** `ArgumentException` for invalid input; `HttpRequestException` on network failure; may throw an authorization exception.

**`public async Task<List<LogEntry>> GetServiceLogsAsync`**

Retrieves log entries for a service.

- **Parameters:** Accepts a service identifier and optionally filtering parameters (e.g., time range, maximum entry count, severity level). Consult the project source for the exact parameter set.
- **Returns:** A `List<LogEntry>` containing timestamped log records with message content and severity.
- **Throws:** `ArgumentException` for invalid input; `HttpRequestException` on network failure; may throw if the log backend is unavailable.

**`public async Task<SystemResources> GetSystemResourcesAsync`**

Queries current system resource utilization from the monitored host.

- **Returns:** A `SystemResources` object containing CPU, memory, disk, or other resource metrics (exact properties depend on the project’s model definition).
- **Throws:** `HttpRequestException` on network failure; `TaskCanceledException` on timeout.

**`public async Task<bool> IsHealthyAsync`**

Performs a health check against the monitoring backend.

- **Returns:** `true` if the backend reports a healthy status; `false` otherwise.
- **Throws:** `HttpRequestException` on network failure; does not throw on a negative health response (returns `false`).

**`public void Dispose`**

Releases all resources held by the client, including the underlying HTTP connection if the client owns it. After disposal, all method calls throw `ObjectDisposedException`.

### Properties

**`public string Name`**

Gets the name of the client instance or the connected backend service, depending on context. In `ServiceInfo` and `ServiceDetails` contexts, this represents the systemd unit name.

**`public string DisplayName`**

Gets the human-readable display name of a service (available in `ServiceInfo`).

**`public string State`**

Gets the current state string of a service (e.g., `"active"`, `"inactive"`, `"failed"`). Available in both `ServiceInfo` and `ServiceDetails`.

**`public bool IsActive`**

Gets whether the service is currently in an active state. Available in both `ServiceInfo` and `ServiceDetails`.

**`public string Description`**

Gets the descriptive text for a service (available in `ServiceDetails`).

**`public int UptimeSeconds`**

Gets the number of seconds the service has been continuously active (available in `ServiceDetails`).

**`public int Pid`**

Gets the main process ID of the service, if applicable (available in `ServiceDetails`).

## Usage

### Example 1: Monitoring and restarting a failed service

```csharp
using var client = new ServiceMonitorClient("https://monitor.example.com");

// Check backend health first
if (!await client.IsHealthyAsync())
{
    Console.WriteLine("Monitoring backend is unhealthy. Aborting.");
    return;
}

// Inspect a specific service
var details = await client.GetServiceDetailsAsync("nginx.service");

if (!details.IsActive)
{
    Console.WriteLine($"nginx is {details.State}. Attempting restart...");
    bool restarted = await client.RestartServiceAsync("nginx.service");
    Console.WriteLine(restarted ? "Restart command accepted." : "Restart rejected.");
}
else
{
    Console.WriteLine($"nginx is active (PID {details.Pid}, uptime {details.UptimeSeconds}s).");
}
```

### Example 2: Gathering system overview and logs

```csharp
using var client = new ServiceMonitorClient("https://monitor.example.com");

// Get all services and system resources in parallel
var servicesTask = client.GetServicesAsync();
var resourcesTask = client.GetSystemResourcesAsync();

await Task.WhenAll(servicesTask, resourcesTask);

var services = servicesTask.Result;
var resources = resourcesTask.Result;

Console.WriteLine($"CPU: {resources.CpuUsage}%, Memory: {resources.MemoryUsage}%");
Console.WriteLine($"Total services: {services.Count}");

// Fetch logs for any failed services
foreach (var svc in services.Where(s => s.State == "failed"))
{
    var logs = await client.GetServiceLogsAsync(svc.Name, maxEntries: 10);
    Console.WriteLine($"--- Logs for {svc.DisplayName} ---");
    foreach (var entry in logs)
    {
        Console.WriteLine($"[{entry.Timestamp}] {entry.Severity}: {entry.Message}");
    }
}
```

## Notes

- **Thread safety:** Instance methods are not guaranteed to be thread-safe. A single `ServiceMonitorClient` instance should be used from one thread at a time, or protected by external synchronization. For concurrent scenarios, create separate instances or rely on an `IHttpClientFactory`-managed pool.
- **Disposal:** `Dispose` must be called when the client is no longer needed, especially if it owns the underlying `HttpClient`. Failure to dispose may lead to socket exhaustion. The `using` statement pattern is recommended.
- **Network resilience:** All async methods can throw `HttpRequestException` or timeout-related exceptions. Callers should implement retry logic with exponential backoff for production scenarios.
- **Property context:** The properties `Name`, `State`, `IsActive`, `DisplayName`, `Description`, `UptimeSeconds`, and `Pid` belong to the returned model types (`ServiceInfo` and `ServiceDetails`), not directly to `ServiceMonitorClient` itself. They are accessed on the objects returned by `GetServicesAsync` and `GetServiceDetailsAsync`.
- **Service identification:** Methods accepting a service identifier expect the systemd unit name (e.g., `"nginx.service"`). Passing an incorrect or non-existent name results in a not-found error from the backend.
- **Health check semantics:** `IsHealthyAsync` returns `false` for a degraded backend rather than throwing, allowing callers to gracefully degrade functionality without exception handling overhead.
