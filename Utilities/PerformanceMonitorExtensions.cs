#nullable enable

using System.Text;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Extension methods for <see cref="PerformanceMonitor"/> providing additional utility functionality.
/// </summary>
public static class PerformanceMonitorExtensions
{
    /// <summary>
    /// Records multiple checkpoints at once with their current elapsed times.
    /// Useful for batch recording of multiple stages in an operation.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="checkpointNames">Names of checkpoints to record. Cannot be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="checkpointNames"/> is <see langword="null"/>.</exception>
    public static void RecordCheckpoints(this PerformanceMonitor monitor, params string[] checkpointNames)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(checkpointNames);

        foreach (var name in checkpointNames)
        {
            monitor.RecordCheckpoint(name);
        }
    }

    /// <summary>
    /// Gets a formatted summary of the operation performance with additional details.
    /// Includes checkpoints and elapsed time.
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>A detailed summary string with checkpoints and elapsed time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static string GetDetailedSummary(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var sb = new StringBuilder();
        sb.AppendLine(monitor.GetSummary());

        var checkpoints = monitor.GetCheckpoints();
        if (checkpoints.Count > 0)
        {
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
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="action">The action to measure. Cannot be null.</param>
    /// <returns>Tuple containing elapsed milliseconds and success status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static (long ElapsedMs, bool Success) MeasureWithSuccess(this PerformanceMonitor monitor, Action action)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(action);

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
    /// <param name="monitor">The performance monitor instance.</param>
    /// <param name="action">The async action to measure. Cannot be null.</param>
    /// <returns>Tuple containing elapsed milliseconds and success status.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static async Task<(long ElapsedMs, bool Success)> MeasureWithSuccessAsync(this PerformanceMonitor monitor, Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action().ConfigureAwait(false);
            return (monitor.ElapsedMilliseconds, true);
        }
        catch
        {
            return (monitor.ElapsedMilliseconds, false);
        }
    }

    /// <summary>
    /// Gets the elapsed time in a human-readable format (HH:mm:ss.fff).
    /// </summary>
    /// <param name="monitor">The performance monitor instance.</param>
    /// <returns>Formatted time span string in HH:mm:ss.fff format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="monitor"/> is <see langword="null"/>.</exception>
    public static string GetFormattedElapsed(this PerformanceMonitor monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);

        var ts = monitor.Elapsed;
        return $"{ts.TotalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
    }
}
