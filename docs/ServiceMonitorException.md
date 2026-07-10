# ServiceMonitorException

`ServiceMonitorException` is the base exception type for the `systemd-service-monitor` project, providing specialized exception types for service monitoring operations. It serves as the foundation for more specific exceptions related to service discovery, DBus communication, permission checks, and log access failures.

## API

### Constructors

#### `ServiceMonitorException(string message)`
Initializes a new instance of the `ServiceMonitorException` class with a specified error message.
- **Parameters**:
  - `message` (string): The message that describes the error.
- **Remarks**: This constructor is inherited by all derived exception types in the hierarchy.

---

### Properties

#### `ServiceName` (string)
Gets the name of the systemd service associated with the exception.
- **Remarks**: This property is available on exceptions related to service operations (`ServiceNotFoundException`, `ServiceOperationException`). The value is `null` for exceptions unrelated to a specific service.

#### `BusName` (string?)
Gets the DBus bus name involved in the operation that caused the exception.
- **Remarks**: This property is available on `DBusConnectionException` and related types. The value may be `null` if the bus name is not applicable or could not be determined.

#### `RequiredPermission` (string)
Gets the permission required to perform the operation that caused the exception.
- **Remarks**: This property is available on `InsufficientPermissionsException`. The value indicates the missing permission that would have allowed the operation to succeed.

#### `Operation` (string)
Gets the systemd operation that failed.
- **Remarks**: This property is available on `ServiceOperationException`. The value describes the specific operation (e.g., "start", "stop", "status") that could not be completed.

---

### Exception Types

#### `ServiceNotFoundException`
Indicates that the specified systemd service could not be found.
- **Inherits**: `ServiceMonitorException`
- **Properties**: Inherits `ServiceName` (non-null).
- **Remarks**: Thrown when attempting to interact with a service that does not exist in the systemd environment.

#### `DBusConnectionException`
Indicates a failure in establishing or maintaining a DBus connection to systemd.
- **Inherits**: `ServiceMonitorException`
- **Properties**: Inherits `BusName` (may be null).
- **Remarks**: Thrown when DBus communication with systemd fails, such as during service status checks or control operations.

#### `InsufficientPermissionsException`
Indicates that the current process lacks the necessary permissions to perform the requested operation.
- **Inherits**: `ServiceMonitorException`
- **Properties**: Inherits `RequiredPermission` (non-null).
- **Remarks**: Thrown when the application attempts to perform an operation without adequate privileges, such as querying service status without appropriate DBus permissions.

#### `ServiceOperationException`
Indicates that an operation on a systemd service failed.
- **Inherits**: `ServiceMonitorException`
- **Properties**: Inherits `ServiceName` (non-null) and `Operation` (non-null).
- **Remarks**: Thrown when a systemd service operation (e.g., start, stop, restart) cannot be completed successfully.

#### `LogAccessException`
Indicates a failure to access or read service logs.
- **Inherits**: `ServiceMonitorException`
- **Remarks**: Thrown when the application cannot retrieve logs for a service, typically due to permission issues or log file corruption.

---

## Usage

### Example 1: Handling service not found
```csharp
try
{
    var service = await serviceMonitor.GetServiceAsync("non-existent.service");
}
catch (ServiceNotFoundException ex) when (ex.ServiceName == "non-existent.service")
{
    Console.WriteLine($"Service '{ex.ServiceName}' not found.");
    // Attempt recovery or fallback logic
}
```

### Example 2: Handling insufficient permissions
```csharp
try
{
    await serviceMonitor.StartServiceAsync("example.service");
}
catch (InsufficientPermissionsException ex)
{
    Console.WriteLine($"Missing permission: {ex.RequiredPermission}");
    // Request elevated privileges or notify user
}
```

## Notes

- **Thread Safety**: All exception types are immutable and thread-safe by design. Properties are read-only, and constructors are the only means of state initialization.
- **Null Handling**: Properties like `BusName` and `ServiceName` may return `null` when the context does not provide a meaningful value. Always check for `null` when the property is not guaranteed to be non-null.
- **Error Context**: When catching derived exceptions, prefer pattern matching or explicit type checks to handle specific failure modes (e.g., `ServiceNotFoundException` vs. `DBusConnectionException`).
- **Log Correlation**: These exceptions are designed to be logged using structured logging. Include the exception type and relevant properties (e.g., `ServiceName`, `BusName`) in log entries for diagnostic purposes.
