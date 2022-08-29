// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for managing service logs from systemd journald.
/// </summary>
public interface IServiceLogService
{
    /// <summary>
    /// Retrieves recent logs for a specific service.
    /// </summary>
    Task<IEnumerable<ServiceLog>> GetServiceLogsAsync(string unitName, int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs within a specific time range.
    /// </summary>
    Task<IEnumerable<ServiceLog>> GetLogsInTimeRangeAsync(string unitName, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs by severity level.
    /// </summary>
    Task<IEnumerable<ServiceLog>> GetLogsByLevelAsync(string unitName, LogLevel level, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs matching a search term.
    /// </summary>
    Task<IEnumerable<ServiceLog>> SearchLogsAsync(string searchTerm, int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Fetches latest logs directly from systemd journald.
    /// </summary>
    Task<IEnumerable<ServiceLog>> FetchLatestFromJournalAsync(string unitName, int count = 50, CancellationToken ct = default);

    /// <summary>
    /// Stores a log entry in the repository.
    /// </summary>
    Task<ServiceLog> StoreLogAsync(ServiceLog log, CancellationToken ct = default);

    /// <summary>
    /// Stores multiple log entries in batch.
    /// </summary>
    Task<int> StoreLogsAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default);

    /// <summary>
    /// Clears logs older than specified days.
    /// </summary>
    Task<int> ClearOldLogsAsync(int retentionDays, CancellationToken ct = default);

    /// <summary>
    /// Gets log statistics for a service.
    /// </summary>
    Task<LogStatistics> GetLogStatisticsAsync(string unitName, CancellationToken ct = default);
}

/// <summary>
/// Statistics about service logs.
/// </summary>
public class LogStatistics
{
    public string UnitName { get; set; } = string.Empty;
    public long TotalLogEntries { get; set; }
    public long ErrorCount { get; set; }
    public long WarningCount { get; set; }
    public long InfoCount { get; set; }
    public DateTime OldestLogTime { get; set; }
    public DateTime LatestLogTime { get; set; }
}
