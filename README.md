## ApiResponse

The `ApiResponse` class provides a standardized way to return data and errors from API endpoints. It wraps the actual data being returned, along with additional metadata such as success status, human-readable messages, and error details.

### Usage Example

```csharp
var successResponse = new ApiResponse<string>
{
    Data = "Hello, World!",
    Success = true,
    Message = "Operation completed successfully.",
    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
    TraceId = Guid.NewGuid().ToString()
};

var errorResponse = new ApiResponse<string>
{
    Success = false,
    Message = "An error occurred.",
    ErrorDetails = "Invalid input data.",
    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
    TraceId = Guid.NewGuid().ToString()
};

Console.WriteLine($"Success: {successResponse.Success}, Data: {successResponse.Data}");
Console.WriteLine($"Success: {errorResponse.Success}, Error: {errorResponse.ErrorDetails}");
```

## RateLimitingMiddleware

The `RateLimitingMiddleware` implements a token‑bucket algorithm to limit the number of requests per IP address. It consumes a token on each request and returns a 429 status code when the bucket is empty. The middleware can be added to the ASP.NET Core pipeline using the provided extension methods.

### Usage Example

```csharp
using SystemdServiceMonitor.Middleware;
using Microsoft.AspNetCore.Builder;

// In your ASP.NET Core application startup:
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure rate limiting: 200 requests per minute, refill every 60 seconds
app.UseRateLimiting(new RateLimitOptions
{
    RequestsPerMinute = 200,
    RefillIntervalSeconds = 60
});

// Alternatively, use the default configuration (300 requests/minute)
// app.UseRateLimiting();

// Example of creating and inspecting a TokenBucket manually
var bucket = new TokenBucket(100, 60); // 100 tokens, refill every 60 seconds
bool consumed = bucket.TryConsumeToken(); // true if a token was available
int remaining = bucket.RemainingTokens;   // current token count
int capacity = bucket.RequestsPerMinute;  // bucket capacity
int refill = bucket.RefillIntervalSeconds; // refill interval

app.MapGet("/", () => "Hello, world!");
app.Run();
```

The middleware automatically tracks requests per IP and enforces the configured limits, ensuring that clients cannot exceed the specified request rate.

## ServiceMonitorService

The `ServiceMonitorService` class provides comprehensive monitoring of systemd services. It allows you to retrieve service information, monitor service health, track resource usage, and perform real-time monitoring of service states. The service integrates with the systemd D-Bus interface to provide up-to-date information about service status, resource consumption, and failure states.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceMonitorService>();

// Create service instance (dependencies would typically be injected in production)
var serviceMonitor = new ServiceMonitorService(
    logger,
    new SystemdConnectionService(),
    new ServiceRepository()
);

// Get all services
var allServices = await serviceMonitor.GetAllServicesAsync();
Console.WriteLine($"Total services: {allServices.Count()}");

// Get a specific service by name
var specificService = await serviceMonitor.GetServiceByNameAsync("nginx.service");
if (specificService != null)
{
    Console.WriteLine($"Service: {specificService.UnitName}, State: {specificService.State}");
}

// Get active and failed services
var activeServices = await serviceMonitor.GetActiveServicesAsync();
var failedServices = await serviceMonitor.GetFailedServicesAsync();

// Get detailed status for a service
var status = await serviceMonitor.GetServiceStatusAsync("nginx.service");
if (status != null)
{
    Console.WriteLine($"Status: {status.State}, CPU: {status.CpuUsagePercent}%, Memory: {status.MemoryUsageMb}MB");
}

// Start monitoring a service (checks status every 5 seconds)
await serviceMonitor.StartMonitoringAsync("nginx.service", intervalMs: 5000);

// Get monitoring statistics
var stats = await serviceMonitor.GetStatisticsAsync();
Console.WriteLine($"Active: {stats.ActiveServices}, Failed: {stats.FailedServices}, Avg CPU: {stats.AverageCpuUsage}%");

// Stop monitoring
await serviceMonitor.StopMonitoringAsync("nginx.service");
```

The `ServiceMonitorService` provides real-time monitoring capabilities and comprehensive service information retrieval for systemd services.