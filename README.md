// entire file content ...
// ... goes in between
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
// ... rest of the file content ...
