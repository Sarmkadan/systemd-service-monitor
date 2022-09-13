#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums; // Required for SyslogLevel
using SystemdServiceMonitor.Exceptions; // Required for ServiceMonitorException
using SystemdServiceMonitor.Integration; // Required for IJournal
using Tmds.DBus; // Required for Connection and CreateProxy

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of service log management from systemd journald.
/// </summary>
public class ServiceLogService : IServiceLogService
{
    private readonly ILogger<ServiceLogService> _logger;
    private readonly ILogRepository _logRepository;
    private readonly SystemdOptions _options;

    public ServiceLogService(
        ILogger<ServiceLogService> logger,
        ILogRepository logRepository,
        SystemdOptions options)
    {
        _logger = logger;
        _logRepository = logRepository;
        _options = options;
    }

    public async Task<IEnumerable<ServiceLog>> GetServiceLogsAsync(string unitName, int limit = 100, CancellationToken ct = default)
    {
        try
        {
            limit = Math.Min(limit, _options.MaxLogEntriesPerRequest);
            return await _logRepository.GetByUnitNameAsync(unitName, limit, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetLogsInTimeRangeAsync(string unitName, DateTime from, DateTime to, CancellationToken ct = default)
    {
        try
        {
            var range = to - from;
            var allLogs = await _logRepository.GetByUnitNameAsync(unitName, _options.MaxLogEntriesPerRequest, ct);

            return allLogs
                .Where(l => l.Timestamp >= from && l.Timestamp <= to)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs in time range for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetLogsByLevelAsync(string unitName, SyslogLevel level, CancellationToken ct = default)
    {
        try
        {
            var allLogs = await _logRepository.GetByUnitNameAsync(unitName, _options.MaxLogEntriesPerRequest, ct);
            return allLogs.Where(l => l.Level == level).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve logs by level for service: {ServiceName}", unitName);
            throw;
        }
    }

    public async Task<IEnumerable<ServiceLog>> SearchLogsAsync(string searchTerm, int limit = 100, CancellationToken ct = default)
    {
        try
        {
            limit = Math.Min(limit, _options.MaxLogEntriesPerRequest);
            return await _logRepository.SearchAsync(searchTerm, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search logs");
            throw;
        }
    }

    public async Task<IEnumerable<ServiceLog>> FetchLatestFromJournalAsync(string unitName, int count = 50, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Fetching latest logs from journald for service: {ServiceName}", unitName);

            if (!_connectionService.IsConnected)
            {
                _logger.LogWarning("D-Bus connection not established, attempting to connect.");
                await _connectionService.ConnectAsync(ct);
            }

            var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync();
            var journal = connection.CreateProxy<IJournal>("org.freedesktop.Journal1", "/org/freedesktop/Journal1");

            var logs = new List<ServiceLog>();
            
            // Add match for the specific unit
            await journal.AddMatchAsync($"_SYSTEMD_UNIT={unitName}");
            // Seek to the end of the journal
            await journal.SeekTailAsync();

            ulong entriesCount = 0;
            while (entriesCount < (ulong)count && await journal.NextAsync() > 0)
            {
                var data = await journal.GetDataAsync();
                
                ServiceLog log = new()
                {
                    Id = Guid.NewGuid(), // Generate a new ID for each fetched log
                    UnitName = unitName,
                };

                if (data.TryGetValue("MESSAGE", out string? message))
                {
                    log.Message = message;
                }
                if (data.TryGetValue("PRIORITY", out string? priorityStr) && int.TryParse(priorityStr, out int priorityInt))
                {
                    // Map syslog priority to our SyslogLevel enum
                    log.Level = (SyslogLevel)priorityInt;
                }
                if (data.TryGetValue("_HOSTNAME", out string? hostname))
                {
                    log.Hostname = hostname;
                }
                if (data.TryGetValue("_PID", out string? pidStr) && int.TryParse(pidStr, out int pidInt))
                {
                    log.ProcessId = pidInt;
                }
                if (data.TryGetValue("__REALTIME_TIMESTAMP", out string? timestampStr) && ulong.TryParse(timestampStr, out ulong timestampUs))
                {
                    // Timestamp from Journald is in microseconds since epoch
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    log.Timestamp = epoch.AddTicks((long)timestampUs * 10);
                }
                if (data.TryGetValue("__MONOTONIC_TIMESTAMP", out string? monotonicTimestampStr) && ulong.TryParse(monotonicTimestampStr, out ulong monotonicTimestampUs))
                {
                    log.Sequence = monotonicTimestampUs; // Using monotonic timestamp as sequence
                }
                
                logs.Add(log);
                entriesCount++;
            }
            
            // Clear matches to avoid affecting subsequent queries
            await journal.FlushMatchesAsync();

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch latest logs from journald for service: {ServiceName}", unitName);
            throw new ServiceMonitorException($"Failed to fetch logs for service '{unitName}' from journald", ex);
        }
    }

    public async Task<ServiceLog> StoreLogAsync(ServiceLog log, CancellationToken ct = default)
    {
        try
        {
            return await _logRepository.CreateAsync(log, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store log entry");
            throw;
        }
    }

    public async Task<int> StoreLogsAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default)
    {
        try
        {
            var logList = logs.ToList();
            _logger.LogDebug("Storing {LogCount} log entries", logList.Count);
            return await _logRepository.CreateBatchAsync(logList, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store logs in batch");
            throw;
        }
    }

    public async Task<int> ClearOldLogsAsync(int retentionDays, CancellationToken ct = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            _logger.LogInformation("Clearing logs older than {CutoffDate}", cutoffDate);

            int deletedCount = await _logRepository.DeleteOlderThanAsync(cutoffDate, ct);
            _logger.LogInformation("Deleted {DeletedCount} old log entries", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear old logs");
            throw;
        }
    }

    public async Task<LogStatistics> GetLogStatisticsAsync(string unitName, CancellationToken ct = default)
    {
        try
        {
            var allLogs = (await _logRepository.GetByUnitNameAsync(unitName, 10000, ct)).ToList();

            return new LogStatistics
            {
                UnitName = unitName,
                TotalLogEntries = allLogs.Count,
                ErrorCount = allLogs.Count(l => l.Level == SyslogLevel.Error),
                WarningCount = allLogs.Count(l => l.Level == SyslogLevel.Warning),
                InfoCount = allLogs.Count(l => l.Level == SyslogLevel.Info),
                OldestLogTime = allLogs.Any() ? allLogs.Min(l => l.Timestamp) : DateTime.UtcNow,
                LatestLogTime = allLogs.Any() ? allLogs.Max(l => l.Timestamp) : DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute log statistics");
            throw;
        }
    }
}
