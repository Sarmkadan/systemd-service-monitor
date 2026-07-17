# DBusConnectionManagerExtensions

Extension methods for `DBusConnectionManager` that provide higher-level operations for managing and inspecting D-Bus connections, including connection state monitoring, detailed status reporting, and reconnection logic with failure diagnostics.

## API

### `GetConnectionStateAsync`

Asynchronously retrieves the current state of the D-Bus connection.

- **Returns**: A `Task<ConnectionState>` representing the current connection state.
- **Throws**: `ArgumentNullException` if the underlying connection is null.

### `GetExtendedStatusAsync`

Asynchronously retrieves a detailed status report of the D-Bus connection, including uptime, status age, and attempt history.

- **Returns**: A `Task<ExtendedConnectionStatusInfo>` containing detailed connection metrics and state.
- **Throws**: `ArgumentNullException` if the underlying connection is null.

### `ReconnectWithDetailsAsync`

Asynchronously attempts to reconnect the D-Bus connection and returns detailed information about the reconnection attempt.

- **Returns**: A `Task<IReadOnlyList<ReconnectionAttempt>>` containing the sequence of reconnection attempts made, including success or failure details for each.
- **Throws**: `InvalidOperationException` if the connection is not in a reconnectable state.

### `Uptime`

Gets the duration for which the connection has been active.

- **Type**: `TimeSpan?`
- **Remarks**: Returns `null` if the connection has not been established or if uptime is not available.

### `StatusAge`

Gets the time elapsed since the current connection status was last updated.

- **Type**: `TimeSpan`
- **Remarks**: Useful for detecting stale or unresponsive connections.

### `AttemptNumber`

Gets the current reconnection attempt number.

- **Type**: `int`
- **Remarks**: Starts at 1 for the first attempt after a failure. Returns 0 if no reconnection has been attempted.

### `Timestamp`

Gets the timestamp when the current status was recorded.

- **Type**: `DateTime`
- **Remarks**: Useful for correlating status changes with external events.

### `Completed`

Gets a value indicating whether the current operation (e.g., reconnection) has completed.

- **Type**: `bool`
- **Remarks**: Returns `true` once a reconnection attempt has succeeded or definitively failed.

### `Success`

Gets a value indicating whether the current operation (e.g., reconnection) succeeded.

- **Type**: `bool`
- **Remarks**: Only meaningful when `Completed` is `true`. Returns `false` if the operation failed.

### `Error`

Gets the error message associated with a failed operation.

- **Type**: `string?`
- **Remarks**: Returns `null` if the operation succeeded or is still in progress.

### `ExceptionType`

Gets the type name of the exception thrown during a failed operation.

- **Type**: `string?`
- **Remarks**: Returns `null` if no exception was thrown or if the operation succeeded.

## Usage

### Monitoring Connection State
