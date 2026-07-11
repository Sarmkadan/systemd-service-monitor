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

// ... existing content ...
