#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;

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

            // Placeholder: would fetch from systemd journald via D-Bus in production
            var logs = new List<ServiceLog>();
            var now = DateTime.UtcNow;

            for (int i = 0; i < count; i++)
            {
                logs.Add(new ServiceLog
                {
                    Id = Guid.NewGuid(),
                    UnitName = unitName,
                    Level = SyslogLevel.Info,
                    Message = $"Sample log message {i + 1}",
                    Timestamp = now.AddSeconds(-i),
                    ProcessId = 1000 + i,
                    Hostname = "localhost",
                    Sequence = (ulong)(i + 1)
                });
            }

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch latest logs from journald");
            throw;
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
