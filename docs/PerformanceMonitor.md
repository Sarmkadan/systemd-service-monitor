# PerformanceMonitor

A lightweight utility for measuring and recording performance metrics of operations within a `systemd`-managed service. It supports synchronous and asynchronous measurements, checkpoint tracking, and detailed statistics aggregation.

## API

### `public PerformanceMonitor`

Initializes a new `PerformanceMonitor` instance for the specified operation. The monitor is not started until the first measurement is recorded.

### `public void RecordCheckpoint()`

Records a checkpoint with the current elapsed time. Checkpoints are stored in the order they are recorded and can be retrieved via `GetCheckpoints()`.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public Dictionary<string, long> GetCheckpoints()`

Returns a dictionary mapping checkpoint names to their recorded elapsed times in milliseconds. The dictionary is a snapshot and will not reflect subsequent changes.

_Returns:_ A new dictionary containing all recorded checkpoints.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public long GetElapsedBetween(string startCheckpoint, string endCheckpoint)`

Calculates the elapsed time in milliseconds between two recorded checkpoints.

_Parameters:_
- `startCheckpoint`: The name of the starting checkpoint.
- `endCheckpoint`: The name of the ending checkpoint.

_Returns:_ The elapsed time in milliseconds between the two checkpoints.

_Throws:_
- `ArgumentException` if either checkpoint does not exist.
- `InvalidOperationException` if the monitor has been disposed.

### `public string GetSummary()`

Generates a human-readable summary of the performance statistics, including total measurements, average, minimum, and maximum durations.

_Returns:_ A formatted string containing the summary.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public void Dispose()`

Releases all resources used by the `PerformanceMonitor`. Subsequent calls to public members will throw `ObjectDisposedException`.

### `public static PerformanceMonitor CreateMonitor(string operationName = "")`

Creates and returns a new `PerformanceMonitor` instance with the specified operation name.

_Parameters:_
- `operationName`: An optional name for the operation being monitored. Defaults to an empty string.

_Returns:_ A new `PerformanceMonitor` instance.

### `public static long MeasureAction(Action action)`

Measures the execution time of a synchronous action in milliseconds.

_Parameters:_
- `action`: The action to measure.

_Returns:_ The elapsed time in milliseconds.

_Throws:_
- `ArgumentNullException` if `action` is `null`.

### `public static async Task<long> MeasureActionAsync(Func<Task> action)`

Measures the execution time of an asynchronous action in milliseconds.

_Parameters:_
- `action`: The asynchronous action to measure.

_Returns:_ A `Task<long>` representing the elapsed time in milliseconds.

_Throws:_
- `ArgumentNullException` if `action` is `null`.

### `public static T MeasureFunc<T>(Func<T> func)`

Measures the execution time of a synchronous function in milliseconds and returns the function's result.

_Parameters:_
- `func`: The function to measure.

_Returns:_ The result of `func`.

_Throws:_
- `ArgumentNullException` if `func` is `null`.

### `public static async Task<T> MeasureFuncAsync<T>(Func<Task<T>> func)`

Measures the execution time of an asynchronous function in milliseconds and returns the function's result.

_Parameters:_
- `func`: The asynchronous function to measure.

_Returns:_ A `Task<T>` representing the result of `func`.

_Throws:_
- `ArgumentNullException` if `func` is `null`.

### `public void RecordMetric(long elapsedMs)`

Records a custom performance metric with the specified elapsed time in milliseconds.

_Parameters:_
- `elapsedMs`: The elapsed time to record.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public OperationStats? GetStats()`

Returns the aggregated statistics for all recorded metrics. Returns `null` if no metrics have been recorded.

_Returns:_ An `OperationStats` object containing the aggregated statistics, or `null`.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public List<OperationStats> GetAllStats()`

Returns a list of individual statistics for each recorded metric.

_Returns:_ A new list of `OperationStats` objects.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public void Clear()`

Resets all recorded metrics, checkpoints, and statistics. The monitor remains usable after clearing.

_Throws:_
- `InvalidOperationException` if the monitor has been disposed.

### `public string OperationName`

Gets the name of the operation being monitored.

### `public int TotalMeasurements`

Gets the total number of measurements recorded.

### `public long TotalMs`

Gets the total elapsed time in milliseconds across all measurements.

### `public long AverageMs`

Gets the average elapsed time in milliseconds across all measurements.

### `public long MinMs`

Gets the minimum elapsed time in milliseconds across all measurements.

## Usage

### Example 1: Basic Synchronous Measurement
