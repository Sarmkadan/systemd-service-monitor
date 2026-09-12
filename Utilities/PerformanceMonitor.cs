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

    private const long DefaultWarningThresholdMs = 1000;
    private const string CheckpointLogFormat = "{OperationName} checkpoint '{CheckpointName}': {ElapsedMs}ms";
    private const string SummaryFormat = "{0}: {1}ms";
    private const string CheckpointSummaryFormat = "{0}:{1}ms";
    private const string WarningLogFormat = "{Message} (exceeded {ThresholdMs}ms threshold)";
    private const string DebugLogFormat = "{Message}";
    private const string OperationTimeSeparator = ": ";
    private const string MillisecondsSuffix = "ms";
    private const string CheckpointSeparator = ", ";
    private const string KeyValueSeparator = ":";
    private const string OpenBracket = " [";
    private const string CloseBracket = "]";

    public PerformanceMonitor(
        string operationName,
        ILogger? logger = null,
        long warningThresholdMs = DefaultWarningThresholdMs)
    {
        ArgumentNullException.ThrowIfNull(operationName);

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
        ArgumentNullException.ThrowIfNull(name);

        _checkpoints[name] = _stopwatch.ElapsedMilliseconds;
        _logger?.LogDebug(CheckpointLogFormat,
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
        ArgumentNullException.ThrowIfNull(startCheckpoint);
        ArgumentNullException.ThrowIfNull(endCheckpoint);

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
        var summary = string.Format(SummaryFormat, _operationName, _stopwatch.ElapsedMilliseconds);

        if (_checkpoints.Any())
        {
            summary += OpenBracket;
            summary += string.Join(CheckpointSeparator, _checkpoints.Select(kvp => string.Format(CheckpointSummaryFormat, kvp.Key, kvp.Value)));
            summary += CloseBracket;
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
            _logger?.LogWarning(WarningLogFormat, message, _warningThresholdMs);
        }
        else
        {
            _logger?.LogDebug(DebugLogFormat, message);
        }
    }
}
