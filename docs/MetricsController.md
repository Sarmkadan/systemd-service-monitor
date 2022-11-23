# MetricsController

`MetricsController` exposes HTTP endpoints for querying real-time system and service resource metrics in the `systemd-service-monitor` project. It provides aggregated views of CPU, memory, disk, and network utilization at both the host level and per-service granularity, returning structured API responses suitable for dashboard consumption or automated alerting pipelines.

## API

### GetSystemMetrics

```csharp
public async Task<ActionResult<ApiResponse<SystemResource>>> GetSystemMetrics()
```

Retrieves the current host-level resource snapshot, including total and available CPU, memory, and optionally disk/network aggregates.

- **Parameters**: None (derives context from the current host).
- **Return value**: `200 OK` with an `ApiResponse<SystemResource>` payload containing the system-wide resource counters. The `SystemResource` object includes CPU percentage, total/used/free memory in bytes, and load averages where available.
- **Throws**: No exceptions are thrown to the caller; internal failures are captured and surfaced as `ApiResponse` error statuses with appropriate HTTP status codes (typically `500` or `503` when the underlying collector is unreachable).

### GetServiceMetrics

```csharp
public async Task<ActionResult<ApiResponse<ServiceMetric>>> GetServiceMetrics(string serviceName)
```

Returns the current resource metrics for a single systemd service identified by `serviceName`.

- **Parameters**:
  - `serviceName` (`string`): The exact systemd unit name (e.g., `nginx.service`).
- **Return value**: `200 OK` with an `ApiResponse<ServiceMetric>` containing CPU usage, memory consumption, and I/O counters for the specified service. Returns `404 Not Found` when the service is not recognized or not currently running.
- **Throws**: `ArgumentException` if `serviceName` is null or whitespace. Internal collection failures produce `ApiResponse` error payloads with `500` or `503`.

### GetAllServiceMetrics

```csharp
public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetAllServiceMetrics()
```

Aggregates resource metrics for all monitored systemd services currently active on the host.

- **Parameters**: None.
- **Return value**: `200 OK` with an `ApiResponse<List<ServiceMetric>>` where each list entry corresponds to one running service. An empty list is returned when no services are being tracked.
- **Throws**: No direct exceptions; collector unavailability results in an error `ApiResponse` with `503`.

### GetTopMemoryConsumers

```csharp
public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetTopMemoryConsumers(int count = 5)
```

Returns a descending list of services ordered by current memory usage, limited to the top `count` entries.

- **Parameters**:
  - `count` (`int`, optional, default `5`): Maximum number of services to include.
- **Return value**: `200 OK` with an `ApiResponse<List<ServiceMetric>>` sorted by memory consumption descending. Fewer than `count` entries may be returned if fewer services are running.
- **Throws**: `ArgumentOutOfRangeException` when `count` is less than `1`. Internal errors follow the standard `ApiResponse` error pattern.

### GetTopCpuConsumers

```csharp
public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetTopCpuConsumers(int count = 5)
```

Returns a descending list of services ordered by current CPU usage, limited to the top `count` entries.

- **Parameters**:
  - `count` (`int`, optional, default `5`): Maximum number of services to include.
- **Return value**: `200 OK` with an `ApiResponse<List<ServiceMetric>>` sorted by CPU percentage descending. Fewer than `count` entries may be returned if fewer services are running.
- **Throws**: `ArgumentOutOfRangeException` when `count` is less than `1`. Internal errors follow the standard `ApiResponse` error pattern.

### GetServiceDiskMetrics

```csharp
public async Task<ActionResult<ApiResponse<object>>> GetServiceDiskMetrics(string serviceName)
```

Returns disk I/O statistics for the specified service, including read/write bytes and operation counts since the service started or the last reset interval.

- **Parameters**:
  - `serviceName` (`string`): The exact systemd unit name.
- **Return value**: `200 OK` with an `ApiResponse<object>` whose payload shape is a dynamic object containing `ReadBytes`, `WriteBytes`, `ReadOps`, and `WriteOps` fields. `404 Not Found` when the service is not tracked.
- **Throws**: `ArgumentException` for null/whitespace `serviceName`. Internal collection failures produce error `ApiResponse` payloads.

### GetServiceNetworkMetrics

```csharp
public async Task<ActionResult<ApiResponse<object>>> GetServiceNetworkMetrics(string serviceName)
```

Returns network I/O statistics for the specified service, including transmitted/received bytes and packet counts.

- **Parameters**:
  - `serviceName` (`string`): The exact systemd unit name.
- **Return value**: `200 OK` with an `ApiResponse<object>` whose payload shape is a dynamic object containing `RxBytes`, `TxBytes`, `RxPackets`, and `TxPackets` fields. `404 Not Found` when the service is not tracked.
- **Throws**: `ArgumentException` for null/whitespace `serviceName`. Internal collection failures produce error `ApiResponse` payloads.

## Usage

### Example 1: Fetching system overview and top memory consumers

```csharp
using var httpClient = new HttpClient { BaseAddress = new Uri("https://monitor.example.com") };

// Retrieve host-level metrics
var systemResponse = await httpClient.GetFromJsonAsync<ApiResponse<SystemResource>>(
    "/api/metrics/system");
Console.WriteLine($"CPU: {systemResponse.Data.CpuPercent}%");
Console.WriteLine($"Memory free: {systemResponse.Data.MemoryFreeBytes} bytes");

// Get the top 3 services by memory usage
var topMemResponse = await httpClient.GetFromJsonAsync<ApiResponse<List<ServiceMetric>>>(
    "/api/metrics/services/top-memory?count=3");
foreach (var svc in topMemResponse.Data)
{
    Console.WriteLine($"{svc.ServiceName}: {svc.MemoryUsageBytes} bytes");
}
```

### Example 2: Monitoring a specific service's disk and network activity

```csharp
using var httpClient = new HttpClient { BaseAddress = new Uri("https://monitor.example.com") };

string targetService = "postgresql.service";

// Fetch disk metrics
var diskResponse = await httpClient.GetFromJsonAsync<ApiResponse<JsonElement>>(
    $"/api/metrics/services/{targetService}/disk");
if (diskResponse.IsSuccess)
{
    var disk = diskResponse.Data;
    Console.WriteLine($"Read: {disk.GetProperty("ReadBytes").GetInt64()} bytes");
    Console.WriteLine($"Write: {disk.GetProperty("WriteBytes").GetInt64()} bytes");
}

// Fetch network metrics
var netResponse = await httpClient.GetFromJsonAsync<ApiResponse<JsonElement>>(
    $"/api/metrics/services/{targetService}/network");
if (netResponse.IsSuccess)
{
    var net = netResponse.Data;
    Console.WriteLine($"RX: {net.GetProperty("RxBytes").GetInt64()} bytes");
    Console.WriteLine($"TX: {net.GetProperty("TxBytes").GetInt64()} bytes");
}
```

## Notes

- **Edge cases**: `GetServiceMetrics`, `GetServiceDiskMetrics`, and `GetServiceNetworkMetrics` return `404` when the requested service is not running or not recognized by the monitor. The `GetTopMemoryConsumers` and `GetTopCpuConsumers` endpoints may return fewer entries than requested if the total number of tracked services is below the `count` parameter. Passing a `count` of zero or negative throws `ArgumentOutOfRangeException` on the server side.
- **Dynamic payloads**: `GetServiceDiskMetrics` and `GetServiceNetworkMetrics` return `object` typed as dynamic JSON structures. Consumers should deserialize into `JsonElement` or a custom POCO matching the documented field names to avoid runtime casting errors.
- **Thread safety**: The controller itself is stateless; all metric collection is delegated to underlying services that manage their own synchronization. Concurrent requests are safe and will not corrupt shared state. No instance fields are mutated during request handling.
- **Error handling**: Internal failures (e.g., collector process crash, permission errors reading cgroups) are not propagated as exceptions to HTTP clients. They are wrapped in `ApiResponse` objects with `IsSuccess = false` and an appropriate error message, accompanied by HTTP status codes `500` or `503`. Clients should always check `ApiResponse.IsSuccess` before accessing `Data`.
