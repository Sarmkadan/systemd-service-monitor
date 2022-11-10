# ServiceControlService

`ServiceControlService` provides an asynchronous interface for managing systemd services, including starting, stopping, restarting, enabling, and querying service status. It encapsulates systemd's `systemctl` commands in a structured, awaitable API suitable for integration into .NET applications requiring service lifecycle management.

## API

### `public ServiceControlService`

Constructs a new instance of the service control client. No parameters are required for initialization; the service name and systemd connection details are configured via properties or dependency injection.

### `public async Task<bool> StartServiceAsync`

Starts the associated systemd service asynchronously. Returns `true` if the service was successfully started or was already running. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> StopServiceAsync`

Stops the associated systemd service asynchronously. Returns `true` if the service was successfully stopped or was not running. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> RestartServiceAsync`

Restarts the associated systemd service asynchronously. Returns `true` if the service was successfully restarted. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> ReloadServiceAsync`

Reloads the configuration of the associated systemd service asynchronously without interrupting active processes. Returns `true` if the reload was successful or the service does not support reloading. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> EnableServiceAsync`

Enables the associated systemd service to start at boot asynchronously. Returns `true` if the service was successfully enabled or was already enabled. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> DisableServiceAsync`

Disables the associated systemd service from starting at boot asynchronously. Returns `true` if the service was successfully disabled or was already disabled. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> RestartWithStrategyAsync`

Restarts the service using systemd's restart strategy (e.g., respects `RestartSec` and `Restart=` directives in the unit file). Returns `true` if the restart was initiated successfully. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<bool> GracefulShutdownAsync`

Initiates a graceful shutdown of the service by sending `SIGTERM` and waiting for the service to terminate cleanly. Returns `true` if the shutdown was initiated successfully. Throws `InvalidOperationException` if the service name is not set or the underlying systemd call fails. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

### `public async Task<OperationResult?> GetLastOperationStatusAsync`

Retrieves the status of the most recent operation performed by this instance. Returns `null` if no operations have been executed or the status is unavailable. The result includes success/failure status, timestamps, and any error messages.

### `public async Task<BulkOperationResult> BulkRestartAsync`

Restarts multiple services asynchronously in a single coordinated operation. Returns a `BulkOperationResult` containing per-service outcomes, including success/failure status and error details. Throws `InvalidOperationException` if any service name in the collection is invalid or empty. Throws `OperationCanceledException` if the operation is canceled via the provided `CancellationToken`.

## Usage
