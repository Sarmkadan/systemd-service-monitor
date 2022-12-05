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

## ValidationHelperTests

The `ValidationHelperTests` class provides unit tests for the `ValidationHelper` utility class, which centralizes common validation logic used throughout the systemd-service-monitor application. These tests verify validation rules for service names, ports, URLs, time ranges, and input sanitization to ensure data integrity and prevent invalid configurations.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;

// Validate a service name
var serviceNameResult = ValidationHelper.ValidateServiceName("nginx.service");
if (!serviceNameResult.IsValid)
{
    Console.WriteLine($"Invalid service name: {serviceNameResult.ErrorMessage}");
}

// Validate a port number
var portResult = ValidationHelper.ValidatePort(8080);
if (!portResult.IsValid)
{
    Console.WriteLine($"Invalid port: {portResult.ErrorMessage}");
}

// Validate a URL
var urlResult = ValidationHelper.ValidateUrl("https://example.com/api");
if (!urlResult.IsValid)
{
    Console.WriteLine($"Invalid URL: {urlResult.ErrorMessage}");
}

// Validate a time range
var timeRangeResult = ValidationHelper.ValidateTimeRange(
    DateTime.UtcNow.AddDays(-30),
    DateTime.UtcNow.AddDays(1)
);
if (!timeRangeResult.IsValid)
{
    Console.WriteLine($"Invalid time range: {timeRangeResult.ErrorMessage}");
}

// Sanitize user input
var sanitizedInput = ValidationHelper.SanitizeInput(
    new string('x', 2000),
    maxLength: 100
);
Console.WriteLine($"Sanitized input length: {sanitizedInput.Length}");
```

## ServiceMonitorServiceTests
The `ServiceMonitorServiceTests` class provides a set of tests for the service monitoring functionality. It includes tests for retrieving all services, getting a service by name, and getting active services. These tests help ensure that the service monitoring functionality works as expected.

### Usage Example
```csharp
var serviceMonitorServiceTests = new ServiceMonitorServiceTests();
await serviceMonitorServiceTests.GetAllServicesAsync_ReturnsAllServices();
await serviceMonitorServiceTests.GetServiceByNameAsync_WithValidName_ReturnsService();
await serviceMonitorServiceTests.GetActiveServicesAsync_ReturnsOnlyActiveServices();
```

// ... existing content ...
