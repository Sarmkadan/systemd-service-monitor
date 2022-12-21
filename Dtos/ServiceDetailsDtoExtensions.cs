#nullable enable

namespace SystemdServiceMonitor.Dtos;

/// <summary>
/// Extension methods for <see cref="ServiceDetailsDto"/> providing utility operations.
/// </summary>
public static class ServiceDetailsDtoExtensions
{
    /// <summary>
    /// Determines if the service is currently active (running or activating).
    /// </summary>
    /// <param name="service">The service details to check.</param>
    /// <returns>True if the service is active; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool IsActive(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.State.Equals("active", StringComparison.OrdinalIgnoreCase)
               || service.State.Equals("activating", StringComparison.OrdinalIgnoreCase)
               || service.State.Equals("reloading", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the service is in a failed state.
    /// </summary>
    /// <param name="service">The service details to check.</param>
    /// <returns>True if the service is failed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool IsFailed(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return string.Equals(service.State, "failed", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines if the service is enabled to start automatically at boot.
    /// </summary>
    /// <param name="service">The service details to check.</param>
    /// <returns>True if auto-start is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool IsAutoStartEnabled(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.AutoStart && service.Restart;
    }

    /// <summary>
    /// Gets a formatted status string suitable for display in dashboards.
    /// </summary>
    /// <param name="service">The service details.</param>
    /// <returns>Formatted status string with emoji and state information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static string GetStatusDisplay(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var stateLower = service.State.ToLowerInvariant();
        var subStateLower = service.SubState.ToLowerInvariant();

        return stateLower switch
        {
            "active" when subStateLower.Contains("running") => "✓ Running",
            "active" when subStateLower.Contains("exited") => "✓ Exited (success)",
            "active" when subStateLower.Contains("waiting") => "✓ Waiting",
            "active" => "✓ Active",
            "inactive" when subStateLower.Contains("dead") => "○ Dead",
            "inactive" when subStateLower.Contains("failed") => "✗ Failed",
            "inactive" => "○ Inactive",
            "failed" => "✗ Failed",
            "activating" => "↻ Activating",
            "deactivating" => "↻ Deactivating",
            "reloading" => "↻ Reloading",
            _ => $"? {service.State}/{service.SubState}"
        };
    }

    /// <summary>
    /// Gets a health status summary combining multiple health indicators.
    /// </summary>
    /// <param name="service">The service details.</param>
    /// <returns>Health status summary string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static string GetHealthSummary(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        // If HealthStatus is explicitly set, use it
        if (!string.IsNullOrWhiteSpace(service.HealthStatus))
        {
            return service.HealthStatus;
        }

        // Derive health status from state and result
        if (service.IsFailed())
        {
            return "Unhealthy - Service failed";
        }

        if (service.State.Equals("active", StringComparison.OrdinalIgnoreCase))
        {
            if (service.Result.Equals("success", StringComparison.OrdinalIgnoreCase))
            {
                return "Healthy - Running successfully";
            }

            if (service.Result.Equals("exit-code", StringComparison.OrdinalIgnoreCase))
            {
                return "Warning - Exited with non-zero code";
            }

            return "Healthy - Active";
        }

        if (service.State.Equals("inactive", StringComparison.OrdinalIgnoreCase))
        {
            return "Unhealthy - Not running";
        }

        return "Unknown";
    }

    /// <summary>
    /// Gets the formatted uptime as a human-readable string.
    /// </summary>
    /// <param name="service">The service details.</param>
    /// <returns>Formatted uptime string (e.g., "2h 30m 15s").</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static string GetFormattedUptime(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (service.UptimeSeconds <= 0)
        {
            return "0s";
        }

        var uptime = TimeSpan.FromSeconds(service.UptimeSeconds);
        return uptime.ToHumanReadableString();
    }

    /// <summary>
    /// Determines if the service should be restarted based on its restart policy.
    /// </summary>
    /// <param name="service">The service details.</param>
    /// <returns>True if the service should be restarted; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    public static bool ShouldRestart(this ServiceDetailsDto service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.Restart && service.RestartCount > 0;
    }
}

/// <summary>
/// Extension methods for <see cref="TimeSpan"/> to provide human-readable formatting.
/// </summary>
internal static class TimeSpanExtensions
{
    /// <summary>
    /// Converts <see cref="TimeSpan"/> to a human-readable string (e.g., "2h 30m 15s").
    /// </summary>
    /// <param name="timeSpan">The time span to format.</param>
    /// <returns>Human-readable string representation.</returns>
    internal static string ToHumanReadableString(this TimeSpan timeSpan)
    {
        var parts = new List<string>();

        if (timeSpan.TotalDays >= 1)
        {
            var days = (int)timeSpan.TotalDays;
            parts.Add($"{days}d");
            timeSpan = timeSpan.Subtract(TimeSpan.FromDays(days));
        }

        if (timeSpan.TotalHours >= 1)
        {
            var hours = (int)timeSpan.TotalHours;
            parts.Add($"{hours}h");
            timeSpan = timeSpan.Subtract(TimeSpan.FromHours(hours));
        }

        if (timeSpan.TotalMinutes >= 1)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            parts.Add($"{minutes}m");
            timeSpan = timeSpan.Subtract(TimeSpan.FromMinutes(minutes));
        }

        if (timeSpan.TotalSeconds >= 1 || parts.Count == 0)
        {
            var seconds = (int)timeSpan.TotalSeconds;
            parts.Add($"{seconds}s");
        }

        return string.Join(" ", parts);
    }
}