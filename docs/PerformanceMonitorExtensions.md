# PerformanceMonitorExtensions

Provides utility methods for recording, formatting, and summarizing performance checkpoints within the `systemd-service-monitor` project. This static class facilitates lightweight instrumentation of operations by capturing elapsed time and success status, producing human-readable summaries, and formatting durations for logs or reports.

## API

### RecordCheckpoints

```csharp
public static void RecordCheckpoints(this IPerformanceMonitor monitor, params string[] checkpoints)
```

Records one or more named checkpoints on an `IPerformanceMonitor` instance. Each string argument becomes a timestamped marker in the monitor’s internal timeline, enabling later analysis of phase durations.

- **Parameters**:
  - `monitor` — the performance monitor to extend (extension method target).
  - `checkpoints` — a variable-length array of checkpoint names to record in sequence.
- **Returns**: nothing.
- **Throws**: `ArgumentNullException` if `monitor` is `null`; `ArgumentException` if any checkpoint name is `null` or empty.

### GetDetailedSummary

```csharp
public static string GetDetailedSummary(this IPerformanceMonitor monitor)
```

Generates a multi-line string detailing every recorded checkpoint and the elapsed time between consecutive markers, including total duration and success/failure counts where available.

- **Parameters**:
  - `monitor` — the performance monitor to summarize.
- **Returns**: a formatted string containing checkpoint names, inter-checkpoint durations, and aggregate statistics.
- **Throws**: `ArgumentNullException` if `monitor` is `null`.

### MeasureWithSuccess

```csharp
public static (long ElapsedMs, bool Success) MeasureWithSuccess(this IPerformanceMonitor monitor, Action action)
```

Executes a synchronous action, measures its wall-clock duration in milliseconds, and captures whether it completed without throwing.

- **Parameters**:
  - `monitor` — the performance monitor to associate the measurement with.
  - `action` — the delegate to execute and measure.
- **Returns**: a tuple containing the elapsed time in milliseconds and a boolean indicating success (`true` if no exception was thrown).
- **Throws**: `ArgumentNullException` if `monitor` or `action` is `null`. Exceptions thrown by `action` are caught internally and reflected in the `Success` flag; they do **not** propagate.

### MeasureWithSuccessAsync

```csharp
public static async Task<(long ElapsedMs, bool Success)> MeasureWithSuccessAsync(this IPerformanceMonitor monitor, Func<Task> asyncAction)
```

Asynchronously executes a task-returning delegate, measures its wall-clock duration in milliseconds, and captures whether it completed without throwing.

- **Parameters**:
  - `monitor` — the performance monitor to associate the measurement with.
  - `asyncAction` — the asynchronous delegate to execute and measure.
- **Returns**: a task that resolves to a tuple containing the elapsed time in milliseconds and a boolean indicating success (`true` if no exception was thrown).
- **Throws**: `ArgumentNullException` if `monitor` or `asyncAction` is `null`. Exceptions thrown by `asyncAction` are caught internally and reflected in the `Success` flag; they do **not** propagate.

### GetFormattedElapsed

```csharp
public static string GetFormattedElapsed(this IPerformanceMonitor monitor)
```

Returns a human-friendly representation of the total elapsed time recorded by the monitor, formatted as a string (e.g., `"1.234 s"` or `"2 m 15 s"`).

- **Parameters**:
  - `monitor` — the performance monitor whose total elapsed time is formatted.
- **Returns**: a formatted duration string.
- **Throws**: `ArgumentNullException` if `monitor` is `null`.

## Usage

### Example 1: Instrumenting a Service Health Check

```csharp
var monitor = new PerformanceMonitor();
monitor.RecordCheckpoints("start");

var (elapsed, success) = monitor.MeasureWithSuccess(() =>
{
    // Simulate a health check operation
    Thread.Sleep(120);
});

monitor.RecordCheckpoints("health-check-complete");

Console.WriteLine(monitor.GetFormattedElapsed());
Console.WriteLine($"Success: {success}, Elapsed: {elapsed} ms");
Console.WriteLine(monitor.GetDetailedSummary());
```

### Example 2: Async Operation with Multiple Phases

```csharp
var monitor = new PerformanceMonitor();
monitor.RecordCheckpoints("fetch-begin");

var (fetchMs, fetchOk) = await monitor.MeasureWithSuccessAsync(async () =>
{
    await Task.Delay(200); // Simulate I/O
});

monitor.RecordCheckpoints("fetch-end", "process-begin");

var (processMs, processOk) = monitor.MeasureWithSuccess(() =>
{
    Thread.Sleep(80); // Simulate CPU-bound processing
});

monitor.RecordCheckpoints("process-end");

Console.WriteLine(monitor.GetDetailedSummary());
```

## Notes

- All methods are extension methods on `IPerformanceMonitor`; a `null` target always throws `ArgumentNullException`.
- `MeasureWithSuccess` and `MeasureWithSuccessAsync` swallow exceptions thrown by the measured delegate. The `Success` flag is the sole indicator of failure. Callers that need exception details should wrap the delegate themselves before calling these methods.
- `RecordCheckpoints` records markers in the order they are passed. Duplicate or out-of-order names are permitted but may reduce the clarity of `GetDetailedSummary` output.
- Thread safety depends entirely on the underlying `IPerformanceMonitor` implementation. These extension methods do not introduce any additional synchronization; concurrent calls to `RecordCheckpoints` or `MeasureWithSuccess` on the same monitor instance may interleave checkpoints unpredictably.
- `GetFormattedElapsed` and `GetDetailedSummary` are read-only operations and safe to call from multiple threads if the underlying monitor supports concurrent reads.
