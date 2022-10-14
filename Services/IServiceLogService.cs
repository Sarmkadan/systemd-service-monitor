#nullable enable

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
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="limit">The maximum number of logs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of service logs.</returns>
    Task<IEnumerable<ServiceLog>> GetServiceLogsAsync(string unitName, int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs within a specific time range.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of service logs within the specified time range.</returns>
    Task<IEnumerable<ServiceLog>> GetLogsInTimeRangeAsync(string unitName, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs by severity level.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="level">The syslog severity level to filter by.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of service logs matching the severity level.</returns>
    Task<IEnumerable<ServiceLog>> GetLogsByLevelAsync(string unitName, SyslogLevel level, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the most recent logs across all services.
    /// </summary>
    /// <param name="limit">The maximum number of logs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of recent service logs.</returns>
    Task<IEnumerable<ServiceLog>> GetRecentLogsAsync(int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Retrieves logs matching a search term.
    /// </summary>
    /// <param name="searchTerm">The search term to match against logs.</param>
    /// <param name="limit">The maximum number of logs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of service logs matching the search term.</returns>
    Task<IEnumerable<ServiceLog>> SearchLogsAsync(string searchTerm, int limit = 100, CancellationToken ct = default);

    /// <summary>
    /// Fetches latest logs directly from systemd journald.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="count">The maximum number of logs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of latest logs fetched from the systemd journal.</returns>
    Task<IEnumerable<ServiceLog>> FetchLatestFromJournalAsync(string unitName, int count = 50, CancellationToken ct = default);

    /// <summary>
    /// Fetches latest logs from systemd journald filtered by minimum syslog priority.
    /// Only entries at <paramref name="minimumPriority"/> or higher severity are returned.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="minimumPriority">The minimum syslog priority to include.</param>
    /// <param name="count">The maximum number of logs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of logs matching the priority criteria.</returns>
    Task<IEnumerable<ServiceLog>> FetchFromJournalByPriorityAsync(string unitName, SyslogLevel minimumPriority, int count = 50, CancellationToken ct = default);

    /// <summary>
    /// Stores a log entry in the repository.
    /// </summary>
    /// <param name="log">The log entry to store.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The stored service log.</returns>
    Task<ServiceLog> StoreLogAsync(ServiceLog log, CancellationToken ct = default);

    /// <summary>
    /// Stores multiple log entries in batch.
    /// </summary>
    /// <param name="logs">The collection of log entries to store.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of logs stored.</returns>
    Task<int> StoreLogsAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default);

    /// <summary>
    /// Clears logs older than specified days.
    /// </summary>
    /// <param name="retentionDays">The number of days to retain logs.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The number of logs cleared.</returns>
    Task<int> ClearOldLogsAsync(int retentionDays, CancellationToken ct = default);

    /// <summary>
    /// Gets log statistics for a service.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The log statistics for the service.</returns>
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
