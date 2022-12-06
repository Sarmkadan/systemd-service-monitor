// ... existing content ...
## PaginationHelperTests

The `PaginationHelperTests` class provides unit tests for the `PaginationHelper` utility class, which centralizes common pagination logic used throughout the systemd-service-monitor application. These tests verify pagination rules for calculating total pages, validating pagination parameters, and retrieving page numbers to ensure data integrity and prevent invalid configurations.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;

// Calculate total pages with zero page size
var totalPages = PaginationHelper.CalculateTotalPages(10, 0);
Console.WriteLine($"Total pages: {totalPages}");

// Validate pagination parameters with negative page number
var paginationMetadata = PaginationHelper.GetMetadata(5, 10);
Console.WriteLine($"Start index: {paginationMetadata.StartIndex}, End index: {paginationMetadata.EndIndex}");

// Get page numbers near the end of total pages
var pageNumbers = PaginationHelper.GetPageNumbers(10, 5);
Console.WriteLine($"Page numbers: [{string.Join(", ", pageNumbers)}]");
```

## ServiceHealthCheckerTests

The `ServiceHealthCheckerTests` class verifies the behavior of the `ServiceHealthChecker` utility, ensuring that health status is correctly determined for various service states and that the health summary string is non-empty.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using SystemdServiceMonitor.Models;

// Create a sample ServiceInfo instance
var service = new ServiceInfo
{
    Name = "example.service",
    State = ServiceState.Active,
    RestartCount = 0,
    AutoStart = true
};

// Determine health status
var status = ServiceHealthChecker.GetHealthStatus(service);
Console.WriteLine($"Health status: {status}");

// Get a human‑readable summary
var summary = ServiceHealthChecker.GetHealthSummary(service);
Console.WriteLine($"Health summary: {summary}");
```

## DBusConnectionManager

The `DBusConnectionManager` class is responsible for managing connections to the DBus system. It provides methods to establish and manage connections, check connection status, and handle reconnections. The class also provides information about the current connection state, such as the last status check time and any error messages.

### Usage Example

```csharp
using SystemdServiceMonitor.Integration;

// Create a new DBusConnectionManager instance
var connectionManager = new DBusConnectionManager();

// Get the current connection status
var isConnected = await connectionManager.IsConnectedAsync();
Console.WriteLine($"Is connected: {isConnected}");

// Get the current connection status info
var statusInfo = await connectionManager.GetStatusAsync();
Console.WriteLine($"Connection status: {statusInfo}");

// Reconnect to the DBus system if necessary
var reconnectResult = await connectionManager.ReconnectAsync();
Console.WriteLine($"Reconnect result: {reconnectResult}");

// Dispose of the connection manager when finished
connectionManager.Dispose();
```

// ... existing content ...
