// existing content ...

## PerformanceMonitorExtensions

The `PerformanceMonitorExtensions` class provides utility methods for measuring and monitoring performance in your application. It enables you to record checkpoints, measure execution time, and generate detailed summaries. 

### Usage Example

```csharp
using SystemdServiceMonitor.Utilities;

// Measure synchronous execution time
var (elapsedMs, success) = PerformanceMonitorExtensions.MeasureWithSuccess(() =>
{
    // Code to be measured
    System.Threading.Thread.Sleep(100);
});
Console.WriteLine($"Elapsed: {PerformanceMonitorExtensions.GetFormattedElapsed(elapsedMs)}, Success: {success}");

// Measure asynchronous execution time
var (asyncElapsedMs, asyncSuccess) = await PerformanceMonitorExtensions.MeasureWithSuccessAsync(async () =>
{
    // Asynchronous code to be measured
    await Task.Delay(100);
});
Console.WriteLine($"Elapsed: {PerformanceMonitorExtensions.GetFormattedElapsed(asyncElapsedMs)}, Success: {asyncSuccess}");

// Record a checkpoint
PerformanceMonitorExtensions.RecordCheckpoints("Checkpoint 1");

// Get a detailed summary
var detailedSummary = PerformanceMonitorExtensions.GetDetailedSummary();
Console.WriteLine($"Detailed Summary: {detailedSummary}");
```

## ServiceDetailsDtoExtensions

The `ServiceDetailsDtoExtensions` class provides extension methods for `ServiceDetailsDto` objects, allowing you to easily check the status and health of a service. It includes methods to determine if a service is active, failed, or if auto-start is enabled, as well as methods to get a display string for the service status and a health summary.

### Usage Example

```csharp
using SystemdServiceMonitor.Dtos;

// Create a ServiceDetailsDto object
var serviceDetails = new ServiceDetailsDto();

// Check if the service is active
bool isActive = serviceDetails.IsActive();

// Check if the service has failed
bool isFailed = serviceDetails.IsFailed();

// Get a display string for the service status
string statusDisplay = serviceDetails.GetStatusDisplay();

// Get a health summary for the service
string healthSummary = serviceDetails.GetHealthSummary();

Console.WriteLine($"Is Active: {isActive}, Is Failed: {isFailed}, Status Display: {statusDisplay}, Health Summary: {healthSummary}");
```

## SystemControllerExtensions

The `SystemControllerExtensions` class provides a set of extension methods for `SystemController` that expose health‑related information as API responses. These helpers return `ActionResult<ApiResponse<object>>` objects for simple health status, compact system info, resource health summaries, and critical service counts.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Controllers;

// In a real application the controller instance is typically obtained via dependency injection.
var controller = new SystemController();

// Simple health status
ActionResult<ApiResponse<object>> health = controller.GetSimpleHealthStatus();

// Compact system information
ActionResult<ApiResponse<object>> compactInfo = controller.GetCompactSystemInfo();

// Resource health summary
ActionResult<ApiResponse<object>> resourceSummary = controller.GetResourceHealthSummary();

// Critical service counts
ActionResult<ApiResponse<object>> criticalCounts = controller.GetCriticalServiceCounts();

// Example of using the ResourceThresholds record
var thresholds = new SystemControllerExtensions.ResourceThresholds(
    cpuThreshold: 80,
    memoryThreshold: 1024,
    diskThreshold: 90);
```

## DependencyGraphControllerExtensions

The `DependencyGraphControllerExtensions` class provides utility methods for analyzing and querying service dependency graphs. It enables filtering services by criteria, finding dependent services, retrieving all dependencies for a service, and generating summary statistics about the graph structure and service states.

### Usage Example

```csharp
using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Controllers;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;

// In a real application the controller instance is typically obtained via dependency injection.
var controller = new DependencyGraphController();

// Get a filtered dependency graph containing only active services
ActionResult<ApiResponse<ServiceDependencyGraph>> filteredGraph = await controller.GetFilteredGraph(
    node => node.State == Enums.ServiceState.Active);

// Get all services that depend on the "nginx" service
ActionResult<ApiResponse<List<DependencyNode>>> nginxDependents = await controller.GetDependents("nginx");

// Get all dependencies for the "web-app" service (up to depth 5)
ActionResult<ApiResponse<List<DependencyNode>>> webAppDependencies = await controller.GetAllDependencies("web-app", maxDepth: 5);

// Get summary statistics about the entire dependency graph
ActionResult<ApiResponse<DependencyGraphSummary>> summary = await controller.GetGraphSummary();

// Access summary properties
var graphSummary = summary.Value.Data;
Console.WriteLine($"Total nodes: {graphSummary.TotalNodes}, Total edges: {graphSummary.TotalEdges}");
Console.WriteLine($"Active: {graphSummary.ActiveServices}, Failed: {graphSummary.FailedServices}");
Console.WriteLine($"Generated at: {graphSummary.GeneratedAt}");
```
