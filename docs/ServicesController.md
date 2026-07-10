# ServicesController

`ServicesController` provides a RESTful API surface for querying and controlling systemd services on the host machine. It exposes endpoints to list all monitored services, retrieve detailed state for a single service, execute lifecycle operations (start, stop, restart, reload, enable, disable), and perform bulk restart actions against multiple services simultaneously. The controller relies on the `ServiceNames` collection to define which units are under management and uses `MaxConcurrency` to limit parallel execution of bulk operations.

## API

### `IEnumerable<string> ServiceNames`

Gets the set of systemd service unit names that this controller instance is configured to monitor and manage. Each string is a valid systemd unit name (e.g., `"nginx.service"`, `"sshd.service"`). This collection drives enumeration in `GetAllServices` and constrains which services are eligible for lifecycle operations.

### `int MaxConcurrency`

Gets the maximum number of services that may be acted upon concurrently during bulk operations such as `BulkRestartServices`. When a bulk request targets more services than this limit, the controller processes them in bounded parallel batches to avoid overwhelming the host’s systemd daemon.

### `async Task<ActionResult<ApiResponse<List<ServiceInfo>>>> GetAllServices()`

Retrieves a list of `ServiceInfo` objects for every service name present in `ServiceNames`. Each `ServiceInfo` includes the current active state, substate, load state, unit file status, description, and any recent log excerpts.

- **Returns**: `200 OK` with an `ApiResponse` whose `Data` property contains a `List<ServiceInfo>`. If no services are configured, the list is empty.
- **Throws**: May propagate `InvalidOperationException` if the underlying systemd communication layer is unavailable. Transient D-Bus errors are surfaced as HTTP 5xx responses.

### `async Task<ActionResult<ApiResponse<ServiceInfo>>> GetServiceDetails(string serviceName)`

Fetches detailed state for a single service identified by `serviceName`.

- **Parameters**:
  - `serviceName` (`string`): The full unit name (e.g., `"cron.service"`). Must exist in `ServiceNames`; otherwise the call is rejected.
- **Returns**: `200 OK` with an `ApiResponse` containing the `ServiceInfo`. Returns `404 Not Found` when `serviceName` is not present in `ServiceNames`.
- **Throws**: Same D-Bus communication exceptions as `GetAllServices`.

### `async Task<ActionResult<ApiResponse<bool>>> StartService(string serviceName)`

Instructs systemd to start the specified service unit.

- **Parameters**:
  - `serviceName` (`string`): The unit to start. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `ApiResponse.Data` set to `true` if the start command was successfully enqueued with systemd. Returns `404 Not Found` if the service is not in `ServiceNames`. Returns `409 Conflict` if the service is already active.
- **Throws**: Propagates D-Bus errors as HTTP 5xx. Throws `ArgumentException` for a null or whitespace `serviceName`.

### `async Task<ActionResult<ApiResponse<bool>>> StopService(string serviceName)`

Instructs systemd to stop the specified service unit.

- **Parameters**:
  - `serviceName` (`string`): The unit to stop. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `ApiResponse.Data` set to `true` on success. `404 Not Found` if the service is not managed. `409 Conflict` if the service is already inactive.
- **Throws**: Same error conditions as `StartService`.

### `async Task<ActionResult<ApiResponse<bool>>> RestartService(string serviceName)`

Instructs systemd to restart the specified service unit. This is a stop followed by a start, preserving the unit’s process context where applicable.

- **Parameters**:
  - `serviceName` (`string`): The unit to restart. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `true` on success. `404 Not Found` if not managed. `409 Conflict` if the service is in a transitional state that prevents restart.
- **Throws**: Same error conditions as `StartService`.

### `async Task<ActionResult<ApiResponse<bool>>> ReloadService(string serviceName)`

Instructs systemd to reload the service’s configuration without interrupting its process. This is only meaningful for services that support the `ExecReload` directive.

- **Parameters**:
  - `serviceName` (`string`): The unit to reload. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `true` on success. `404 Not Found` if not managed. `409 Conflict` if the service does not support reload or is not running.
- **Throws**: Same error conditions as `StartService`.

### `async Task<ActionResult<ApiResponse<bool>>> EnableService(string serviceName)`

Enables the service unit so that systemd starts it automatically at boot. This operation modifies the symlinks in the unit file directories.

- **Parameters**:
  - `serviceName` (`string`): The unit to enable. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `true` on success. `404 Not Found` if not managed.
- **Throws**: May throw `UnauthorizedAccessException` if the host process lacks filesystem permissions to manipulate unit symlinks. Propagates D-Bus errors as HTTP 5xx.

### `async Task<ActionResult<ApiResponse<bool>>> DisableService(string serviceName)`

Disables the service unit so that it no longer starts at boot. Removes the relevant symlinks.

- **Parameters**:
  - `serviceName` (`string`): The unit to disable. Must be present in `ServiceNames`.
- **Returns**: `200 OK` with `true` on success. `404 Not Found` if not managed.
- **Throws**: Same error conditions as `EnableService`.

### `async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkRestartServices(IEnumerable<string> serviceNames)`

Restarts multiple services in a controlled parallel batch, respecting the `MaxConcurrency` limit. Each service is validated against `ServiceNames` before execution.

- **Parameters**:
  - `serviceNames` (`IEnumerable<string>`): The collection of unit names to restart. Null or empty collections result in an immediate `400 Bad Request`.
- **Returns**: `200 OK` with a `BulkOperationResult` containing:
  - `Successful`: list of service names that restarted successfully.
  - `Failed`: list of service names with per-item error messages for those that could not be restarted.
  - `Skipped`: list of service names that were not present in `ServiceNames` and were therefore ignored.
- **Throws**: `ArgumentNullException` when `serviceNames` is null. D-Bus communication failures are captured per-item in the `Failed` list rather than thrown globally.

## Usage

### Example 1: Retrieving and conditionally restarting a service

```csharp
// Assume controller is injected via DI as ServicesController
var allServices = await controller.GetAllServices();
var target = allServices.Value.Data.FirstOrDefault(s => s.Name == "nginx.service");

if (target is not null && target.ActiveState != "active")
{
    var restartResult = await controller.RestartService("nginx.service");
    if (restartResult.Value.Data)
    {
        Console.WriteLine("nginx.service restarted successfully.");
    }
}
```

### Example 2: Bulk restart with concurrency control

```csharp
var candidates = new List<string> { "nginx.service", "sshd.service", "cron.service" };
var bulkResult = await controller.BulkRestartServices(candidates);

Console.WriteLine($"Succeeded: {bulkResult.Value.Data.Successful.Count}");
Console.WriteLine($"Failed: {bulkResult.Value.Data.Failed.Count}");
Console.WriteLine($"Skipped (not managed): {bulkResult.Value.Data.Skipped.Count}");

foreach (var failure in bulkResult.Value.Data.Failed)
{
    Console.WriteLine($"  {failure.ServiceName}: {failure.Error}");
}
```

## Notes

- **Service name validation**: Every lifecycle method that accepts a `serviceName` parameter validates it against the `ServiceNames` collection. Requests for services not in that collection receive `404 Not Found`, even if the unit exists on the system. This ensures the controller only operates on explicitly configured units.
- **Bulk operation concurrency**: `BulkRestartServices` uses `MaxConcurrency` to cap the number of parallel systemd calls. If the input list exceeds this value, the controller processes items in sliding windows. This prevents thread-pool exhaustion and systemd D-Bus queue overflow.
- **Thread safety**: The controller itself does not mutate `ServiceNames` or `MaxConcurrency` after construction; both are treated as immutable configuration values. Instance methods are safe to call concurrently from multiple HTTP requests. The underlying systemd communication layer is accessed via transient D-Bus connections per operation, avoiding shared mutable state.
- **Reload semantics**: `ReloadService` only succeeds for units with an `ExecReload` directive and an active process. Calling it on a stopped service or one without reload support yields `409 Conflict`.
- **Enable/Disable persistence**: `EnableService` and `DisableService` modify on-disk unit file symlinks. These changes survive reboots and are independent of the service’s current running state. The host process must have sufficient filesystem privileges (typically root) for these operations to succeed.
- **Error propagation**: Single-service methods throw or return error responses for immediate failures. `BulkRestartServices` adopts a best-effort approach, capturing per-item failures in the `Failed` collection so that one failing service does not abort the entire batch.
