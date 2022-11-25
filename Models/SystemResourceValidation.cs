#nullable enable

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="SystemResource"/> instances.
/// </summary>
public static class SystemResourceValidation
{
    /// <summary>
    /// Validates a <see cref="SystemResource"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The system resource to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SystemResource? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id == Guid.Empty)
        {
            errors.Add("Id must not be empty (Guid.Empty).");
        }

        // Validate memory metrics
        ValidateMemoryMetrics(value, errors);

        // Validate CPU metrics
        ValidateCpuMetrics(value, errors);

        // Validate disk metrics
        ValidateDiskMetrics(value, errors);

        // Validate network metrics
        ValidateNetworkMetrics(value, errors);

        // Validate process count
        ValidateProcessCount(value, errors);

        // Validate uptime
        ValidateUptime(value, errors);

        // Validate percentages
        ValidatePercentages(value, errors);

        // Validate recorded timestamp
        ValidateRecordedAt(value, errors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SystemResource"/> instance is valid.
    /// </summary>
    /// <param name="value">The system resource to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SystemResource? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a <see cref="SystemResource"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The system resource to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid with a detailed message.</exception>
    public static void EnsureValid(this SystemResource? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SystemResource is invalid. Problems: {string.Join("; ", errors)}",
            nameof(value));
    }

    private static void ValidateMemoryMetrics(SystemResource resource, List<string> errors)
    {
        // Memory values should be non-negative
        if (resource.TotalMemoryMb < 0)
        {
            errors.Add("TotalMemoryMb must not be negative.");
        }

        if (resource.AvailableMemoryMb < 0)
        {
            errors.Add("AvailableMemoryMb must not be negative.");
        }

        if (resource.UsedMemoryMb < 0)
        {
            errors.Add("UsedMemoryMb must not be negative.");
        }

        if (resource.CachedMemoryMb < 0)
        {
            errors.Add("CachedMemoryMb must not be negative.");
        }

        // Validate memory consistency
        if (resource.TotalMemoryMb > 0 && resource.AvailableMemoryMb > resource.TotalMemoryMb)
        {
            errors.Add("AvailableMemoryMb cannot exceed TotalMemoryMb.");
        }

        if (resource.UsedMemoryMb > resource.TotalMemoryMb)
        {
            errors.Add("UsedMemoryMb cannot exceed TotalMemoryMb.");
        }

        if (resource.CachedMemoryMb > resource.TotalMemoryMb)
        {
            errors.Add("CachedMemoryMb cannot exceed TotalMemoryMb.");
        }

        // Calculate derived memory usage percentage if not set
        if (resource.MemoryUsagePercent == 0 && resource.TotalMemoryMb > 0)
        {
            var calculatedUsage = (decimal)resource.UsedMemoryMb / resource.TotalMemoryMb * 100;
            if (calculatedUsage > 100)
            {
                errors.Add("MemoryUsagePercent calculated from UsedMemoryMb/TotalMemoryMb exceeds 100%.");
            }
        }
    }

    private static void ValidateCpuMetrics(SystemResource resource, List<string> errors)
    {
        // CPU core count must be positive
        if (resource.CpuCoreCount <= 0)
        {
            errors.Add("CpuCoreCount must be greater than zero.");
        }

        // CPU load values should be non-negative
        if (resource.CpuLoad1Min < 0)
        {
            errors.Add("CpuLoad1Min must not be negative.");
        }

        if (resource.CpuLoad5Min < 0)
        {
            errors.Add("CpuLoad5Min must not be negative.");
        }

        if (resource.CpuLoad15Min < 0)
        {
            errors.Add("CpuLoad15Min must not be negative.");
        }

        // CPU usage percentage should be in valid range
        if (resource.CpuUsagePercent < 0 || resource.CpuUsagePercent > 100)
        {
            errors.Add("CpuUsagePercent must be between 0 and 100 inclusive.");
        }

        // Load average percentage should be in valid range
        if (resource.LoadAveragePercent < 0 || resource.LoadAveragePercent > 100)
        {
            errors.Add("LoadAveragePercent must be between 0 and 100 inclusive.");
        }
    }

    private static void ValidateDiskMetrics(SystemResource resource, List<string> errors)
    {
        // Disk values should be non-negative
        if (resource.TotalDiskGb < 0)
        {
            errors.Add("TotalDiskGb must not be negative.");
        }

        if (resource.UsedDiskGb < 0)
        {
            errors.Add("UsedDiskGb must not be negative.");
        }

        if (resource.AvailableDiskGb < 0)
        {
            errors.Add("AvailableDiskGb must not be negative.");
        }

        if (resource.DiskIopsPerSecond < 0)
        {
            errors.Add("DiskIopsPerSecond must not be negative.");
        }

        // Validate disk consistency
        if (resource.TotalDiskGb > 0 && resource.AvailableDiskGb > resource.TotalDiskGb)
        {
            errors.Add("AvailableDiskGb cannot exceed TotalDiskGb.");
        }

        if (resource.UsedDiskGb > resource.TotalDiskGb)
        {
            errors.Add("UsedDiskGb cannot exceed TotalDiskGb.");
        }

        // Calculate derived disk usage percentage if not set
        if (resource.DiskUsagePercent == 0 && resource.TotalDiskGb > 0)
        {
            var calculatedUsage = (decimal)resource.UsedDiskGb / resource.TotalDiskGb * 100;
            if (calculatedUsage > 100)
            {
                errors.Add("DiskUsagePercent calculated from UsedDiskGb/TotalDiskGb exceeds 100%.");
            }
        }
    }

    private static void ValidateNetworkMetrics(SystemResource resource, List<string> errors)
    {
        // Network metrics should be non-negative
        if (resource.NetworkBytesIn < 0)
        {
            errors.Add("NetworkBytesIn must not be negative.");
        }

        if (resource.NetworkBytesOut < 0)
        {
            errors.Add("NetworkBytesOut must not be negative.");
        }
    }

    private static void ValidateProcessCount(SystemResource resource, List<string> errors)
    {
        if (resource.RunningProcesses < 0)
        {
            errors.Add("RunningProcesses must not be negative.");
        }
    }

    private static void ValidateUptime(SystemResource resource, List<string> errors)
    {
        if (resource.SystemUptimeSeconds < 0)
        {
            errors.Add("SystemUptimeSeconds must not be negative.");
        }
    }

    private static void ValidatePercentages(SystemResource resource, List<string> errors)
    {
        // Memory usage percentage validation
        if (resource.MemoryUsagePercent < 0 || resource.MemoryUsagePercent > 100)
        {
            errors.Add("MemoryUsagePercent must be between 0 and 100 inclusive.");
        }

        // Disk usage percentage validation
        if (resource.DiskUsagePercent < 0 || resource.DiskUsagePercent > 100)
        {
            errors.Add("DiskUsagePercent must be between 0 and 100 inclusive.");
        }
    }

    private static void ValidateRecordedAt(SystemResource resource, List<string> errors)
    {
        // RecordedAt should not be in the future or default DateTime
        if (resource.RecordedAt == default)
        {
            errors.Add("RecordedAt must be set to a valid DateTime.");
        }
        else if (resource.RecordedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("RecordedAt cannot be in the future.");
        }
    }
}