using System;
using System.Collections.Generic;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Provides validation helpers for configuration options.
/// </summary>
public static class SystemdOptionsValidation
{
    /// <summary>
    /// Validates the <see cref="SystemdOptions"/> instance.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SystemdOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate EnableMonitoring (no specific validation needed for bool)
        // Validate MetricCollectionIntervalMs
        if (value.MetricCollectionIntervalMs <= 0)
        {
            problems.Add($"MetricCollectionIntervalMs must be positive, but was {value.MetricCollectionIntervalMs}");
        }
        else if (value.MetricCollectionIntervalMs > 3600000) // 1 hour in ms
        {
            problems.Add($"MetricCollectionIntervalMs cannot exceed 1 hour (3600000ms), but was {value.MetricCollectionIntervalMs}");
        }

        // Validate LogRetentionDays
        if (value.LogRetentionDays < 0)
        {
            problems.Add($"LogRetentionDays cannot be negative, but was {value.LogRetentionDays}");
        }
        else if (value.LogRetentionDays > 3650) // ~10 years
        {
            problems.Add($"LogRetentionDays cannot exceed 10 years (3650 days), but was {value.LogRetentionDays}");
        }

        // Validate MaxLogEntriesPerRequest
        if (value.MaxLogEntriesPerRequest <= 0)
        {
            problems.Add($"MaxLogEntriesPerRequest must be positive, but was {value.MaxLogEntriesPerRequest}");
        }
        else if (value.MaxLogEntriesPerRequest > 100000) // 100k entries max
        {
            problems.Add($"MaxLogEntriesPerRequest cannot exceed 100000, but was {value.MaxLogEntriesPerRequest}");
        }

        // Validate EnableRemoteOperations (no specific validation needed for bool)

        // Validate OperationTimeoutMs
        if (value.OperationTimeoutMs <= 0)
        {
            problems.Add($"OperationTimeoutMs must be positive, but was {value.OperationTimeoutMs}");
        }
        else if (value.OperationTimeoutMs > 300000) // 5 minutes in ms
        {
            problems.Add($"OperationTimeoutMs cannot exceed 5 minutes (300000ms), but was {value.OperationTimeoutMs}");
        }

        // Validate ConnectionRetryCount
        if (value.ConnectionRetryCount < 0)
        {
            problems.Add($"ConnectionRetryCount cannot be negative, but was {value.ConnectionRetryCount}");
        }
        else if (value.ConnectionRetryCount > 20)
        {
            problems.Add($"ConnectionRetryCount cannot exceed 20, but was {value.ConnectionRetryCount}");
        }

        // Validate ConnectionRetryDelayMs
        if (value.ConnectionRetryDelayMs <= 0)
        {
            problems.Add($"ConnectionRetryDelayMs must be positive, but was {value.ConnectionRetryDelayMs}");
        }
        else if (value.ConnectionRetryDelayMs > 60000) // 1 minute in ms
        {
            problems.Add($"ConnectionRetryDelayMs cannot exceed 1 minute (60000ms), but was {value.ConnectionRetryDelayMs}");
        }

        // Validate EnableHealthChecks (no specific validation needed for bool)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SystemdOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SystemdOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SystemdOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing the validation problems.</exception>
    public static void EnsureValid(this SystemdOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SystemdOptions is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}
