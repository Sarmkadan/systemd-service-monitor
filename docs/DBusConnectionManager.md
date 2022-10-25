# DBusConnectionManager

`DBusConnectionManager` is responsible for establishing, monitoring, and maintaining a connection to a D-Bus message bus within the `systemd-service-monitor` project. It provides mechanisms to retrieve active connection handles, check current connectivity status, and perform manual reconnection attempts if the connection is lost or enters an error state.

## API

### Constructors

*   **`public DBusConnectionManager()`**
    Initializes a new instance of the `DBusConnectionManager` class.

### Methods

*   **`public async Task<Connection> GetConnectionAsync()`**
    Retrieves the active D-Bus `Connection` object. If no connection exists, it attempts to initialize one. Returns the `Connection` instance. Throws `InvalidOperationException` or relevant D-Bus connection exceptions if initialization fails.

*   **`public async Task<bool> ReconnectAsync()`**
    Attempts to forcefully close the existing connection (if any) and establish a new one. Returns `true` if the connection was successfully re-established, otherwise `false`.

*   **`public async Task<bool> IsConnectedAsync()`**
    Checks the underlying transport status to verify if the D-Bus connection is currently active. Returns `true` if connected, otherwise `false`.

*   **`public async Task<ConnectionStatusInfo> GetStatusAsync()`**
    Retrieves a snapshot of the current connection status, including internal state and metrics. Returns a `ConnectionStatusInfo` object.

*   **`public void Dispose()`**
    Releases all resources used by the `DBusConnectionManager`, including closing the active D-Bus connection.

### Properties

*   **`public bool IsConnected`**
    Gets a value indicating whether the manager currently believes the connection is active based on the last internal check.

*   **`public string? State`**
    Gets a human-readable representation of the current connection state (e.g., "Connected", "Disconnected", "Connecting").

*   **`public DateTime LastStatusCheck`**
    Gets the timestamp of the last time the connection status was verified.

*   **`public string? ErrorMessage`**
    Gets the error message associated with the last failed connection attempt or runtime error, if any. Returns `null` if there is no error.

*   **`public int ReconnectAttempts`**
    Gets the total number of reconnection attempts made since the manager was initialized.

## Usage

### Retrieving and Using the Connection
```csharp
var manager = new DBusConnectionManager();
try 
{
    var connection = await manager.GetConnectionAsync();
    // Perform D-Bus operations with the returned connection
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to get D-Bus connection: {ex.Message}");
}
```

### Checking Status and Handling Reconnection
```csharp
var manager = new DBusConnectionManager();
if (!await manager.IsConnectedAsync())
{
    Console.WriteLine("Connection lost. Attempting to reconnect...");
    bool success = await manager.ReconnectAsync();
    if (success)
    {
        Console.WriteLine("Reconnected successfully.");
    }
}
```

## Notes

*   **Thread Safety:** The `DBusConnectionManager` is designed to be used in asynchronous contexts. While instance methods are generally thread-safe regarding internal state updates, callers should ensure that `Dispose()` is not called while other asynchronous operations are actively using the connection.
*   **Resource Management:** Always call `Dispose()` when the `DBusConnectionManager` is no longer required to ensure the underlying D-Bus connection is cleanly terminated.
*   **Asynchronous Behavior:** Methods returning `Task` perform I/O-bound operations related to the D-Bus socket. These should be awaited properly to avoid blocking the calling thread.
