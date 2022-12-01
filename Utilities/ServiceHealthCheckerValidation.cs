#nullable enable

using System.Collections.Generic;
using System.Globalization;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="ServiceHealthChecker"/> to ensure service health data is valid.
/// </summary>
public static class ServiceHealthCheckerValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceInfo"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The service information to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the service is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate required string fields
        if (string.IsNullOrWhiteSpace(value.UnitName))
            problems.Add("UnitName is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(value.Description))
            problems.Add("Description is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(value.UnitFilePath))
            problems.Add("UnitFilePath is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(value.RunAsUser))
            problems.Add("RunAsUser is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(value.RunAsGroup))
            problems.Add("RunAsGroup is required and cannot be empty");

        if (string.IsNullOrWhiteSpace(value.WorkingDirectory))
            problems.Add("WorkingDirectory is required and cannot be empty");

        // Validate enum values
        if (value.State == ServiceState.Unknown)
            problems.Add("State is Unknown, which indicates the service state was not properly initialized");

        if (value.LoadState == ServiceLoadState.Unknown)
            problems.Add("LoadState is Unknown, which indicates the service load state was not properly initialized");

        if (value.SubState == ServiceSubState.Unknown)
            problems.Add("SubState is Unknown, which indicates the service sub-state was not properly initialized");

        // Validate numeric ranges
        if (value.RestartCount < 0)
            problems.Add("RestartCount cannot be negative");

        if (value.UptimeSeconds < 0)
            problems.Add("UptimeSeconds cannot be negative");

        if (value.CpuUsagePercent < 0 || value.CpuUsagePercent > 100)
            problems.Add("CpuUsagePercent must be between 0 and 100");

        if (value.MemoryUsageMb < 0)
            problems.Add("MemoryUsageMb cannot be negative");

        if (value.MainProcessId < 0)
            problems.Add("MainProcessId cannot be negative");

        // Validate dates
        if (value.CreatedAt == default)
            problems.Add("CreatedAt has not been initialized");

        if (value.UpdatedAt == default)
            problems.Add("UpdatedAt has not been initialized");

        if (value.CreatedAt > value.UpdatedAt)
            problems.Add("CreatedAt cannot be after UpdatedAt");

        // Validate default dates (within reasonable bounds)
        var minReasonableDate = DateTime.UtcNow.AddYears(-1);
        var maxReasonableDate = DateTime.UtcNow.AddYears(1);

        if (value.CreatedAt < minReasonableDate || value.CreatedAt > maxReasonableDate)
            problems.Add("CreatedAt appears to be an unreasonable date value");

        if (value.UpdatedAt < minReasonableDate || value.UpdatedAt > maxReasonableDate)
            problems.Add("UpdatedAt appears to be an unreasonable date value");

        // Validate lists
        if (value.Dependencies is null)
            problems.Add("Dependencies collection is null");

        if (value.Dependents is null)
            problems.Add("Dependents collection is null");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServiceInfo"/> instance is valid.
    /// </summary>
    /// <param name="value">The service information to check.</param>
    /// <returns>True if the service is valid; otherwise, false.</returns>
    public static bool IsValid(this ServiceInfo value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ServiceInfo"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The service information to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ServiceInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ServiceInfo is not valid. Problems: {string.Join("; ", problems)}",
                nameof(value));
        }
    }
}
