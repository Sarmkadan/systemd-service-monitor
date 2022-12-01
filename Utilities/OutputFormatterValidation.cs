#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Provides validation helpers for output formatting methods to ensure they receive valid inputs.
/// </summary>
public static class OutputFormatterValidation
{
    /// <summary>
    /// Validates formatting parameters.
    /// </summary>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks whether formatting parameters are valid.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that formatting parameters are valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if parameters contain validation errors.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"OutputFormatter parameters are not valid. Problems:\n{string.Join("\n", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatAsJson{T}(IEnumerable{T}, bool)"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection to validate.</param>
    /// <param name="indent">Whether to indent the JSON output.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate<T>(
        this IEnumerable<T>? items, bool indent = true) where T : class
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(items);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatAsCsv"/>.
    /// </summary>
    /// <param name="services">The services collection to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this IEnumerable<ServiceInfo>? services)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(services);

        foreach (var service in services)
        {
            ArgumentNullException.ThrowIfNull(service);

            if (string.IsNullOrWhiteSpace(service.UnitName))
            {
                errors.Add($"Service has null or empty UnitName: {service.Id}");
            }

            if (service.MainProcessId < 0)
            {
                errors.Add($"Service '{service.UnitName}' has negative MainProcessId: {service.MainProcessId}");
            }

            if (service.RestartCount < 0)
            {
                errors.Add($"Service '{service.UnitName}' has negative RestartCount: {service.RestartCount}");
            }

            if (service.UptimeSeconds < 0)
            {
                errors.Add($"Service '{service.UnitName}' has negative UptimeSeconds: {service.UptimeSeconds}");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatAsTable"/>.
    /// </summary>
    /// <param name="services">The services collection to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateTable(this IEnumerable<ServiceInfo>? services)
    {
        return Validate(services);
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatServiceDetails"/>.
    /// </summary>
    /// <param name="service">The service to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this ServiceInfo? service)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(service);

        if (string.IsNullOrWhiteSpace(service.UnitName))
        {
            errors.Add("Service UnitName cannot be null or whitespace");
        }

        if (service.MainProcessId < 0)
        {
            errors.Add($"Service '{service.UnitName}' has negative MainProcessId: {service.MainProcessId}");
        }

        if (service.RestartCount < 0)
        {
            errors.Add($"Service '{service.UnitName}' has negative RestartCount: {service.RestartCount}");
        }

        if (service.UptimeSeconds < 0)
        {
            errors.Add($"Service '{service.UnitName}' has negative UptimeSeconds: {service.UptimeSeconds}");
        }

        if (service.CreatedAt == default)
        {
            errors.Add($"Service '{service.UnitName}' has default CreatedAt date");
        }

        if (service.UpdatedAt == default)
        {
            errors.Add($"Service '{service.UnitName}' has default UpdatedAt date");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatMetricsAsTable"/>.
    /// </summary>
    /// <param name="metrics">The system metrics to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this SystemResource? metrics)
    {
        var errors = new List<string>();

        ArgumentNullException.ThrowIfNull(metrics);

        if (metrics.TotalMemoryMb < 0)
        {
            errors.Add($"SystemResource has negative TotalMemoryMb: {metrics.TotalMemoryMb}");
        }

        if (metrics.AvailableMemoryMb < 0)
        {
            errors.Add($"SystemResource has negative AvailableMemoryMb: {metrics.AvailableMemoryMb}");
        }

        if (metrics.UsedMemoryMb < 0)
        {
            errors.Add($"SystemResource has negative UsedMemoryMb: {metrics.UsedMemoryMb}");
        }

        if (metrics.CpuCoreCount < 0)
        {
            errors.Add($"SystemResource has negative CpuCoreCount: {metrics.CpuCoreCount}");
        }

        if (metrics.CpuLoad1Min < 0)
        {
            errors.Add($"SystemResource has negative CpuLoad1Min: {metrics.CpuLoad1Min}");
        }

        if (metrics.CpuLoad5Min < 0)
        {
            errors.Add($"SystemResource has negative CpuLoad5Min: {metrics.CpuLoad5Min}");
        }

        if (metrics.CpuLoad15Min < 0)
        {
            errors.Add($"SystemResource has negative CpuLoad15Min: {metrics.CpuLoad15Min}");
        }

        if (metrics.CpuUsagePercent < 0 || metrics.CpuUsagePercent > 100)
        {
            errors.Add($"SystemResource has invalid CpuUsagePercent (must be 0-100): {metrics.CpuUsagePercent}");
        }

        if (metrics.TotalDiskGb < 0)
        {
            errors.Add($"SystemResource has negative TotalDiskGb: {metrics.TotalDiskGb}");
        }

        if (metrics.UsedDiskGb < 0)
        {
            errors.Add($"SystemResource has negative UsedDiskGb: {metrics.UsedDiskGb}");
        }

        if (metrics.AvailableDiskGb < 0)
        {
            errors.Add($"SystemResource has negative AvailableDiskGb: {metrics.AvailableDiskGb}");
        }

        if (metrics.DiskIopsPerSecond < 0)
        {
            errors.Add($"SystemResource has negative DiskIopsPerSecond: {metrics.DiskIopsPerSecond}");
        }

        if (metrics.NetworkBytesIn < 0)
        {
            errors.Add($"SystemResource has negative NetworkBytesIn: {metrics.NetworkBytesIn}");
        }

        if (metrics.NetworkBytesOut < 0)
        {
            errors.Add($"SystemResource has negative NetworkBytesOut: {metrics.NetworkBytesOut}");
        }

        if (metrics.RunningProcesses < 0)
        {
            errors.Add($"SystemResource has negative RunningProcesses: {metrics.RunningProcesses}");
        }

        if (metrics.SystemUptimeSeconds < 0)
        {
            errors.Add($"SystemResource has negative SystemUptimeSeconds: {metrics.SystemUptimeSeconds}");
        }

        if (metrics.LoadAveragePercent < 0 || metrics.LoadAveragePercent > 100)
        {
            errors.Add($"SystemResource has invalid LoadAveragePercent (must be 0-100): {metrics.LoadAveragePercent}");
        }

        if (metrics.MemoryUsagePercent < 0 || metrics.MemoryUsagePercent > 100)
        {
            errors.Add($"SystemResource has invalid MemoryUsagePercent (must be 0-100): {metrics.MemoryUsagePercent}");
        }

        if (metrics.DiskUsagePercent < 0 || metrics.DiskUsagePercent > 100)
        {
            errors.Add($"SystemResource has invalid DiskUsagePercent (must be 0-100): {metrics.DiskUsagePercent}");
        }

        if (metrics.RecordedAt == default)
        {
            errors.Add("SystemResource has default RecordedAt date");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.CreateProgressBar"/>.
    /// </summary>
    /// <param name="percentage">The percentage value to validate (0-100).</param>
    /// <param name="width">The width of the progress bar in characters.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(
        this double percentage, int width = 20)
    {
        var errors = new List<string>();

        if (double.IsNaN(percentage) || double.IsInfinity(percentage))
        {
            errors.Add($"Progress percentage is not a valid number: {percentage}");
        }

        if (percentage < 0 || percentage > 100)
        {
            errors.Add($"Progress percentage must be between 0 and 100: {percentage}");
        }

        if (width <= 0)
        {
            errors.Add($"Progress bar width must be positive: {width}");
        }
        else if (width > 1000)
        {
            errors.Add($"Progress bar width is excessive (>1000): {width}");
        }

        return errors.AsReadOnly();
    }
}
