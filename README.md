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


## ResourceMonitorService

The `ResourceMonitorService` class provides comprehensive monitoring of system and service resource usage. It collects metrics such as CPU, memory, disk usage, and process information from system files like `/proc/stat`, `/proc/meminfo`, and cgroup directories. The service supports both one-time measurements and continuous monitoring with alerting capabilities for resource thresholds.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ResourceMonitorService>();

// Create service instance (dependencies would typically be injected in production)
var resourceMonitor = new ResourceMonitorService(
    logger,
    new SystemdOptions(),
    new SystemdConnectionService(),
    new ServiceMonitorService(logger, new SystemdConnectionService(), new ServiceRepository())
);

// Get system-wide resource metrics
var systemResources = await resourceMonitor.GetSystemResourcesAsync();
Console.WriteLine($"CPU Usage: {systemResources.CpuUsagePercent}%");
Console.WriteLine($"Memory Usage: {systemResources.UsedMemoryMb}MB / {systemResources.TotalMemoryMb}MB ({systemResources.MemoryUsagePercent}%)");
Console.WriteLine($"Disk Usage: {systemResources.UsedDiskGb}GB / {systemResources.TotalDiskGb}GB ({systemResources.DiskUsagePercent}%)");

// Get resource metrics for a specific service
var serviceMetrics = await resourceMonitor.GetServiceResourceMetricsAsync("nginx.service");
Console.WriteLine($"Service CPU: {serviceMetrics.CpuUsagePercent}%");
Console.WriteLine($"Service Memory: {serviceMetrics.MemoryUsageMb}MB");
Console.WriteLine($"Service Threads: {serviceMetrics.ThreadCount}");

// Get CPU usage for a service
var cpuUsage = await resourceMonitor.GetServiceCpuUsageAsync("nginx.service");
Console.WriteLine($"CPU Usage: {cpuUsage}%");

// Get memory usage for a service
var memoryUsage = await resourceMonitor.GetServiceMemoryUsageAsync("nginx.service");
Console.WriteLine($"Memory Usage: {memoryUsage}MB");

// Collect metrics for all services
var allMetrics = await resourceMonitor.CollectAllMetricsAsync();
foreach (var metric in allMetrics)
{
    Console.WriteLine($"{metric.UnitName}: CPU={metric.CpuUsagePercent}%, Memory={metric.MemoryUsageMb}MB");
}

// Start continuous monitoring with alerts (checks every 5 seconds)
await resourceMonitor.StartContinuousMonitoringAsync(intervalMs: 5000);

// Get current resource alerts
var alerts = await resourceMonitor.GetResourceAlertsAsync();
foreach (var alert in alerts)
{
    Console.WriteLine($"ALERT: {alert.UnitName} - {alert.Message} (Current: {alert.CurrentValue}, Threshold: {alert.Threshold})");
}

// Stop continuous monitoring when done
await resourceMonitor.StopContinuousMonitoringAsync();
```

The `ResourceMonitorService` provides real-time resource monitoring capabilities with alerting for systemd services and system-wide resources.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceDependencyGraphService>();

// Create service instance (dependencies would typically be injected in production)
var graphService = new ServiceDependencyGraphService(
    new ServiceRepository()
);

// Build complete dependency graph for all services
var fullGraph = await graphService.BuildGraphAsync();
Console.WriteLine($"Total services: {fullGraph.TotalNodes}");
Console.WriteLine($"Total dependencies: {fullGraph.TotalEdges}");

// Build dependency graph for a specific service with depth limit
var nginxGraph = await graphService.BuildGraphForServiceAsync("nginx.service", depth: 2);
Console.WriteLine($"Nginx service graph: {nginxGraph.TotalNodes} nodes");

// Get dependency chain between two services
var chain = await graphService.GetDependencyChainAsync("postgresql.service", "nginx.service");
if (chain.Any())
{
    Console.WriteLine("Dependency chain: " + string.Join(" -> ", chain));
}

// Get root services (services with no dependents)
var rootServices = await graphService.GetRootServicesAsync();
Console.WriteLine($"Root services: {rootServices.Count()}");

// Get leaf services (services with no dependencies)
var leafServices = await graphService.GetLeafServicesAsync();
Console.WriteLine($"Leaf services: {leafServices.Count()}");

// Access service information from a node
var node = nginxGraph.Nodes.FirstOrDefault(n => n.ServiceName == "nginx.service");
if (node != null)
{
    Console.WriteLine($"Service: {node.ServiceName}");
    Console.WriteLine($"Description: {node.Description}");
    Console.WriteLine($"State: {node.State}");
    Console.WriteLine($"Dependencies: {string.Join(", ", node.Dependencies)}");
    Console.WriteLine($"Dependents: {string.Join(", ", node.Dependents)}");
    Console.WriteLine($"Is root: {node.IsRootNode}");
    Console.WriteLine($"Is leaf: {node.IsLeafNode}");
}
```

## ServiceControlService

The `ServiceControlService` class provides comprehensive control operations for systemd services. It allows you to start, stop, restart, reload, enable, and disable services through the systemd D-Bus interface. The service also supports advanced operations like graceful shutdowns, restart strategies, and bulk operations for managing multiple services efficiently.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceControlService>();

// Create service instance (dependencies would typically be injected in production)
var serviceControl = new ServiceControlService(
    logger,
    new SystemdConnectionService(),
    new SystemdOptions()
);

// Start a service
bool startResult = await serviceControl.StartServiceAsync("nginx.service");
Console.WriteLine($"Start result: {startResult}");

// Stop a service
bool stopResult = await serviceControl.StopServiceAsync("nginx.service");
Console.WriteLine($"Stop result: {stopResult}");

// Restart a service
bool restartResult = await serviceControl.RestartServiceAsync("nginx.service");
Console.WriteLine($"Restart result: {restartResult}");

// Reload a service (useful for services that support configuration reload)
bool reloadResult = await serviceControl.ReloadServiceAsync("nginx.service");
Console.WriteLine($"Reload result: {reloadResult}");

// Enable a service to start on boot
bool enableResult = await serviceControl.EnableServiceAsync("nginx.service");
Console.WriteLine($"Enable result: {enableResult}");

// Disable a service from starting on boot
bool disableResult = await serviceControl.DisableServiceAsync("nginx.service");
Console.WriteLine($"Disable result: {disableResult}");

// Restart with different strategies
bool immediateRestart = await serviceControl.RestartWithStrategyAsync(
    "nginx.service",
    RestartStrategy.Immediate
);

bool gracefulRestart = await serviceControl.RestartWithStrategyAsync(
    "nginx.service",
    RestartStrategy.Graceful
);

// Gracefully shutdown a service with timeout
bool gracefulShutdown = await serviceControl.GracefulShutdownAsync(
    "nginx.service",
    timeoutSeconds: 30
);

// Get the status of the last operation on a service
var lastOperation = await serviceControl.GetLastOperationStatusAsync("nginx.service");
if (lastOperation != null)
{
    Console.WriteLine($"Last operation: {lastOperation.Operation} - Success: {lastOperation.Success}");
    Console.WriteLine($"Duration: {lastOperation.DurationMs}ms");
}

// Bulk restart multiple services with controlled concurrency
var bulkResult = await serviceControl.BulkRestartAsync(
    new[] { "nginx.service", "postgresql.service", "redis.service" },
    maxConcurrency: 5
);

foreach (var result in bulkResult.Results)
{
    Console.WriteLine($"{result.UnitName}: {(result.Success ? "SUCCESS" : "FAILED")} - {result.Message}");
}
```

The `ServiceControlService` provides a comprehensive API for managing systemd services with built-in error handling, logging, and operation tracking capabilities.

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