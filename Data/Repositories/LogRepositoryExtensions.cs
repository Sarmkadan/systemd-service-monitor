#nullable enable

using System;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// Extension methods for <see cref="LogRepository"/> providing additional utility operations.
/// </summary>
public static class LogRepositoryExtensions
{
    /// <summary>
    /// Gets logs filtered by service name with optional time range and level filtering.
    /// </summary>
    /// <param name="repository">The log repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is empty or whitespace</exception>
    /// <param name="serviceName">The service name to filter by</param>
    /// <param name="timeRange">Optional time range to filter logs within</param>
    /// <param name="level">Optional log level to filter by</param>
    /// <param name="limit">Maximum number of logs to return</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtered collection of service logs</returns>
    public static async Task<IEnumerable<ServiceLog>> GetByServiceNameAsync(
        this LogRepository repository,
        string serviceName,
        TimeSpan? timeRange = null,
        SyslogLevel? level = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(serviceName);
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be empty or whitespace.", nameof(serviceName));
        }

        var logs = await repository.GetByUnitNameAsync(serviceName, limit, ct);

        if (timeRange.HasValue)
        {
            var cutoff = DateTime.UtcNow.Subtract(timeRange.Value);
            logs = logs.Where(l => l.Timestamp >= cutoff);
        }

        if (level.HasValue)
        {
            logs = logs.Where(l => l.Level == level.Value);
        }

        return logs.OrderByDescending(l => l.Timestamp);
    }

    /// <summary>
    /// Gets the most recent log entry for a specific service.
    /// </summary>
    /// <param name="repository">The log repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <param name="serviceId">The service ID to get the latest log for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The most recent log entry, or null if none exists</returns>
    public static async Task<ServiceLog?> GetLatestForServiceAsync(
        this LogRepository repository,
        Guid serviceId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var logs = await repository.GetByServiceIdAsync(serviceId, limit: 1, ct);
        return logs.FirstOrDefault();
    }

    /// <summary>
    /// Gets logs grouped by their level with counts for each level.
    /// </summary>
    /// <param name="repository">The log repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dictionary mapping log levels to their respective log counts</returns>
    public static async Task<Dictionary<SyslogLevel, int>> GetLogLevelCountsAsync(
        this LogRepository repository,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var result = new Dictionary<SyslogLevel, int>();

        foreach (SyslogLevel level in Enum.GetValues(typeof(SyslogLevel)))
        {
            var logs = await repository.GetByLevelAsync(level, ct);
            result[level] = logs.Count();
        }

        return result;
    }

    /// <summary>
    /// Gets logs that match any of the specified log levels.
    /// </summary>
    /// <param name="repository">The log repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentNullException"><paramref name="levels"/> is <see langword="null"/></exception>
    /// <param name="levels">Collection of log levels to filter by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Logs matching any of the specified levels</returns>
    public static async Task<IEnumerable<ServiceLog>> GetByLevelsAsync(
        this LogRepository repository,
        IEnumerable<SyslogLevel> levels,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(levels);

        var result = new List<ServiceLog>();

        foreach (var level in levels.Distinct())
        {
            var logs = await repository.GetByLevelAsync(level, ct);
            result.AddRange(logs);
        }

        return result.OrderByDescending(l => l.Timestamp);
    }
}