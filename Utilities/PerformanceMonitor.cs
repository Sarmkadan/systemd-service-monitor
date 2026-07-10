#nullable enable

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

