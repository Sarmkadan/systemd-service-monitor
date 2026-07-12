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
