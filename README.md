## ServiceExtensions

The `ServiceExtensions` class provides a set of static extension methods for configuring services in the application. It allows you to add application services, application middleware, response caching, JSON options, background services, event bus, and API documentation.

### Usage Example

```csharp
using SystemdServiceMonitor.Extensions;

// Add application services
var services = new ServiceCollection();
services.AddApplicationServices();

// Add response caching
services.AddResponseCaching();

// Add JSON options
var builder = new WebApplicationBuilder();
builder.AddJsonOptions();

// Add background services
services.AddBackgroundServices();

// Add event bus
services.AddEventBus();

// Add API documentation
var apiBuilder = new WebApplicationBuilder();
apiBuilder.AddApiDocumentation();
```

## ServiceInfoExtensions

The `ServiceInfoExtensions` class provides utility methods for inspecting and formatting the status of a `ServiceInfo` object. These methods simplify common operations such as checking service state, uptime formatting, and generating status summaries.

### Usage Example

```csharp
using SystemdServiceMonitor.Models;

ServiceInfo service = GetServiceInfo(); // Assume this method retrieves a ServiceInfo instance

bool isActive = ServiceInfoExtensions.IsActive(service);
bool isFailed = ServiceInfoExtensions.IsFailed(service);
bool isEnabled = ServiceInfoExtensions.IsEnabled(service);
string uptime = ServiceInfoExtensions.GetFormattedUptime(service);
bool canRestart = ServiceInfoExtensions.CanRestart(service);
string summary = ServiceInfoExtensions.GetStatusSummary(service);

Console.WriteLine($"Service '{service.Name}' is {(isActive ? "active" : "inactive")}, {(isFailed ? "failed" : "healthy")}, and {(isEnabled ? "enabled" : "disabled")}. Uptime: {uptime}. Status: {summary}. Can restart: {canRestart}.");
```

This example demonstrates how to use the static methods of `ServiceInfoExtensions` to extract and display key status information from a `ServiceInfo` object in a clear and concise way.
