## PathResolver

The `PathResolver` class provides utility methods for resolving and normalizing systemd service unit file paths. It handles both system-wide and user-specific unit directories, validates service paths, and provides utilities for working with service names and their corresponding unit files. This class is essential for locating service unit files and determining service scope.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using SystemdServiceMonitor.Enums;

// Get all system unit paths where service files are typically located
var systemUnitPaths = PathResolver.GetSystemUnitPaths();
Console.WriteLine("System unit paths:");
foreach (var path in systemUnitPaths)
{
    Console.WriteLine($"  {path}");
}

// Get the default system unit directory
string defaultSystemUnitDir = PathResolver.GetDefaultSystemUnitDirectory();
Console.WriteLine($"Default system unit directory: {defaultSystemUnitDir}");

// Get the default user unit directory
string defaultUserUnitDir = PathResolver.GetDefaultUserUnitDirectory();
Console.WriteLine($"Default user unit directory: {defaultUserUnitDir}");

// Normalize a service name (adds .service extension if missing)
string normalizedName = PathResolver.NormalizeServiceName("nginx");
Console.WriteLine($"Normalized service name: {normalizedName}");

// Remove service extension from a service name
string withoutExtension = PathResolver.RemoveServiceExtension("nginx.service");
Console.WriteLine($"Service name without extension: {withoutExtension}");

// Find a service unit file by name
string? serviceFile = PathResolver.FindServiceUnitFile("nginx.service");
if (serviceFile != null)
{
    Console.WriteLine($"Found service file: {serviceFile}");
}

// Check if a path is a valid systemd service path
bool isValid = PathResolver.IsValidServicePath("/etc/systemd/system/nginx.service");
Console.WriteLine($"Is valid service path: {isValid}");

// Get the directory containing a service unit file
string? serviceDirectory = PathResolver.GetServiceDirectory("nginx.service");
if (serviceDirectory != null)
{
    Console.WriteLine($"Service directory: {serviceDirectory}");
}

// Determine the scope of a service (system or user)
ServiceScope scope = PathResolver.GetServiceScope("nginx.service");
Console.WriteLine($"Service scope: {scope}");

// Get all services related to a specific service (dependencies and dependents)
var relatedServices = PathResolver.GetRelatedServices("postgresql.service");
Console.WriteLine($"Related services count: {relatedServices.Count}");
foreach (var service in relatedServices)
{
    Console.WriteLine($"  {service}");
}
```

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

## IServiceLogService

The `IServiceLogService` interface provides methods for retrieving, storing, and analyzing service logs from systemd journald. It offers comprehensive log querying capabilities including filtering by service, time range, severity level, and search terms. The interface also includes statistics tracking and log management features such as clearing old logs and batch storage operations.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceLogService>();

// Create service instance (dependencies would typically be injected in production)
var logService = new ServiceLogService(
    logger,
    new ServiceRepository()
);

// Get log statistics for a service
var stats = await logService.GetLogStatisticsAsync("nginx.service");
Console.WriteLine($"Service: {stats.UnitName}");
Console.WriteLine($"Total logs: {stats.TotalLogEntries}");
Console.WriteLine($"Errors: {stats.ErrorCount}, Warnings: {stats.WarningCount}, Info: {stats.InfoCount}");
Console.WriteLine($"Time range: {stats.OldestLogTime} to {stats.LatestLogTime}");

// Get recent logs for a service
var recentLogs = await logService.GetServiceLogsAsync("nginx.service", limit: 50);
foreach (var log in recentLogs.Take(5))
{
    Console.WriteLine($"[{log.Timestamp}] [{log.Level}] {log.Message}");
}

// Get logs by severity level
var errorLogs = await logService.GetLogsByLevelAsync("nginx.service", SyslogLevel.Err);
Console.WriteLine($"Found {errorLogs.Count()} error logs");

// Get logs within a specific time range
var from = DateTime.UtcNow.AddHours(-1);
var to = DateTime.UtcNow;
var timeRangeLogs = await logService.GetLogsInTimeRangeAsync("nginx.service", from, to);
Console.WriteLine($"Found {timeRangeLogs.Count()} logs in the last hour");

// Search logs for specific terms
var searchResults = await logService.SearchLogsAsync("nginx.service", "connection failed");
Console.WriteLine($"Found {searchResults.Count()} logs containing 'connection failed'");

// Store a log entry
var newLog = new ServiceLog
{
    UnitName = "nginx.service",
    Timestamp = DateTime.UtcNow,
    Level = SyslogLevel.Info,
    Message = "Service started successfully",
    Priority = 6
};
var storedLog = await logService.StoreLogAsync(newLog);
Console.WriteLine($"Stored log with ID: {storedLog.Id}");

// Clear old logs (older than 30 days)
var logsCleared = await logService.ClearOldLogsAsync(30);
Console.WriteLine($"Cleared {logsCleared} old log entries");
```

The `IServiceLogService` interface provides comprehensive logging capabilities for systemd services, enabling efficient log retrieval, analysis, and management.

## ServiceLogService

The `ServiceLogService` class provides concrete implementation for service log management from systemd journald. It implements `IServiceLogService` and provides methods for retrieving logs from the systemd journal, storing logs in the database, and performing log analysis and management operations.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Configuration;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceLogService>();

// Create service instance with all required dependencies
var logService = new ServiceLogService(
    logger,
    new ServiceRepository(),
    new SystemdOptions(),
    new SystemdConnectionService()
);

// Get logs directly from journald for a service
var journaldLogs = await logService.FetchLatestFromJournalAsync("nginx.service", count: 100);
foreach (var log in journaldLogs.Take(5))
{
    Console.WriteLine($"[{log.Timestamp}] [{log.Level}] {log.Message}");
}

// Get logs filtered by minimum priority level
var warningAndAbove = await logService.FetchFromJournalByPriorityAsync(
    "nginx.service", 
    SyslogLevel.Warning, 
    count: 50
);
Console.WriteLine($"Found {warningAndAbove.Count()} warning and higher priority logs");

// Get recent logs from database
var recentDatabaseLogs = await logService.GetRecentLogsAsync(limit: 25);
Console.WriteLine($"Found {recentDatabaseLogs.Count()} recent logs in database");

// Store a batch of logs
var batchLogs = new List<ServiceLog>();
for (int i = 0; i < 10; i++)
{
    batchLogs.Add(new ServiceLog
    {
        UnitName = "nginx.service",
        Timestamp = DateTime.UtcNow.AddMinutes(-i),
        Level = i % 3 == 0 ? SyslogLevel.Error : (i % 3 == 1 ? SyslogLevel.Warning : SyslogLevel.Info),
        Message = $"Sample log message {i}",
        Priority = i % 3 == 0 ? 3 : (i % 3 == 1 ? 4 : 6)
    });
}
var storedCount = await logService.StoreLogsAsync(batchLogs);
Console.WriteLine($"Stored {storedCount} logs in batch");

// Get statistics for a specific service
var serviceStats = await logService.GetLogStatisticsAsync("nginx.service");
Console.WriteLine($"Service: {serviceStats.UnitName}");
Console.WriteLine($"Total logs: {serviceStats.TotalLogEntries}");
Console.WriteLine($"Errors: {serviceStats.ErrorCount}, Warnings: {serviceStats.WarningCount}");

// Clear old logs from database
var clearedCount = await logService.ClearOldLogsAsync(retentionDays: 30);
Console.WriteLine($"Cleared {clearedCount} old log entries from database");
```

The `ServiceLogService` provides comprehensive logging capabilities by combining direct journald access with database storage and retrieval, enabling both real-time log analysis and historical log management.


## MemoryCacheProvider

The `MemoryCacheProvider` class provides an in-memory caching implementation with support for asynchronous operations. It supports typed values, automatic expiration, and pattern-based cache removal. The provider tracks access patterns and provides detailed cache statistics including creation time, last access time, expiration time, and access counts.

### Usage Example

```csharp
using SystemdServiceMonitor.Caching;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<MemoryCacheProvider>();

// Create cache provider instance (typically injected in production)
var cacheProvider = new MemoryCacheProvider(logger);

// Set a value in cache
await cacheProvider.SetAsync("user:123:profile", new UserProfile
{
    UserId = 123,
    Username = "john_doe",
    Email = "john@example.com",
    LastLogin = DateTime.UtcNow
});

// Get a value from cache
var cachedProfile = await cacheProvider.GetAsync<UserProfile>("user:123:profile");
if (cachedProfile != null)
{
    Console.WriteLine($"Retrieved user: {cachedProfile.Username}");
}

// Check if a key exists
bool exists = await cacheProvider.ExistsAsync("user:123:profile");
Console.WriteLine($"Cache key exists: {exists}");

// Get TTL (time to live) for a key
var ttl = await cacheProvider.GetTtlAsync("user:123:profile");
Console.WriteLine($"TTL: {ttl} seconds");

// Remove a specific key
await cacheProvider.RemoveAsync("user:123:profile");

// Remove keys by pattern (e.g., all user cache entries)
await cacheProvider.RemoveByPatternAsync("user:*");

// Clear the entire cache
await cacheProvider.ClearAsync();

// Get cache statistics
var stats = new MemoryCacheProvider(logger);
var value = new UserProfile { UserId = 1, Username = "test" };
await stats.SetAsync("test:key", value);

Console.WriteLine($"Value: {stats.Value}");
Console.WriteLine($"ExpirationTime: {stats.ExpirationTime}");
Console.WriteLine($"CreatedAt: {stats.CreatedAt}");
Console.WriteLine($"LastAccessTime: {stats.LastAccessTime}");
Console.WriteLine($"AccessCount: {stats.AccessCount}");
Console.WriteLine($"IsExpired: {stats.IsExpired}");
```

The `MemoryCacheProvider` offers comprehensive caching capabilities with automatic expiration, pattern-based cache management, and detailed statistics tracking for monitoring and debugging purposes.

## ServiceRepository

The `ServiceRepository` class provides an in-memory data access layer for managing service unit information. It implements the `IServiceRepository` interface and provides CRUD operations for service data, including filtering capabilities for active, failed, and user-specific services. The repository uses thread-safe operations with a semaphore lock to ensure data consistency in concurrent scenarios.

### Usage Example

```csharp
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceRepository>();

// Create repository instance (typically injected in production)
var serviceRepository = new ServiceRepository();

// Create a new service
var newService = new ServiceInfo
{
    Id = Guid.NewGuid(),
    UnitName = "nginx.service",
    Description = "Nginx web server",
    State = ServiceState.Active,
    RunAsUser = "www-data",
    ExecStart = "/usr/sbin/nginx -g 'daemon on;'",
    ExecReload = "/usr/sbin/nginx -s reload",
    ExecStop = "/usr/sbin/nginx -s stop",
    WorkingDirectory = "/var/www/html",
    Environment = new Dictionary<string, string>
    {
        ["NGINX_ENV"] = "production",
        ["PATH"] = "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin"
    }
};

var createdService = await serviceRepository.CreateAsync(newService);
Console.WriteLine($"Created service: {createdService.UnitName} with ID: {createdService.Id}");

// Get a service by ID
var retrievedService = await serviceRepository.GetByIdAsync(createdService.Id);
if (retrievedService != null)
{
    Console.WriteLine($"Retrieved: {retrievedService.UnitName} - {retrievedService.Description}");
}

// Get a service by unit name
var nginxService = await serviceRepository.GetByUnitNameAsync("nginx.service");
if (nginxService != null)
{
    Console.WriteLine($"Found service by name: {nginxService.State}");
}

// Get all services
var allServices = await serviceRepository.GetAllAsync();
Console.WriteLine($"Total services: {allServices.Count()}");

// Get active services
var activeServices = await serviceRepository.GetActiveServicesAsync();
Console.WriteLine($"Active services: {activeServices.Count()}");

// Get failed services
var failedServices = await serviceRepository.GetFailedServicesAsync();
Console.WriteLine($"Failed services: {failedServices.Count()}");

// Get services by user
var wwwServices = await serviceRepository.GetByUserAsync("www-data");
Console.WriteLine($"Services for www-data: {wwwServices.Count()}");

// Update a service
retrievedService!.State = ServiceState.Inactive;
var updatedService = await serviceRepository.UpdateAsync(retrievedService);
Console.WriteLine($"Updated service state to: {updatedService.State}");

// Search for services
var searchResults = await serviceRepository.SearchAsync("nginx");
Console.WriteLine($"Search results for 'nginx': {searchResults.Count()} services found");

// Get total count
var totalCount = await serviceRepository.GetTotalCountAsync();
Console.WriteLine($"Total service count: {totalCount}");

// Get paged results (page 1, 10 items per page)
var pagedServices = await serviceRepository.GetPagedAsync(1, 10);
Console.WriteLine($"Page 1 results: {pagedServices.Count()} services");

// Delete a service
bool deleted = await serviceRepository.DeleteAsync(createdService.Id);
Console.WriteLine($"Service deleted: {deleted}");
```

The `ServiceRepository` provides thread-safe data access for service unit information with comprehensive CRUD operations and filtering capabilities for monitoring scenarios.


## MetricRepository

The `MetricRepository` class provides an in-memory data access layer for managing time-series metric data collected from systemd services. It implements the `IMetricRepository` interface and provides CRUD operations for service metrics, including time-series queries, filtering by service ID, metric type, and time ranges. The repository uses thread-safe operations with a semaphore lock to ensure data consistency in concurrent scenarios.

### Usage Example

```csharp
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<MetricRepository>();

// Create repository instance (typically injected in production)
var metricRepository = new MetricRepository();

// Create a CPU usage metric for nginx service
var cpuMetric = new ServiceMetric
{
  Id = Guid.NewGuid(),
  ServiceInfoId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Replace with actual service ID
  MetricType = MetricType.CpuUsage,
  Value = 45.5m, // 45.5% CPU usage
  Timestamp = DateTime.UtcNow,
  UnitName = "nginx.service",
  Description = "CPU usage percentage"
};

var createdMetric = await metricRepository.CreateAsync(cpuMetric);
Console.WriteLine($"Created metric: {createdMetric.MetricType} = {createdMetric.Value}% at {createdMetric.Timestamp}");

// Get a metric by ID
var retrievedMetric = await metricRepository.GetByIdAsync(createdMetric.Id);
if (retrievedMetric != null)
{
  Console.WriteLine($"Retrieved metric: {retrievedMetric.MetricType} = {retrievedMetric.Value}");
}

// Get all metrics for a specific service
var serviceMetrics = await metricRepository.GetByServiceIdAsync(cpuMetric.ServiceInfoId);
Console.WriteLine($"Found {serviceMetrics.Count()} metrics for service");

// Get all CPU usage metrics
var cpuMetrics = await metricRepository.GetByMetricTypeAsync(MetricType.CpuUsage);
Console.WriteLine($"Found {cpuMetrics.Count()} CPU metrics");

// Get time-series data for a service (last 24 hours)
var timeSeries = await metricRepository.GetTimeSeriesAsync(
  cpuMetric.ServiceInfoId,
  MetricType.CpuUsage,
  TimeSpan.FromHours(24)
);
Console.WriteLine($"Time-series data points: {timeSeries.Count()}");

// Get recent metrics (last 6 hours, max 500)
var recentMetrics = await metricRepository.GetRecentAsync(6, 500);
Console.WriteLine($"Found {recentMetrics.Count()} recent metrics");

// Get the latest metric for a service and type
var latestMetric = await metricRepository.GetLatestAsync(
  cpuMetric.ServiceInfoId,
  MetricType.CpuUsage
);
if (latestMetric != null)
{
  Console.WriteLine($"Latest CPU metric: {latestMetric.Value}% at {latestMetric.Timestamp}");
}

// Calculate average CPU usage over last hour
var averageCpu = await metricRepository.GetAverageAsync(
  cpuMetric.ServiceInfoId,
  MetricType.CpuUsage,
  TimeSpan.FromHours(1)
);
Console.WriteLine($"Average CPU usage (last hour): {averageCpu}%");

// Create multiple metrics in batch
var batchMetrics = new List<ServiceMetric>();
for (int i = 0; i < 10; i++)
{
  batchMetrics.Add(new ServiceMetric
  {
    Id = Guid.NewGuid(),
    ServiceInfoId = cpuMetric.ServiceInfoId,
    MetricType = MetricType.CpuUsage,
    Value = 30m + i,
    Timestamp = DateTime.UtcNow.AddMinutes(-i),
    UnitName = "nginx.service",
    Description = $"Sample metric {i}"
  });
}

var batchCount = await metricRepository.CreateBatchAsync(batchMetrics);
Console.WriteLine($"Created {batchCount} metrics in batch");

// Delete a metric
bool deleted = await metricRepository.DeleteAsync(createdMetric.Id);
Console.WriteLine($"Metric deleted: {deleted}");

// Delete metrics older than 30 days
var deletedCount = await metricRepository.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Deleted {deletedCount} old metrics");
```

The `MetricRepository` provides thread-safe data access for time-series metric data with comprehensive CRUD operations, time-series queries, and statistical calculations for monitoring scenarios.



## LogRepository

The `LogRepository` class provides an in-memory data access layer for managing service log entries. It implements the `ILogRepository` interface and provides CRUD operations for service logs, including filtering capabilities by service unit name, service ID, log level, time range, and process ID. The repository uses thread-safe operations with a semaphore lock to ensure data consistency in concurrent scenarios.

### Usage Example

```csharp
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<LogRepository>();

// Create repository instance (typically injected in production)
var logRepository = new LogRepository();

// Create a new log entry
var newLog = new ServiceLog
{
    Id = Guid.NewGuid(),
    UnitName = "nginx.service",
    ServiceInfoId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Replace with actual service ID
    Timestamp = DateTime.UtcNow,
    Level = SyslogLevel.Info,
    Message = "Service started successfully",
    Priority = 6,
    ProcessId = 1234,
    Hostname = "web-server-01"
};

var createdLog = await logRepository.CreateAsync(newLog);
Console.WriteLine($"Created log entry with ID: {createdLog.Id}");

// Get a log by ID
var retrievedLog = await logRepository.GetByIdAsync(createdLog.Id);
if (retrievedLog != null)
{
    Console.WriteLine($"Retrieved log: [{retrievedLog.Timestamp}] [{retrievedLog.Level}] {retrievedLog.Message}");
}

// Get logs by unit name (service)
var nginxLogs = await logRepository.GetByUnitNameAsync("nginx.service", limit: 50);
Console.WriteLine($"Found {nginxLogs.Count()} logs for nginx.service");

// Get logs by service ID
var serviceLogs = await logRepository.GetByServiceIdAsync(newLog.ServiceInfoId, limit: 25);
Console.WriteLine($"Found {serviceLogs.Count()} logs for service ID: {newLog.ServiceInfoId}");

// Get logs by log level (severity)
var errorLogs = await logRepository.GetByLevelAsync(SyslogLevel.Err);
Console.WriteLine($"Found {errorLogs.Count()} error logs");

// Get recent logs (last 24 hours)
var recentLogs = await logRepository.GetRecentAsync(TimeSpan.FromHours(24));
Console.WriteLine($"Found {recentLogs.Count()} logs in the last 24 hours");

// Get logs by process ID
var processLogs = await logRepository.GetByProcessIdAsync(1234);
Console.WriteLine($"Found {processLogs.Count()} logs from process ID 1234");

// Search logs for specific terms
var searchResults = await logRepository.SearchAsync("connection failed");
Console.WriteLine($"Found {searchResults.Count()} logs containing 'connection failed'");

// Get total log count
var totalCount = await logRepository.GetCountAsync();
Console.WriteLine($"Total log entries: {totalCount}");

// Create multiple logs in batch
var batchLogs = new List<ServiceLog>();
for (int i = 0; i < 10; i++)
{
    batchLogs.Add(new ServiceLog
    {
        Id = Guid.NewGuid(),
        UnitName = "nginx.service",
        ServiceInfoId = newLog.ServiceInfoId,
        Timestamp = DateTime.UtcNow.AddMinutes(-i),
        Level = i % 3 == 0 ? SyslogLevel.Error : (i % 3 == 1 ? SyslogLevel.Warning : SyslogLevel.Info),
        Message = $"Sample log message {i}",
        Priority = i % 3 == 0 ? 3 : (i % 3 == 1 ? 4 : 6),
        ProcessId = 1234 + i,
        Hostname = "web-server-01"
    });
}

var batchCount = await logRepository.CreateBatchAsync(batchLogs);
Console.WriteLine($"Created {batchCount} logs in batch");

// Delete a log entry
bool deleted = await logRepository.DeleteAsync(createdLog.Id);
Console.WriteLine($"Log entry deleted: {deleted}");

// Delete logs older than 30 days
var deletedCount = await logRepository.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Deleted {deletedCount} old log entries");
```

The `LogRepository` provides thread-safe data access for service log entries with comprehensive CRUD operations and filtering capabilities for monitoring scenarios.




## PerformanceMonitor

The `PerformanceMonitor` class provides a lightweight utility for measuring and analyzing the performance of operations. It tracks elapsed time, records checkpoints, and generates detailed performance summaries. This is useful for performance profiling, identifying bottlenecks, and monitoring operation durations in systemd service monitoring scenarios.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<PerformanceMonitor>();

// Create a performance monitor for an operation
using (var monitor = new PerformanceMonitor("Database Query", logger, warningThresholdMs: 500))
{
    // Perform some work
    await Task.Delay(250);
    
    // Record checkpoints
    monitor.RecordCheckpoint("Query Started");
    await Task.Delay(100);
    monitor.RecordCheckpoint("Query Executed");
    await Task.Delay(150);
    monitor.RecordCheckpoint("Results Processed");
    
    // Get checkpoint timings
    var checkpoints = monitor.GetCheckpoints();
    Console.WriteLine($"Checkpoint timings: {string.Join(", ", checkpoints.Select(kvp => $"{kvp.Key}={kvp.Value}ms"))}");
    
    // Calculate time between checkpoints
    var queryDuration = monitor.GetElapsedBetween("Query Started", "Query Executed");
    Console.WriteLine($"Query execution time: {queryDuration}ms");
    
    // Get formatted summary
    var summary = monitor.GetSummary();
    Console.WriteLine(summary);
}

// Alternative: Quick measurement without creating a monitor instance
var operationName = "File Processing";
var totalTime = PerformanceMonitor.MeasureAction(operationName, () =>
{
    Thread.Sleep(100);
    return 42;
});
Console.WriteLine($"Operation completed in {totalTime}ms");

// Async version
var asyncTime = await PerformanceMonitor.MeasureActionAsync(operationName, async () =>
{
    await Task.Delay(150);
    return "result";
});
Console.WriteLine($"Async operation completed in {asyncTime}ms");
```

The `PerformanceMonitor` provides detailed performance tracking capabilities with checkpoint recording, time measurements between operations, and automatic logging of performance warnings when thresholds are exceeded.


## OutputFormatter

The `OutputFormatter` class provides utility methods for formatting data in various formats including JSON, CSV, and console-friendly tables. It's designed for CLI tools and export functionality, offering consistent formatting across different output types.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;

// Format services as JSON
var services = new List<ServiceInfo> { /* your services */ };
string jsonOutput = OutputFormatter.FormatAsJson(services);
Console.WriteLine(jsonOutput);

// Format services as CSV
string csvOutput = OutputFormatter.FormatAsCsv(services);
Console.WriteLine(csvOutput);

// Format services as a formatted table
string tableOutput = OutputFormatter.FormatAsTable(services);
Console.WriteLine(tableOutput);

// Format system metrics as a table
var metrics = new SystemResource { /* your metrics */ };
string metricsTable = OutputFormatter.FormatMetricsAsTable(metrics);
Console.WriteLine(metricsTable);

// Format detailed service information
var service = new ServiceInfo { UnitName = "nginx.service", /* other properties */ };
string serviceDetails = OutputFormatter.FormatServiceDetails(service);
Console.WriteLine(serviceDetails);

// Create a progress bar
string progressBar = OutputFormatter.CreateProgressBar(75.5); // 75.5%
Console.WriteLine(progressBar);
```

## ServiceFactory

The `ServiceFactory` utility class provides convenient factory methods for creating and initializing service-related objects with sensible defaults. It simplifies the construction of domain objects like `ServiceInfo`, `ServiceMetric`, `ServiceLog`, `ServiceStatus`, and `RestartPolicyConfig`, reducing boilerplate code and ensuring consistent initialization patterns throughout the application.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;

// Create a basic service info
var nginxService = ServiceFactory.CreateServiceInfo(
    unitName: "nginx.service",
    description: "Nginx web server and reverse proxy",
    state: "Active"
);
Console.WriteLine($"Created service: {nginxService.UnitName} ({nginxService.Description})");

// Create a service metric for monitoring
var cpuMetric = ServiceFactory.CreateServiceMetric("nginx.service");
cpuMetric.MetricType = MetricType.CpuUsage;
cpuMetric.Value = 45.5m;
Console.WriteLine($"Created metric: {cpuMetric.MetricType} = {cpuMetric.Value}%");

// Create a service log entry
var logEntry = ServiceFactory.CreateServiceLog(
    unitName: "nginx.service",
    message: "Service started successfully",
    severity: "INFO"
);
Console.WriteLine($"Created log: [{logEntry.Level}] {logEntry.Message}");

// Create a service status snapshot
var serviceStatus = ServiceFactory.CreateServiceStatus(nginxService);
Console.WriteLine($"Service status: {serviceStatus.State} (Running: {serviceStatus.IsRunning})");

// Create a restart policy configuration
var restartPolicy = ServiceFactory.CreateRestartPolicy(
    policyName: "Always",
    delaySec: 10,
    maxAttempts: 5
);
Console.WriteLine($"Restart policy: {restartPolicy.PolicyType} (Delay: {restartPolicy.RestartDelaySec}s, Max: {restartPolicy.MaxRestarts})");

// Convert service info to dictionary for API responses
var serviceDict = ServiceFactory.ServiceInfoToDictionary(nginxService);
Console.WriteLine($"Service info has {serviceDict.Count} properties");

// Batch create service info objects from names
var services = ServiceFactory.CreateServicesFromNames(
    "nginx.service",
    "postgresql.service",
    "redis.service"
);
Console.WriteLine($"Created {services.Count} services from names");
```

The `ServiceFactory` provides a clean, consistent way to create service-related objects with proper initialization and sensible defaults, reducing repetitive code throughout the application.


## ServiceHealthChecker

The `ServiceHealthChecker` utility class provides comprehensive health assessment capabilities for systemd services. It evaluates service health status based on state, restart count, uptime, and other metrics, generating human-readable summaries and recommended actions for problematic services. This class is essential for proactive service monitoring and automated health checks.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ServiceHealthChecker>();

// Create a sample service
var nginxService = new ServiceInfo
{
    UnitName = "nginx.service",
    Description = "Nginx web server",
    State = ServiceState.Active,
    RestartCount = 2,
    UptimeSeconds = 3600, // 1 hour uptime
    AutoStart = true,
    RestartPolicy = RestartPolicy.Always
};

// Evaluate service health status
var healthStatus = ServiceHealthChecker.GetHealthStatus(nginxService);
Console.WriteLine($"Health Status: {healthStatus}"); // Output: Healthy

// Get human-readable health summary
var healthSummary = ServiceHealthChecker.GetHealthSummary(nginxService);
Console.WriteLine(healthSummary);
// Output: nginx.service: ✓ Healthy | Uptime: 1h 0m

// Check if service is problematic
bool isProblematic = ServiceHealthChecker.IsProblematic(nginxService);
Console.WriteLine($"Is problematic: {isProblematic}"); // Output: False

// Get recommended actions for problematic services
var actions = ServiceHealthChecker.GetRecommendedActions(nginxService);
if (actions.Any())
{
    Console.WriteLine("Recommended actions:");
    foreach (var action in actions)
    {
        Console.WriteLine($"- {action}");
    }
}

// Format uptime for display
var formattedUptime = ServiceHealthChecker.FormatUptime(nginxService.UptimeSeconds);
Console.WriteLine($"Uptime: {formattedUptime}"); // Output: 1h 0m

// Calculate service reliability percentage
var reliability = ServiceHealthChecker.CalculateReliability(nginxService);
Console.WriteLine($"Reliability: {reliability}%"); // Output: 90%

// Example with a problematic service
var problematicService = new ServiceInfo
{
    UnitName = "crashy-service.service",
    Description = "Service that crashes frequently",
    State = ServiceState.Active,
    RestartCount = 15, // High restart count
    UptimeSeconds = 300, // Only 5 minutes uptime
    AutoStart = true,
    RestartPolicy = RestartPolicy.Always
};

var problemStatus = ServiceHealthChecker.GetHealthStatus(problematicService);
Console.WriteLine($"Problem service status: {problemStatus}"); // Output: Critical

var problemActions = ServiceHealthChecker.GetRecommendedActions(problematicService);
Console.WriteLine("Recommended actions for problematic service:");
foreach (var action in problemActions)
{
    Console.WriteLine($"- {action}");
}
```

The `ServiceHealthChecker` provides comprehensive health evaluation capabilities with standardized status reporting, actionable recommendations, and reliability calculations for systemd service monitoring scenarios.



## LogContextEnricher

The `LogContextEnricher` enriches Serilog log events with contextual information such as correlation IDs, request IDs, user information, HTTP method/path details, client IP addresses, and response status codes. This enricher automatically adds these properties to all log events, making it easier to trace requests across service boundaries and correlate logs with specific HTTP requests.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;
using Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// Setup Serilog with the context enricher
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithContextEnricher()
    .WriteTo.Console()
    .CreateLogger();

// In your ASP.NET Core application startup:
var builder = WebApplication.CreateBuilder(args);

// Add required services
builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog(); // Use Serilog for logging

var app = builder.Build();

// Add contextual properties to the logging scope
using (LogContextEnricher.PushContext("CustomProperty", "Value"))
{
    Log.Information("This log will include the custom property");
}

// Push a correlation ID
using (LogContextEnricher.PushCorrelationId(Guid.NewGuid().ToString()))
{
    Log.Information("This log will include a correlation ID");
}

// Push a request ID
using (LogContextEnricher.PushRequestId("req-12345"))
{
    Log.Information("This log will include a request ID");
}

// Use the structured log context helper
using (var context = new StructuredLogContext())
{
    context.AddProperty("SessionId", Guid.NewGuid().ToString());
    context.AddProperty("UserId", "user123");
    
    Log.Information("User action performed");
    // Properties are automatically removed when context is disposed
}

app.MapGet("/", () => "Hello, World!");
app.Run();
```

The `LogContextEnricher` automatically enriches logs with HTTP context information when available, providing better observability and debugging capabilities for distributed systems.

## SystemdConnectionService

The `SystemdConnectionService` class provides a low-level connection to the systemd D-Bus interface. It establishes and maintains the connection to systemd, handles authentication, and provides the foundation for all systemd operations throughout the application. This service is responsible for establishing the D-Bus connection, verifying its integrity, and providing methods to interact with systemd's API.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;

// Create a connection service instance
var connectionService = new SystemdConnectionService();

// Connect to systemd D-Bus interface
bool isConnected = await connectionService.ConnectAsync();
if (isConnected)
{
    Console.WriteLine("Successfully connected to systemd D-Bus interface");
}

// Verify the connection is active
bool isVerified = await connectionService.VerifyConnectionAsync();
Console.WriteLine($"Connection verified: {isVerified}");

// Get systemd version
string version = await connectionService.GetSystemdVersionAsync();
Console.WriteLine($"systemd version: {version}");

// Call a method on systemd (generic method call)
var result = await connectionService.CallMethodAsync<bool>(
    "org.freedesktop.systemd1",
    "/org/freedesktop/systemd1",
    "org.freedesktop.DBus.Properties",
    "Get",
    "s",
    "org.freedesktop.systemd1.Manager",
    "string",
    "DefaultTimeoutStartUSec"
);

Console.WriteLine($"Method call result: {result}");

// Subscribe to systemd signals
await connectionService.SubscribeToSignalsAsync();
Console.WriteLine("Subscribed to systemd D-Bus signals");

// Disconnect when done
await connectionService.DisconnectAsync();
Console.WriteLine("Disconnected from systemd D-Bus interface");
```

The `SystemdConnectionService` provides the essential connection management capabilities required for all systemd interactions in the application.


## ServiceControlService

The `ServiceControlService` class provides comprehensive control operations for systemd services. It allows you to start, stop, restart, reload, enable, and disable services through the systemd D-Bus interface. The service also supports advanced operations like graceful shutdowns, restart strategies, and bulk operations for managing multiple services efficiently.

## IResourceMonitorService

The `IResourceMonitorService` interface defines a contract for monitoring system and service resource usage. It provides properties for tracking CPU usage, memory consumption, thread counts, file descriptors, network I/O, disk I/O, and alert information for systemd services. Implementations of this interface can be used to create custom resource monitors that collect and report detailed performance metrics.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;
using Microsoft.Extensions.Logging;

// Create a resource monitor instance (dependencies would typically be injected in production)
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ResourceMonitorService>();

var resourceMonitor = new ResourceMonitorService(
    logger,
    new SystemdOptions(),
    new SystemdConnectionService(),
    new ServiceMonitorService(logger, new SystemdConnectionService(), new ServiceRepository())
);

// Get current resource metrics for the system
var systemMetrics = await resourceMonitor.GetSystemResourcesAsync();
Console.WriteLine($"System CPU Usage: {systemMetrics.CpuUsagePercent}%");
Console.WriteLine($"System Memory: {systemMetrics.UsedMemoryMb}MB / {systemMetrics.TotalMemoryMb}MB");
Console.WriteLine($"System Threads: {systemMetrics.ThreadCount}");
Console.WriteLine($"File Descriptors: {systemMetrics.FileDescriptorCount}");
Console.WriteLine($"Network In: {systemMetrics.NetworkBytesIn} bytes");
Console.WriteLine($"Network Out: {systemMetrics.NetworkBytesOut} bytes");
Console.WriteLine($"Disk Read: {systemMetrics.DiskBytesRead} bytes");
Console.WriteLine($"Disk Write: {systemMetrics.DiskBytesWritten} bytes");

// Get resource metrics for a specific service
var serviceMetrics = await resourceMonitor.GetServiceResourceMetricsAsync("nginx.service");
Console.WriteLine($"Service: {serviceMetrics.UnitName}");
Console.WriteLine($"CPU Usage: {serviceMetrics.CpuUsagePercent}%");
Console.WriteLine($"Memory Usage: {serviceMetrics.MemoryUsageMb}MB");
Console.WriteLine($"Thread Count: {serviceMetrics.ThreadCount}");
Console.WriteLine($"File Descriptor Count: {serviceMetrics.FileDescriptorCount}");
Console.WriteLine($"Network In: {serviceMetrics.NetworkBytesIn} bytes");
Console.WriteLine($"Network Out: {serviceMetrics.NetworkBytesOut} bytes");
Console.WriteLine($"Disk Read: {serviceMetrics.DiskBytesRead} bytes");
Console.WriteLine($"Disk Write: {serviceMetrics.DiskBytesWritten} bytes");

// Check for resource alerts
var alerts = await resourceMonitor.GetResourceAlertsAsync();
foreach (var alert in alerts)
{
    Console.WriteLine($"ALERT [{alert.AlertTime}] {alert.UnitName}: {alert.Message}");
    Console.WriteLine($"  Type: {alert.AlertType}, Current: {alert.CurrentValue}, Threshold: {alert.Threshold}");
}

// Get individual metric values
var cpuUsage = await resourceMonitor.GetServiceCpuUsageAsync("nginx.service");
var memoryUsage = await resourceMonitor.GetServiceMemoryUsageAsync("nginx.service");
var threadCount = await resourceMonitor.GetServiceThreadCountAsync("nginx.service");

Console.WriteLine($"Nginx CPU: {cpuUsage}%");
Console.WriteLine($"Nginx Memory: {memoryUsage}MB");
Console.WriteLine($"Nginx Threads: {threadCount}");
```

The `IResourceMonitorService` interface provides standardized access to comprehensive resource monitoring data for both system-wide and service-specific metrics.

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

## AlertOptions

The `AlertOptions` class defines configuration settings for service monitoring alerts, including thresholds, intervals, escalation policies, and notification preferences. It controls how alerts are triggered, escalated, and resolved based on service conditions and resource usage.

### Usage Example

```csharp
using SystemdServiceMonitor.Configuration;
using Microsoft.Extensions.Options;

// Create alert options with custom configuration
var alertOptions = Options.Create(new AlertOptions
{
    Enabled = true,
    AutoResolveOnConditionCleared = true,
    MaxIncidentHistorySize = 1000,
    EscalationCheckIntervalSeconds = 300,
    ServiceEvaluationIntervalSeconds = 60,
    StartupDelaySeconds = 30,
    TimeoutSeconds = 300,
    MaxRetries = 3,
    RetryDelayMs = 1000,
    InitialEscalationDelayMinutes = 15,
    SubsequentEscalationDelayMinutes = 30,
    MaxEscalationLevels = 5,
    DefaultCooldownMinutes = 120,
    
    // Webhook notification configuration
    Webhook = new WebhookNotificationOptions
    {
        Enabled = true,
        Url = "https://hooks.example.com/alerts",
        Method = "POST",
        Headers = new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer token123",
            ["X-API-Key"] = "api-key-456"
        },
        PayloadTemplate = "{ \"service\": \"{UnitName}\", \"state\": \"{State}\", \"cpu\": {CpuUsagePercent} }"
    },
    
    // Escalation defaults
    EscalationDefaults = new EscalationDefaults
    {
        Levels = new List<EscalationLevel>
        {
            new EscalationLevel
            {
                Level = 1,
                Recipients = new List<string> { "team-lead@example.com" },
                MessageTemplate = "Level 1 alert for {UnitName}"
            },
            new EscalationLevel
            {
                Level = 2,
                Recipients = new List<string> { "oncall@example.com", "manager@example.com" },
                MessageTemplate = "Level 2 escalation for {UnitName}"
            }
        }
    },
    
    // Default headers for all HTTP requests
    DefaultHeaders = new Dictionary<string, string>
    {
        ["User-Agent"] = "SystemdServiceMonitor/1.0",
        ["Accept"] = "application/json"
    }
});

Console.WriteLine($"Alerts enabled: {alertOptions.Value.Enabled}");
Console.WriteLine($"Evaluation interval: {alertOptions.Value.ServiceEvaluationIntervalSeconds} seconds");
```

## SystemdOptions

The `SystemdOptions` class provides configuration settings for systemd D-Bus integration and service monitoring operations. It controls various aspects of systemd service monitoring including connection settings, monitoring behavior, and operational parameters.

### Usage Example

```csharp
using SystemdServiceMonitor.Configuration;
using Microsoft.Extensions.Options;

// Create systemd options with custom configuration
var systemdOptions = Options.Create(new SystemdOptions
{
    EnableMonitoring = true,
    MetricCollectionIntervalMs = 10000, // 10 seconds
    LogRetentionDays = 60, // Keep logs for 60 days
    MaxLogEntriesPerRequest = 5000, // Max 5000 logs per API request
    EnableRemoteOperations = true, // Allow remote operations
    OperationTimeoutMs = 60000, // 60 second timeout
    ConnectionRetryCount = 10, // Retry up to 10 times
    ConnectionRetryDelayMs = 1000, // 1 second between retries
    EnableHealthChecks = true // Enable health check endpoints
});

Console.WriteLine($"Monitoring enabled: {systemdOptions.Value.EnableMonitoring}");
Console.WriteLine($"Metric collection interval: {systemdOptions.Value.MetricCollectionIntervalMs}ms");
Console.WriteLine($"Log retention: {systemdOptions.Value.LogRetentionDays} days");
```

## PaginationHelper

The `PaginationHelper` class provides utility methods for implementing consistent pagination across API endpoints and data access layers. It handles validation of pagination parameters, calculation of skip/take values, and generation of pagination metadata including page numbers, total pages, and navigation indicators.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;

// Validate pagination parameters from user input
var (pageNumber, pageSize) = PaginationHelper.ValidatePaginationParams(2, 25);
Console.WriteLine($"Validated pagination: Page {pageNumber}, Size {pageSize}");

// Calculate skip value for database queries
int skip = PaginationHelper.CalculateSkip(pageNumber, pageSize);
Console.WriteLine($"Skip value for database query: {skip}");

// Calculate total pages based on total item count
totalCount = 150; // Total items from your data source
int totalPages = PaginationHelper.CalculateTotalPages(totalCount, pageSize);
Console.WriteLine($"Total pages: {totalPages}");

// Check if there are more pages
bool hasNextPage = PaginationHelper.HasNextPage(pageNumber, totalPages);
bool hasPreviousPage = PaginationHelper.HasPreviousPage(pageNumber);
Console.WriteLine($"Has next page: {hasNextPage}, Has previous page: {hasPreviousPage}");

// Get next/previous page numbers
int nextPage = PaginationHelper.GetNextPageNumber(pageNumber);
int previousPage = PaginationHelper.GetPreviousPageNumber(pageNumber);
Console.WriteLine($"Next page: {nextPage}, Previous page: {previousPage}");

// Paginate a list of items
var allItems = Enumerable.Range(1, 150).ToList(); // Your data source
var paginatedItems = PaginationHelper.Paginate(allItems, pageNumber, pageSize);
Console.WriteLine($"Page {pageNumber} contains {paginatedItems.Count} items");

// Get pagination metadata for API responses
var metadata = PaginationHelper.GetMetadata(pageNumber, pageSize, totalCount);
Console.WriteLine($"Metadata: Page {metadata.PageNumber}/{metadata.TotalPages}, Items {metadata.StartIndex}-{metadata.EndIndex} of {metadata.TotalCount}");

// Get page numbers for display in UI (e.g., 1, 2, 3, ..., 10)
var pageNumbers = PaginationHelper.GetPageNumbers(pageNumber, totalPages, maxVisiblePages: 5);
Console.WriteLine($"Display page numbers: {string.Join(", ", pageNumbers)}");

// Example with a service repository
var serviceRepository = new ServiceRepository();
var allServices = await serviceRepository.GetAllAsync();

// Get page 3 with 10 items per page
var servicePage = await serviceRepository.GetPagedAsync(3, 10);
var serviceMetadata = PaginationHelper.GetMetadata(3, 10, allServices.Count());

Console.WriteLine($"Services page 3: {servicePage.Count} items");
Console.WriteLine($"Total pages: {serviceMetadata.TotalPages}");
Console.WriteLine($"Has next: {PaginationHelper.HasNextPage(3, serviceMetadata.TotalPages)}");
Console.WriteLine($"Has previous: {PaginationHelper.HasPreviousPage(3)}");
```

## ValidationHelper

The `ValidationHelper` class provides utility methods for validating common input patterns such as service names, IP addresses, ports, URLs, time ranges, and pagination parameters. It includes both validation and sanitization methods to ensure data integrity when working with systemd service configurations and monitoring data.

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;

// Validate a service name
var serviceNameResult = ValidationHelper.ValidateServiceName("nginx.service");
if (serviceNameResult.IsValid)
{
    Console.WriteLine("Service name is valid");
}
else
{
    Console.WriteLine($"Validation error: {serviceNameResult.ErrorMessage}");
}

// Validate an IP address
var ipResult = ValidationHelper.ValidateIpAddress("192.168.1.100");
Console.WriteLine(ipResult.IsValid ? "Valid IP" : "Invalid IP");

// Validate a port number
var portResult = ValidationHelper.ValidatePort("8080");
Console.WriteLine(portResult.IsValid ? "Valid port" : "Invalid port");

// Validate a URL
var urlResult = ValidationHelper.ValidateUrl("https://example.com/api/services");
Console.WriteLine(urlResult.IsValid ? "Valid URL" : "Invalid URL");

// Validate a time range (HH:mm-HH:mm format)
var timeRangeResult = ValidationHelper.ValidateTimeRange("09:00-17:00");
Console.WriteLine(timeRangeResult.IsValid ? "Valid time range" : "Invalid time range");

// Validate pagination parameters
var paginationResult = ValidationHelper.ValidatePagination(1, 25);
Console.WriteLine(paginationResult.IsValid ? "Valid pagination" : "Invalid pagination");

// Sanitize user input
string userInput = "<script>alert('xss')</script>";
string sanitized = ValidationHelper.SanitizeInput(userInput);
Console.WriteLine($"Sanitized input: {sanitized}");

// Quick validation checks
bool isValidServiceName = ValidationHelper.ValidateServiceName("postgresql").IsValid;
bool isValidPort = ValidationHelper.ValidatePort("80").IsValid;
bool isValidUrl = ValidationHelper.ValidateUrl("http://localhost:5000").IsValid;

Console.WriteLine($"Service name valid: {isValidServiceName}");
Console.WriteLine($"Port valid: {isValidPort}");
Console.WriteLine($"URL valid: {isValidUrl}");
```

## AlertRulesEngine

The `AlertRulesEngine` provides real-time alert evaluation, incident lifecycle management, and escalation policy support for systemd service monitoring.