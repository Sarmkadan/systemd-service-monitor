# `ISystemdConnectionService`

`ISystemdConnectionService` defines the connection-lifecycle boundary between the monitoring services and systemd's system D-Bus. Its default implementation, `SystemdConnectionService`, tracks a local connected state and delegates ownership of the underlying `Tmds.DBus.Connection` to `DBusConnectionManager`.

The interface is defined in `Services/ISystemdConnectionService.cs`; the default implementation is in `Services/SystemdConnectionService.cs`.

## Registration and dependencies

The application registers `ISystemdConnectionService` as a scoped service implemented by `SystemdConnectionService`. `DBusConnectionManager`, which owns and reuses the actual D-Bus connection, is registered separately as a singleton.

`SystemdConnectionService` requires:

- `ILogger<SystemdConnectionService>` for connection, verification, and failure events.
- `SystemdOptions`. The implementation retains these options, but currently does not read any of their values.
- `DBusConnectionManager` for connection creation, health checks, reconnection, and access to the underlying connection.

The constructor throws `ArgumentNullException` when any dependency is null.

## Connection state

### `bool IsConnected`

Reports the implementation's local connection flag. It becomes `true` after `ConnectAsync` obtains a connection and becomes `false` after `DisconnectAsync` or a failed verification. It is not a live query of `DBusConnectionManager`, so the underlying connection can fail before this property changes.

### `DateTime? ConnectedSince`

Contains the UTC time at which the latest successful `ConnectAsync` call established the local connected state. It is null before connection and after disconnect or failed verification. Calling `ConnectAsync` while already locally connected preserves the original timestamp.

### `DBusConnectionManager DBusConnectionManager`

Exposes the injected manager. Other services use this property to obtain the underlying `Tmds.DBus.Connection` and create strongly typed D-Bus proxies. Disposing the connection is the manager's responsibility, not this service's.

The state fields in `SystemdConnectionService` are not synchronized. Callers should not rely on atomic lifecycle transitions when the same scoped instance is used concurrently.

## Lifecycle methods

### `ConnectAsync(CancellationToken ct = default)`

If the local state is already connected, returns `true` without consulting the manager. Otherwise, it calls `DBusConnectionManager.GetConnectionAsync()`, sets `IsConnected` to `true`, records `DateTime.UtcNow` in `ConnectedSince`, and returns `true`.

If obtaining the connection fails, the implementation logs the failure and throws `DBusConnectionException` with the original exception as its inner exception. It does not return `false` for this failure path. The cancellation token is currently not passed to the manager or otherwise inspected.

### `VerifyConnectionAsync(CancellationToken ct = default)`

Returns `false` immediately when the local state is disconnected. When locally connected, it first asks `DBusConnectionManager.IsConnectedAsync()` for the underlying status. A `false` result clears the local state and timestamp and returns `false`.

If the manager reports connected, verification retrieves the systemd version. It returns `true` when the returned version is nonempty and `false` when it is null or empty.

Any exception during manager verification or version retrieval clears the local state, calls `DBusConnectionManager.ReconnectAsync()`, and returns `false`. The reconnection result does not restore this service's local connected flag; a later `ConnectAsync` call is required to do that. Reconnection failures are handled by the manager and are not propagated by this method. The supplied token reaches `GetSystemdVersionAsync`, but the current implementation does not observe it.

### `DisconnectAsync(CancellationToken ct = default)`

If already locally disconnected, completes without doing anything. Otherwise, it clears the local state and timestamp. It does not close or dispose the underlying D-Bus connection because `DBusConnectionManager` owns that connection. The cancellation token is not inspected.

## D-Bus operations

### `GetSystemdVersionAsync(CancellationToken ct = default)`

Requires the local state to be connected; otherwise it throws `DBusConnectionException`. It obtains the manager's connection, creates an `ISystemdManagerProperties` proxy for:

- Service: `org.freedesktop.systemd1`
- Object path: `/org/freedesktop/systemd1`
- Interface: `org.freedesktop.systemd1.Manager`

It then invokes the proxy's `GetVersionAsync()` method and returns the string reported by systemd. Errors while obtaining the connection, creating the proxy, or reading the property are wrapped in `ServiceMonitorException`. The cancellation token is currently not inspected.

### `CallMethodAsync<T>(string methodName, params object?[] args)`

This API is a placeholder rather than a functional generic D-Bus dispatcher. A null `methodName` causes `ArgumentNullException`; a disconnected service causes `DBusConnectionException`. When connected, the method logs a warning and throws `NotImplementedException`. The supplied arguments are not used.

Consumers should obtain the manager connection through `DBusConnectionManager` and define a strongly typed Tmds.DBus proxy for the required systemd interface.

### `SubscribeToSignalsAsync(string signalName, Action<dynamic> handler, CancellationToken ct = default)`

This API is also a placeholder. A null signal name or handler causes `ArgumentNullException`; a disconnected service causes `DBusConnectionException`. When connected, it logs a warning and throws `NotImplementedException`. It does not register the handler, and the cancellation token is not inspected.

Consumers that need signals should define the appropriate strongly typed D-Bus interface and subscribe through a proxy obtained from the manager's connection.

## Example

The service is intended to be resolved through dependency injection:

```csharp
public sealed class SystemdVersionReader
{
    private readonly ISystemdConnectionService _connectionService;

    public SystemdVersionReader(ISystemdConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<string> ReadAsync(CancellationToken cancellationToken)
    {
        await _connectionService.ConnectAsync(cancellationToken);

        if (!await _connectionService.VerifyConnectionAsync(cancellationToken))
        {
            throw new InvalidOperationException("The systemd D-Bus connection could not be verified.");
        }

        return await _connectionService.GetSystemdVersionAsync(cancellationToken);
    }
}
```

The process must run on a system with systemd's system bus available and with sufficient D-Bus permissions for the requested operations.
