// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Provides a real-time, filterable <see cref="IAsyncEnumerable{T}"/> stream of systemd service logs.
/// Historical entries up to <see cref="LogStreamFilter.BufferSize"/> are replayed first,
/// followed by live tailing until the caller cancels the stream.
/// </summary>
public interface ILogStreamService
{
    /// <summary>
    /// Streams log entries that satisfy <paramref name="filter"/> until <paramref name="ct"/> is cancelled.
    /// </summary>
    /// <param name="filter">Criteria controlling which entries are emitted and replay/polling settings.</param>
    /// <param name="ct">Token that stops the stream when cancelled.</param>
    IAsyncEnumerable<LogStreamEntry> StreamLogsAsync(LogStreamFilter filter, CancellationToken ct = default);
}

/// <inheritdoc cref="ILogStreamService"/>
public sealed class LogStreamService(
    IServiceLogService logService,
    ILogger<LogStreamService> logger) : ILogStreamService
{
    private const int MinPollingMs  = 500;
    private const int MaxPollingMs  = 30_000;
    private const int MaxBufferSize = 500;

    /// <inheritdoc/>
    public async IAsyncEnumerable<LogStreamEntry> StreamLogsAsync(
        LogStreamFilter filter,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var pollingMs  = Math.Clamp(filter.PollingIntervalMs, MinPollingMs, MaxPollingMs);
        var bufferSize = Math.Clamp(filter.BufferSize, 0, MaxBufferSize);
        var cursor     = DateTime.UtcNow;

        logger.LogInformation(
            "Log stream opened — service='{Service}', search='{Term}', minLevel={Level}, buffer={Buffer}",
            filter.ServiceName ?? "*", filter.SearchTerm ?? string.Empty,
            filter.MinLevel?.ToString() ?? "any", bufferSize);

        // ── Historical buffer ─────────────────────────────────────────────────
        if (bufferSize > 0)
        {
            IEnumerable<ServiceLog> initial;
            try
            {
                initial = string.IsNullOrWhiteSpace(filter.ServiceName)
                    ? await logService.SearchLogsAsync(filter.SearchTerm ?? string.Empty, bufferSize, ct)
                    : await logService.GetServiceLogsAsync(filter.ServiceName, bufferSize, ct);
            }
            catch (OperationCanceledException) { yield break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching initial log buffer for stream");
                yield break;
            }

            foreach (var log in ApplyFilter(initial, filter).OrderBy(l => l.Timestamp))
                yield return LogStreamEntry.FromServiceLog(log, isBuffered: true);
        }

        // ── Live tail ─────────────────────────────────────────────────────────
        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(pollingMs, ct); }
            catch (OperationCanceledException) { break; }

            var windowEnd = DateTime.UtcNow;
            IEnumerable<ServiceLog> batch;

            try
            {
                if (!string.IsNullOrWhiteSpace(filter.ServiceName))
                {
                    batch = await logService.GetLogsInTimeRangeAsync(filter.ServiceName, cursor, windowEnd, ct);
                }
                else
                {
                    // Fetch across all services and narrow to the current polling window.
                    var raw = await logService.SearchLogsAsync(filter.SearchTerm ?? string.Empty, MaxBufferSize, ct);
                    batch = raw.Where(l => l.Timestamp > cursor && l.Timestamp <= windowEnd);
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error polling logs during stream; retrying after next interval");
                continue;
            }

            foreach (var log in ApplyFilter(batch, filter).OrderBy(l => l.Timestamp))
            {
                yield return LogStreamEntry.FromServiceLog(log, isBuffered: false);
                if (log.Timestamp > cursor)
                    cursor = log.Timestamp;
            }

            // Advance cursor even when no new entries arrived to avoid re-querying stale windows.
            if (windowEnd > cursor)
                cursor = windowEnd;
        }

        logger.LogInformation("Log stream closed — service='{Service}'", filter.ServiceName ?? "*");
    }

    private static IEnumerable<ServiceLog> ApplyFilter(IEnumerable<ServiceLog> logs, LogStreamFilter filter)
    {
        if (filter.MinLevel.HasValue)
            logs = logs.Where(l => l.Level <= filter.MinLevel.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            logs = logs.Where(l => l.Message.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase));

        return logs;
    }
}
