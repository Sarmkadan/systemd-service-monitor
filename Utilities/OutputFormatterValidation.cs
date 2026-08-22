#nullable enable

using System;
using System.Collections.Generic;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Provides validation helpers for output formatting methods to ensure they receive valid inputs.
/// </summary>
public static class OutputFormatterValidation
{
    /// <summary>
    /// Validates the global output-formatting settings.
    /// </summary>
    /// <remarks>
    /// <see cref="OutputFormatter"/> currently exposes no configurable formatting options,
    /// so this method always reports no errors.
    /// </remarks>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> Validate() => Array.Empty<string>();

    /// <summary>
    /// Checks whether the global output-formatting settings are valid.
    /// </summary>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the global output-formatting settings are valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the settings contain validation errors.</exception>
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
    /// <param name="indent">Whether to indent the JSON output; every value is acceptable.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(
        this IEnumerable<T>? items, bool indent = true) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);

        var errors = new List<string>();

        var index = 0;
        foreach (var item in items)
        {
            if (item is null)
            {
                errors.Add(FormattableString.Invariant(
                    $"Collection contains a null item at index {index}"));
            }

            index++;
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatAsCsv"/>.
    /// </summary>
    /// <param name="services">The services collection to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this IEnumerable<ServiceInfo>? services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var errors = new List<string>();

        foreach (var service in services)
        {
            if (service is null)
            {
                errors.Add("Services collection contains a null service");
                continue;
            }

            if (string.IsNullOrWhiteSpace(service.UnitName))
            {
                errors.Add($"Service has null or empty UnitName: {service.Id}");
            }

            if (service.MainProcessId is < 0)
            {
                errors.Add(FormattableString.Invariant(
                    $"Service '{service.UnitName}' has negative MainProcessId: {service.MainProcessId}"));
            }

            if (service.RestartCount is < 0)
            {
                errors.Add(FormattableString.Invariant(
                    $"Service '{service.UnitName}' has negative RestartCount: {service.RestartCount}"));
            }

            if (service.UptimeSeconds is < 0)
            {
                errors.Add(FormattableString.Invariant(
                    $"Service '{service.UnitName}' has negative UptimeSeconds: {service.UptimeSeconds}"));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatAsTable"/>.
    /// </summary>
    /// <param name="services">The services collection to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateTable(this IEnumerable<ServiceInfo>? services)
        => services.Validate();

    /// <summary>
    /// Validates parameters for <see cref="OutputFormatter.FormatServiceDetails"/>.
    /// </summary>
    /// <param name="service">The service to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ServiceInfo? service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(service.UnitName))
        {
            errors.Add("Service UnitName cannot be null or whitespace");
        }

        if (service.MainProcessId is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"Service '{service.UnitName}' has negative MainProcessId: {service.MainProcessId}"));
        }

        if (service.RestartCount is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"Service '{service.UnitName}' has negative RestartCount: {service.RestartCount}"));
        }

        if (service.UptimeSeconds is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"Service '{service.UnitName}' has negative UptimeSeconds: {service.UptimeSeconds}"));
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
    /// <exception cref="ArgumentNullException"><paramref name="metrics"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SystemResource? metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var errors = new List<string>();

        if (metrics.TotalMemoryMb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative TotalMemoryMb: {metrics.TotalMemoryMb}"));
        }

        if (metrics.AvailableMemoryMb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative AvailableMemoryMb: {metrics.AvailableMemoryMb}"));
        }

        if (metrics.UsedMemoryMb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative UsedMemoryMb: {metrics.UsedMemoryMb}"));
        }

        if (metrics.CachedMemoryMb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative CachedMemoryMb: {metrics.CachedMemoryMb}"));
        }

        if (metrics.CpuCoreCount is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative CpuCoreCount: {metrics.CpuCoreCount}"));
        }

        if (metrics.CpuLoad1Min is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative CpuLoad1Min: {metrics.CpuLoad1Min}"));
        }

        if (metrics.CpuLoad5Min is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative CpuLoad5Min: {metrics.CpuLoad5Min}"));
        }

        if (metrics.CpuLoad15Min is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative CpuLoad15Min: {metrics.CpuLoad15Min}"));
        }

        if (metrics.CpuUsagePercent is < 0 or > 100)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has invalid CpuUsagePercent (must be 0-100): {metrics.CpuUsagePercent}"));
        }

        if (metrics.TotalDiskGb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative TotalDiskGb: {metrics.TotalDiskGb}"));
        }

        if (metrics.UsedDiskGb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative UsedDiskGb: {metrics.UsedDiskGb}"));
        }

        if (metrics.AvailableDiskGb is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative AvailableDiskGb: {metrics.AvailableDiskGb}"));
        }

        if (metrics.DiskIopsPerSecond is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative DiskIopsPerSecond: {metrics.DiskIopsPerSecond}"));
        }

        if (metrics.NetworkBytesIn is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative NetworkBytesIn: {metrics.NetworkBytesIn}"));
        }

        if (metrics.NetworkBytesOut is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative NetworkBytesOut: {metrics.NetworkBytesOut}"));
        }

        if (metrics.RunningProcesses is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative RunningProcesses: {metrics.RunningProcesses}"));
        }

        if (metrics.SystemUptimeSeconds is < 0)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has negative SystemUptimeSeconds: {metrics.SystemUptimeSeconds}"));
        }

        if (metrics.LoadAveragePercent is < 0 or > 100)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has invalid LoadAveragePercent (must be 0-100): {metrics.LoadAveragePercent}"));
        }

        if (metrics.MemoryUsagePercent is < 0 or > 100)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has invalid MemoryUsagePercent (must be 0-100): {metrics.MemoryUsagePercent}"));
        }

        if (metrics.DiskUsagePercent is < 0 or > 100)
        {
            errors.Add(FormattableString.Invariant(
                $"SystemResource has invalid DiskUsagePercent (must be 0-100): {metrics.DiskUsagePercent}"));
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
    public static IReadOnlyList<string> Validate(this double percentage, int width = 20)
    {
        var errors = new List<string>();

        if (!double.IsFinite(percentage))
        {
            errors.Add(FormattableString.Invariant(
                $"Progress percentage is not a valid number: {percentage}"));
        }

        if (percentage is < 0 or > 100)
        {
            errors.Add(FormattableString.Invariant(
                $"Progress percentage must be between 0 and 100: {percentage}"));
        }

        if (width is <= 0)
        {
            errors.Add(FormattableString.Invariant(
                $"Progress bar width must be positive: {width}"));
        }
        else if (width > 1000)
        {
            errors.Add(FormattableString.Invariant(
                $"Progress bar width is excessive (>1000): {width}"));
        }

        return errors.AsReadOnly();
    }
}
