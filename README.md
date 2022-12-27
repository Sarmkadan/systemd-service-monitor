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
