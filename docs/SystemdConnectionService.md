# SystemdConnectionService
The `SystemdConnectionService` class provides a programmatic interface to interact with systemd, allowing applications to connect, verify connections, disconnect, call methods, subscribe to signals, and retrieve the systemd version. This enables developers to integrate their applications with systemd, leveraging its features and capabilities.

## API
* `public SystemdConnectionService`: The constructor for the `SystemdConnectionService` class, used to create a new instance.
* `public async Task<bool> ConnectAsync`: Establishes a connection to systemd. Returns `true` if the connection is successful, `false` otherwise. May throw exceptions if the connection attempt fails.
* `public async Task<bool> VerifyConnectionAsync`: Verifies the current connection to systemd. Returns `true` if the connection is valid, `false` otherwise. May throw exceptions if the verification attempt fails.
* `public async Task DisconnectAsync`: Disconnects from systemd. May throw exceptions if the disconnection attempt fails.
* `public async Task<T?> CallMethodAsync<T>`: Calls a method on the systemd connection, returning a result of type `T`. May throw exceptions if the method call fails.
* `public async Task SubscribeToSignalsAsync`: Subscribes to signals from systemd. May throw exceptions if the subscription attempt fails.
* `public async Task<string> GetSystemdVersionAsync`: Retrieves the version of systemd. Returns the version as a string. May throw exceptions if the version retrieval attempt fails.

## Usage
The following examples demonstrate how to use the `SystemdConnectionService` class:
```csharp
// Example 1: Connect and verify connection
var service = new SystemdConnectionService();
if (await service.ConnectAsync())
{
    if (await service.VerifyConnectionAsync())
    {
        Console.WriteLine("Connected and verified");
    }
    else
    {
        Console.WriteLine("Connection not verified");
    }
}
else
{
    Console.WriteLine("Connection failed");
}

// Example 2: Call a method and handle result
var service = new SystemdConnectionService();
if (await service.ConnectAsync())
{
    var result = await service.CallMethodAsync<string>("GetSystemState");
    if (result != null)
    {
        Console.WriteLine($"System state: {result}");
    }
    else
    {
        Console.WriteLine("Method call failed");
    }
}
```

## Notes
When using the `SystemdConnectionService` class, consider the following:
* The class is not thread-safe, and concurrent access to its members may result in unexpected behavior. Synchronize access to the class instance to ensure thread safety.
* The `ConnectAsync` and `VerifyConnectionAsync` methods may throw exceptions if the connection or verification attempt fails. Handle these exceptions accordingly to ensure robust error handling.
* The `CallMethodAsync` method returns a nullable result, indicating that the method call may fail or return no result. Check the result for null before attempting to use it.
* The `SubscribeToSignalsAsync` method may throw exceptions if the subscription attempt fails. Handle these exceptions to ensure that the application remains stable.
* The `GetSystemdVersionAsync` method returns the systemd version as a string. Be aware that the version format may change between systemd releases, and parse the version string accordingly.
