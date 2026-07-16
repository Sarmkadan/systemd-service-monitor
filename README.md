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

## IServiceMonitorService

The `IServiceMonitorService` interface defines a contract for monitoring systemd services. It provides methods for retrieving service information, monitoring service health, and tracking resource usage. Implementations of this interface can be used to create custom service monitors that integrate with the systemd D-Bus interface.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;

// Create a service monitor instance
var serviceMonitor = new ServiceMonitorService(
    new SystemdConnectionService(),
    new ServiceRepository()
);

// Get monitoring statistics
var stats = await serviceMonitor.GetStatisticsAsync();
Console.WriteLine($"Active: {stats.ActiveServices}, Failed: {stats.FailedServices}, Avg CPU: {stats.AverageCpuUsage}%");

// Get service statistics
var serviceStats = await serviceMonitor.GetServiceStatisticsAsync("nginx.service");
Console.WriteLine($"Service: {serviceStats.UnitName}, CPU: {serviceStats.CpuUsagePercent}%, Memory: {serviceStats.MemoryUsageMb}MB");

// Get service status
var serviceStatus = await serviceMonitor.GetServiceStatusAsync("nginx.service");
Console.WriteLine($"Status: {serviceStatus.State}");
```

The `IServiceMonitorService` interface provides a standardized way to interact with systemd services and retrieve monitoring statistics. Implementations of this interface can be used to create custom service monitors that integrate with the systemd D-Bus interface.


## AlertRulesEngine

The `AlertRulesEngine` class is a thread-safe, in-memory implementation of the `IAlertRulesEngine` interface that provides comprehensive alert management for systemd services. It evaluates alert rules against service status snapshots, manages the complete lifecycle of alert incidents, and drives multi-level escalation policies with on-call rotation support. Rules and incidents are stored in memory, making it ideal for development and testing environments, while the architecture supports easy extension to persistent storage for production deployments.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<AlertRulesEngine>();

// Create dependencies
var onCallService = new InMemoryOnCallScheduleService(logger);
var httpClientFactory = new TestHttpClientFactory(); // or real HttpClientFactory
var alertOptions = Options.Create(new AlertOptions { Enabled = true });

// Create alert rules engine instance
var alertEngine = new AlertRulesEngine(
    logger,
    onCallService,
    alertOptions,
    httpClientFactory
);

// Create and add an alert rule for high CPU usage
var cpuRule = new AlertRule
{
    Name = "High CPU Usage",
    Description = "Alert when CPU usage exceeds 90% for 3 consecutive evaluations",
    ServicePattern = "nginx.service",
    Condition = AlertCondition.CpuThresholdExceeded,
    Threshold = 90,
    Severity = AlertSeverity.Warning,
    IsEnabled = true,
    ConsecutiveEvaluationsRequired = 3,
    CooldownMinutes = 60
};

await alertEngine.AddRuleAsync(cpuRule);

// Evaluate a service status (would typically be called periodically)
var serviceStatus = new ServiceStatus
{
    UnitName = "nginx.service",
    State = ServiceState.Running,
    CpuUsagePercent = 95.5m,
    MemoryUsageMb = 512,
    IsRunning = true
};

await alertEngine.EvaluateServiceAsync(serviceStatus);

// Get active incidents
var activeIncidents = await alertEngine.GetActiveIncidentsAsync();
Console.WriteLine($"Active incidents: {activeIncidents.Count()}");

// Get summary statistics
var summary = await alertEngine.GetSummaryAsync();
Console.WriteLine($"Total rules: {summary.TotalRules}, Open incidents: {summary.OpenIncidents}");

// Manage incidents
var incidents = await alertEngine.GetActiveIncidentsAsync();
foreach (var incident in incidents)
{
    // Acknowledge the incident
    await alertEngine.AcknowledgeIncidentAsync(incident.Id, "admin@example.com");
    
    // Or resolve it
    await alertEngine.ResolveIncidentAsync(incident.Id, "admin@example.com", "Issue resolved");
    
    // Or escalate it
    await alertEngine.EscalateIncidentAsync(incident.Id);
}

// Manage on-call schedules
var schedules = await onCallService.GetSchedulesAsync();
if (!schedules.Any())
{
    var schedule = new OnCallSchedule
    {
        Name = "Production Team",
        Entries = new List<OnCallEntry>
        {
            new OnCallEntry
            {
                ResponderName = "Alice",
                ResponderContact = "alice@example.com",
                ShiftStart = DateTime.UtcNow,
                ShiftEnd = DateTime.UtcNow.AddHours(8)
            }
        }
    };
    await onCallService.CreateScheduleAsync(schedule);
}
```

The `AlertRulesEngine` provides real-time alert evaluation, incident lifecycle management, and escalation policy support for systemd service monitoring.