#nullable enable

using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Configuration;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Provides validation helpers for <see cref="ServiceLogService"/> method arguments and related types.
/// </summary>
public static class ServiceLogServiceValidation
{
    /// <summary>
    /// Validates <see cref="ServiceLog"/> instance for storage operations.
    /// </summary>
    /// <param name="log">The log entry to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this ServiceLog? log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(log.UnitName))
        {
            problems.Add("UnitName cannot be null, empty, or whitespace.");
        }

        // Validate message
        if (string.IsNullOrWhiteSpace(log.Message))
        {
            problems.Add("Message cannot be null, empty, or whitespace.");
        }

        // Validate Level is within valid range
        if (!Enum.IsDefined(typeof(SyslogLevel), log.Level))
        {
            problems.Add($"Level '{log.Level}' is not a valid SyslogLevel value.");
        }

        // Validate ProcessId (should be non-negative)
        if (log.ProcessId < 0)
        {
            problems.Add("ProcessId cannot be negative.");
        }

        // Validate UserId (should be non-negative)
        if (log.UserId < 0)
        {
            problems.Add("UserId cannot be negative.");
        }

        // Validate CodeLine (should be non-negative)
        if (log.CodeLine < 0)
        {
            problems.Add("CodeLine cannot be negative.");
        }

        // Validate Timestamp is not default (not set)
        if (log.Timestamp == default)
        {
            problems.Add("Timestamp cannot be default (not set).");
        }

        // Validate Sequence is not default
        if (log.Sequence == default)
        {
            problems.Add("Sequence cannot be default (not set).");
        }

        // Validate required string properties that can be empty but not null
        if (log.Hostname is null)
        {
            problems.Add("Hostname cannot be null.");
        }

        if (log.CodeFile is null)
        {
            problems.Add("CodeFile cannot be null.");
        }

        if (log.CodeFunction is null)
        {
            problems.Add("CodeFunction cannot be null.");
        }

        if (log.BootId is null)
        {
            problems.Add("BootId cannot be null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServiceLog"/> instance is valid.
    /// </summary>
    /// <param name="log">The log entry to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ServiceLog? log)
    {
        return Validate(log).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ServiceLog"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="log">The log entry to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the log entry is not valid.</exception>
    public static void EnsureValid(this ServiceLog? log)
    {
        var problems = Validate(log);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Log instance is not valid. Problems:\n- " + string.Join("\n- ", problems));
        }
    }

    /// <summary>
    /// Validates <see cref="LogStatistics"/> instance.
    /// </summary>
    /// <param name="stats">The log statistics to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this LogStatistics? stats)
    {
        ArgumentNullException.ThrowIfNull(stats);

        var problems = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(stats.UnitName))
        {
            problems.Add("UnitName cannot be null, empty, or whitespace.");
        }

        // Validate counts are non-negative
        if (stats.TotalLogEntries < 0)
        {
            problems.Add("TotalLogEntries cannot be negative.");
        }

        if (stats.ErrorCount < 0)
        {
            problems.Add("ErrorCount cannot be negative.");
        }

        if (stats.WarningCount < 0)
        {
            problems.Add("WarningCount cannot be negative.");
        }

        if (stats.InfoCount < 0)
        {
            problems.Add("InfoCount cannot be negative.");
        }

        // Validate timestamps are not default
        if (stats.OldestLogTime == default)
        {
            problems.Add("OldestLogTime cannot be default (not set).");
        }

        if (stats.LatestLogTime == default)
        {
            problems.Add("LatestLogTime cannot be default (not set).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="LogStatistics"/> instance is valid.
    /// </summary>
    /// <param name="stats">The log statistics to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this LogStatistics? stats)
    {
        return Validate(stats).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="LogStatistics"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="stats">The log statistics to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the log statistics are not valid.</exception>
    public static void EnsureValid(this LogStatistics? stats)
    {
        var problems = Validate(stats);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "LogStatistics instance is not valid. Problems:\n- " + string.Join("\n- ", problems));
        }
    }

    /// <summary>
    /// Validates method arguments for <see cref="ServiceLogService"/> public methods.
    /// </summary>
    /// <param name="unitName">The service unit name to validate.</param>
    /// <param name="limit">The maximum number of entries to retrieve.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(string? unitName, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(unitName);
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            problems.Add("UnitName cannot be null, empty, or whitespace.");
        }

        if (limit <= 0)
        {
            problems.Add("Limit must be a positive number.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method arguments for time range queries.
    /// </summary>
    /// <param name="unitName">The service unit name to validate.</param>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(string? unitName, DateTime from, DateTime to)
    {
        ArgumentNullException.ThrowIfNull(unitName);
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            problems.Add("UnitName cannot be null, empty, or whitespace.");
        }

        if (from == default)
        {
            problems.Add("From timestamp cannot be default (not set).");
        }

        if (to == default)
        {
            problems.Add("To timestamp cannot be default (not set).");
        }

        if (from > to)
        {
            problems.Add("From timestamp cannot be after To timestamp.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method arguments for search operations.
    /// </summary>
    /// <param name="searchTerm">The search term to validate.</param>
    /// <param name="limit">The maximum number of entries to retrieve.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateSearch(string? searchTerm, int limit = 100)
    {
        ArgumentNullException.ThrowIfNull(searchTerm);
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            problems.Add("SearchTerm cannot be null, empty, or whitespace.");
        }

        if (limit <= 0)
        {
            problems.Add("Limit must be a positive number.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method arguments for priority-based journal queries.
    /// </summary>
    /// <param name="unitName">The service unit name to validate.</param>
    /// <param name="minimumPriority">The minimum syslog priority level.</param>
    /// <param name="count">The maximum number of entries to retrieve.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(string? unitName, SyslogLevel minimumPriority, int count = 50)
    {
        ArgumentNullException.ThrowIfNull(unitName);
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            problems.Add("UnitName cannot be null, empty, or whitespace.");
        }

        if (!Enum.IsDefined(typeof(SyslogLevel), minimumPriority))
        {
            problems.Add($"MinimumPriority '{minimumPriority}' is not a valid SyslogLevel value.");
        }

        if (count <= 0)
        {
            problems.Add("Count must be a positive number.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates method arguments for retention-based log clearing.
    /// </summary>
    /// <param name="retentionDays">The number of days to retain logs.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(int retentionDays)
    {
        var problems = new List<string>();

        if (retentionDays < 0)
        {
            problems.Add("RetentionDays cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified validation results indicate a valid state.
    /// </summary>
    /// <param name="problems">The list of validation problems.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this IReadOnlyList<string> problems)
    {
        return problems.Count == 0;
    }

    /// <summary>
    /// Ensures that the specified validation results indicate a valid state, throwing an exception if not.
    /// </summary>
    /// <param name="problems">The list of validation problems.</param>
    /// <exception cref="ArgumentException">Thrown when there are validation problems.</exception>
    public static void EnsureValid(this IReadOnlyList<string> problems)
    {
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Validation failed. Problems:\n- " + string.Join("\n- ", problems));
        }
    }
}