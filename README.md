// ... existing content ...
## ServiceMonitorException

The `ServiceMonitorException` is a custom exception type used to handle various errors that may occur during service monitoring. It provides a way to capture and handle specific exceptions related to service operations, such as insufficient permissions, log access errors, and dbus connection issues.

### Usage Example

```csharp
try
{
    // Attempt to start a service
    var service = await _serviceRepository.StartServiceAsync("nginx.service");
}
catch (ServiceMonitorException ex) when (ex is InsufficientPermissionsException)
{
    Console.WriteLine("Insufficient permissions to start the service.");
}
catch (ServiceMonitorException ex) when (ex is LogAccessException)
{
    Console.WriteLine("Error accessing service logs.");
}
catch (ServiceMonitorException ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
```

// ... existing content ...
