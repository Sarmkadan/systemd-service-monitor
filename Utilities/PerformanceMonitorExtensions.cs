#nullable enable

using System.Text;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Extension methods for PerformanceMonitor providing additional utility functionality.
/// </summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>
    /// Records multiple checkpoints at once with their current elapsed times.
    /// Useful for batch recording of multiple stages in an operation.
    /// </summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="checkpointNames">Names of checkpoints to record</param>
    public static void RecordCheckpoints(this PerformanceMonitor monitor, params string[] checkpointNames)
    {
        foreach (var name in checkpointNames)
        {
            monitor.RecordCheckpoint(name);
        }
    }

    /// <summary>
    /// Gets a formatted summary of the operation performance with additional details.
    /// Includes checkpoints and elapsed time.
    /// </summary>
    /// <param name="monitor">The performance monitor instance</param>
    public static string GetDetailedSummary(this PerformanceMonitor monitor)
    {
        var sb = new StringBuilder();
        sb.AppendLine(monitor.GetSummary());

        if (monitor.GetCheckpoints().Count > 0)
        {
            var checkpoints = monitor.GetCheckpoints();
            sb.AppendLine("Checkpoints:");
            foreach (var kvp in checkpoints)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}ms");
            }
        }

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Measures the execution time of an action and returns both the elapsed time and a boolean indicating success.
    /// Useful for operations where you need to track both timing and success/failure.
    /// </summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="action">The action to measure</param>
    /// <returns>Tuple containing elapsed milliseconds and success status</returns>
    public static (long ElapsedMs, bool Success) MeasureWithSuccess(this PerformanceMonitor monitor, Action action)
    {
        try
        {
            action();
            return (monitor.ElapsedMilliseconds, true);
        }
        catch
        {
            return (monitor.ElapsedMilliseconds, false);
        }
    }

    /// <summary>
    /// Measures the execution time of an async action and returns both the elapsed time and a boolean indicating success.
    /// Useful for async operations where you need to track both timing and success/failure.
    /// </summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <param name="action">The async action to measure</param>
    /// <returns>Tuple containing elapsed milliseconds and success status</returns>
    public static async Task<(long ElapsedMs, bool Success)> MeasureWithSuccessAsync(this PerformanceMonitor monitor, Func<Task> action)
    {
        try
        {
            await action();
            return (monitor.ElapsedMilliseconds, true);
        }
        catch
        {
            return (monitor.ElapsedMilliseconds, false);
        }
    }

    /// <summary>
    /// Gets the elapsed time in a human-readable format.
    /// </summary>
    /// <param name="monitor">The performance monitor instance</param>
    /// <returns>Formatted time span string</returns>
    public static string GetFormattedElapsed(this PerformanceMonitor monitor)
    {
        var ts = monitor.Elapsed;
        return $"{ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }
}
