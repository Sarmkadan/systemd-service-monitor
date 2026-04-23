#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Utility for monitoring and measuring performance of operations.
/// Provides stopwatch-like functionality with detailed metrics tracking.
/// </summary>
public class PerformanceMonitor : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly ILogger? _logger;
    private readonly long _warningThresholdMs;
    private readonly Dictionary<string, long> _checkpoints = new();

    public PerformanceMonitor(
        string operationName,
        ILogger? logger = null,
        long warningThresholdMs = 1000)
    {
        _operationName = operationName;
        _logger = logger;
        _warningThresholdMs = warningThresholdMs;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Records a checkpoint with the current elapsed time.
    /// </summary>
    public void RecordCheckpoint(string name)
    {
        _checkpoints[name] = _stopwatch.ElapsedMilliseconds;
        _logger?.LogDebug("{OperationName} checkpoint '{CheckpointName}': {ElapsedMs}ms",
            _operationName, name, _stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Gets the elapsed time since the operation started.
    /// </summary>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    /// <summary>
    /// Gets the elapsed time as a TimeSpan.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets all recorded checkpoints with their timings.
    /// </summary>
    public Dictionary<string, long> GetCheckpoints() => new(_checkpoints);

    /// <summary>
    /// Calculates the time between two checkpoints.
    /// </summary>
    public long GetElapsedBetween(string startCheckpoint, string endCheckpoint)
    {
        if (!_checkpoints.TryGetValue(startCheckpoint, out var start))
            return -1;
        if (!_checkpoints.TryGetValue(endCheckpoint, out var end))
            return -1;

        return end - start;
    }

    /// <summary>
    /// Gets a formatted summary of the operation performance.
    /// </summary>
    public string GetSummary()
    {
        var summary = $"{_operationName}: {_stopwatch.ElapsedMilliseconds}ms";

        if (_checkpoints.Any())
        {
            summary += " [";
            summary += string.Join(", ", _checkpoints.Select(kvp => $"{kvp.Key}:{kvp.Value}ms"));
            summary += "]";
        }

        return summary;
    }

    public void Dispose()
    {
        _stopwatch.Stop();

        var message = GetSummary();
        var elapsedMs = _stopwatch.ElapsedMilliseconds;

        if (elapsedMs > _warningThresholdMs)
        {
            _logger?.LogWarning("{Message} (exceeded {ThresholdMs}ms threshold)",
                message, _warningThresholdMs);
        }
        else
        {
            _logger?.LogDebug("{Message}", message);
        }
    }
}

/// <summary>
/// Extension methods for performance monitoring.
/// </summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>
    /// Creates a performance monitor using 'using' statement.
    /// </summary>
    public static PerformanceMonitor CreateMonitor(
        this ILogger logger,
        string operationName,
        long warningThresholdMs = 1000)
    {
        return new PerformanceMonitor(operationName, logger, warningThresholdMs);
    }

    /// <summary>
    /// Measures the execution time of an action.
    /// </summary>
    public static long MeasureAction(this ILogger logger, string operationName, Action action)
    {
        using var monitor = new PerformanceMonitor(operationName, logger);
        action();
        return monitor.ElapsedMilliseconds;
    }

    /// <summary>
    /// Measures the execution time of an async action.
    /// </summary>
    public static async Task<long> MeasureActionAsync(
        this ILogger logger,
        string operationName,
        Func<Task> action)
    {
        using var monitor = new PerformanceMonitor(operationName, logger);
        await action();
        return monitor.ElapsedMilliseconds;
    }

    /// <summary>
    /// Measures the execution time of a function and returns its result.
    /// </summary>
    public static T MeasureFunc<T>(
        this ILogger logger,
        string operationName,
        Func<T> func)
    {
        using var monitor = new PerformanceMonitor(operationName, logger);
        return func();
    }

    /// <summary>
    /// Measures the execution time of an async function and returns its result.
    /// </summary>
    public static async Task<T> MeasureFuncAsync<T>(
        this ILogger logger,
        string operationName,
        Func<Task<T>> func)
    {
        using var monitor = new PerformanceMonitor(operationName, logger);
        return await func();
    }
}

/// <summary>
/// Collects performance metrics across multiple operations.
/// Useful for performance analysis and optimization.
/// </summary>
public class PerformanceMetricsCollector
{
    private readonly Dictionary<string, List<long>> _metrics = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Records a measurement for an operation.
    /// </summary>
    public void RecordMetric(string operationName, long elapsedMs)
    {
        lock (_lockObject)
        {
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new List<long>();
            }

            _metrics[operationName].Add(elapsedMs);
        }
    }

    /// <summary>
    /// Gets statistics for an operation.
    /// </summary>
    public OperationStats? GetStats(string operationName)
    {
        lock (_lockObject)
        {
            if (!_metrics.TryGetValue(operationName, out var measurements))
                return null;

            if (measurements.Count == 0)
                return null;

            return new OperationStats
            {
                OperationName = operationName,
                TotalMeasurements = measurements.Count,
                TotalMs = measurements.Sum(),
                AverageMs = (long)measurements.Average(),
                MinMs = measurements.Min(),
                MaxMs = measurements.Max(),
                MedianMs = GetMedian(measurements),
                P95Ms = GetPercentile(measurements, 95),
                P99Ms = GetPercentile(measurements, 99)
            };
        }
    }

    /// <summary>
    /// Gets statistics for all operations.
    /// </summary>
    public List<OperationStats> GetAllStats()
    {
        lock (_lockObject)
        {
            return _metrics.Keys.Select(op => GetStats(op)).OfType<OperationStats>().ToList();
        }
    }

    /// <summary>
    /// Clears all recorded metrics.
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _metrics.Clear();
        }
    }

    private static long GetMedian(List<long> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int count = sorted.Count;
        if (count % 2 == 0)
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        return sorted[count / 2];
    }

    private static long GetPercentile(List<long> values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int index = (int)((percentile / 100.0) * sorted.Count);
        return sorted[Math.Min(index, sorted.Count - 1)];
    }
}

/// <summary>
/// Statistics for an operation.
/// </summary>
public class OperationStats
{
    public string OperationName { get; set; } = string.Empty;
    public int TotalMeasurements { get; set; }
    public long TotalMs { get; set; }
    public long AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public long MedianMs { get; set; }
    public long P95Ms { get; set; }
    public long P99Ms { get; set; }

    public override string ToString()
    {
        return $"{OperationName}: {TotalMeasurements} measurements, " +
               $"avg={AverageMs}ms, min={MinMs}ms, max={MaxMs}ms, p95={P95Ms}ms, p99={P99Ms}ms";
    }
}
