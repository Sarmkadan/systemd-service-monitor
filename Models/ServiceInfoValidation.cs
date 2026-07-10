using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="ServiceInfo"/> instances.
/// </summary>
public static class ServiceInfoValidation
{
    /// <summary>
    /// Validates the specified <see cref="ServiceInfo"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The service information to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            problems.Add("Id must not be empty (Guid.Empty).");
        }

        // Validate LoadState
        if (value.LoadState == ServiceLoadState.Unknown)
        {
            problems.Add("LoadState must not be Unknown.");
        }

        // Validate CpuUsagePercent
        if (value.CpuUsagePercent < 0 || value.CpuUsagePercent > 100)
        {
            problems.Add($"CpuUsagePercent must be between 0 and 100 (inclusive), but was {value.CpuUsagePercent.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate MemoryUsageMb
        if (value.MemoryUsageMb < 0)
        {
            problems.Add($"MemoryUsageMb must not be negative, but was {value.MemoryUsageMb}.");
        }

        // Validate UnitName
        if (string.IsNullOrWhiteSpace(value.UnitName))
        {
            problems.Add("UnitName must not be null or whitespace.");
        }
        else if (value.UnitName.Length > 255)
        {
            problems.Add($"UnitName must not exceed 255 characters, but was {value.UnitName.Length}.");
        }

        // Validate Description
        if (value.Description.Length > 1024)
        {
            problems.Add($"Description must not exceed 1024 characters, but was {value.Description.Length}.");
        }

        // Validate UnitFilePath
        if (string.IsNullOrWhiteSpace(value.UnitFilePath))
        {
            problems.Add("UnitFilePath must not be null or whitespace.");
        }
        else if (!value.UnitFilePath.StartsWith('/') && !value.UnitFilePath.StartsWith("@"))
        {
            problems.Add("UnitFilePath must be an absolute path (start with '/') or a template path (start with '@').");
        }

        // Validate State
        if (value.State == ServiceState.Unknown)
        {
            problems.Add("State must not be Unknown.");
        }

        // Validate SubState
        if (value.SubState == ServiceSubState.Unknown)
        {
            problems.Add("SubState must not be Unknown.");
        }

        // Validate MainProcessId
        if (value.MainProcessId < 0)
        {
            problems.Add($"MainProcessId must not be negative, but was {value.MainProcessId}.");
        }

        // Validate Result
        if (value.Result.Length > 128)
        {
            problems.Add($"Result must not exceed 128 characters, but was {value.Result.Length}.");
        }

        // Validate RestartPolicy
        // RestartPolicy.No is valid as a default

        // Validate AutoStart and Restart (boolean flags)
        // No specific validation needed beyond being boolean

        // Validate Dependencies
        if (value.Dependencies is null)
        {
            problems.Add("Dependencies must not be null.");
        }
        else
        {
            foreach (var dependency in value.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependency))
                {
                    problems.Add("Dependencies must not contain null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate Dependents
        if (value.Dependents is null)
        {
            problems.Add("Dependents must not be null.");
        }
        else
        {
            foreach (var dependent in value.Dependents)
            {
                if (string.IsNullOrWhiteSpace(dependent))
                {
                    problems.Add("Dependents must not contain null or whitespace entries.");
                    break;
                }
            }
        }

        // Validate LastStartTime
        if (value.LastStartTime.HasValue && value.LastStartTime.Value > DateTime.UtcNow)
        {
            problems.Add("LastStartTime must not be in the future.");
        }

        // Validate LastStopTime
        if (value.LastStopTime.HasValue && value.LastStopTime.Value > DateTime.UtcNow)
        {
            problems.Add("LastStopTime must not be in the future.");
        }

        // Validate LastStartTime vs LastStopTime
        if (value.LastStartTime.HasValue && value.LastStopTime.HasValue &&
            value.LastStartTime.Value > value.LastStopTime.Value)
        {
            problems.Add("LastStartTime must not be after LastStopTime.");
        }

        // Validate UptimeSeconds
        if (value.UptimeSeconds < 0)
        {
            problems.Add($"UptimeSeconds must not be negative, but was {value.UptimeSeconds}.");
        }
        else if (value.UptimeSeconds > TimeSpan.FromDays(365).TotalSeconds)
        {
            problems.Add($"UptimeSeconds seems unrealistic (>{TimeSpan.FromDays(365).TotalSeconds}), but was {value.UptimeSeconds}.");
        }

        // Validate RestartCount
        if (value.RestartCount < 0)
        {
            problems.Add($"RestartCount must not be negative, but was {value.RestartCount}.");
        }

        // Validate WorkingDirectory
        if (!string.IsNullOrEmpty(value.WorkingDirectory) &&
            !value.WorkingDirectory.StartsWith('/') &&
            !value.WorkingDirectory.StartsWith("@"))
        {
            problems.Add("WorkingDirectory must be an absolute path (start with '/') or a template path (start with '@') when specified.");
        }

        // Validate RunAsUser
        if (value.RunAsUser.Length > 64)
        {
            problems.Add($"RunAsUser must not exceed 64 characters, but was {value.RunAsUser.Length}.");
        }

        // Validate RunAsGroup
        if (value.RunAsGroup.Length > 64)
        {
            problems.Add($"RunAsGroup must not exceed 64 characters, but was {value.RunAsGroup.Length}.");
        }

        // Validate CreatedAt
        if (value.CreatedAt > DateTime.UtcNow)
        {
            problems.Add("CreatedAt must not be in the future.");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt > DateTime.UtcNow)
        {
            problems.Add("UpdatedAt must not be in the future.");
        }

        // Validate UpdatedAt vs CreatedAt
        if (value.UpdatedAt < value.CreatedAt)
        {
            problems.Add("UpdatedAt must not be before CreatedAt.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServiceInfo"/> instance is valid.
    /// </summary>
    /// <param name="value">The service information to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ServiceInfo value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ServiceInfo"/> instance is valid.
    /// </summary>
    /// <param name="value">The service information to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid. The exception message contains the validation problems.</exception>
    public static void EnsureValid(this ServiceInfo value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ServiceInfo validation failed:{Environment.NewLine}- {
                string.Join($"{Environment.NewLine}- ", problems)
            }");
    }
}