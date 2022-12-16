#nullable enable

using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Extension methods for <see cref="ServiceInfo"/> providing useful utility functionality.
/// </summary>
public static class ServiceInfoExtensions
{
    /// <summary>
    /// Determines if the service is currently active and running.
    /// </summary>
    /// <param name="service">The service to check.</param>
    /// <returns>True if the service is active, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static bool IsActive(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.State is ServiceState.Active or ServiceState.Activating;
    }

    /// <summary>
    /// Determines if the service is currently failed or in a failure state.
    /// </summary>
    /// <param name="service">The service to check.</param>
    /// <returns>True if the service is failed, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static bool IsFailed(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.State == ServiceState.Failed || service.SubState == ServiceSubState.Failed;
    }

    /// <summary>
    /// Determines if the service is enabled to start on boot.
    /// </summary>
    /// <param name="service">The service to check.</param>
    /// <returns>True if the service is enabled to start on boot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static bool IsEnabled(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.AutoStart && service.LoadState == ServiceLoadState.Loaded;
    }

    /// <summary>
    /// Gets the formatted uptime as a human-readable string.
    /// </summary>
    /// <param name="service">The service to format.</param>
    /// <returns>Formatted uptime string (e.g., "2.12:30:45" for 2 days, 12 hours, 30 minutes, 45 seconds).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static string GetFormattedUptime(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return service.UptimeSeconds <= 0
            ? "0s"
            : TimeSpan.FromSeconds(service.UptimeSeconds).ToString("d\\.hh\\:mm\\:ss");
    }

    /// <summary>
    /// Determines if the service is in a restartable state.
    /// </summary>
    /// <param name="service">The service to check.</param>
    /// <returns>True if the service can be restarted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static bool CanRestart(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);
        return service.IsActive() && service.Restart && service.RestartPolicy != RestartPolicy.No;
    }

    /// <summary>
    /// Gets a status summary string for the service.
    /// </summary>
    /// <param name="service">The service to summarize.</param>
    /// <returns>Human-readable status summary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is null.</exception>
    public static string GetStatusSummary(this ServiceInfo service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (service.IsFailed())
        {
            return $"FAILED ({service.SubState}) - {service.Result}";
        }

        if (!service.IsActive())
        {
            return $"INACTIVE ({service.SubState}) - Last result: {service.Result}";
        }

        var status = service.IsEnabled() ? "ENABLED" : "DISABLED";
        var uptime = service.GetFormattedUptime();
        var cpu = service.CpuUsagePercent > 0 ? $"{service.CpuUsagePercent:F1}% CPU" : string.Empty;
        var memory = service.MemoryUsageMb > 0 ? $"{service.MemoryUsageMb}MB memory" : string.Empty;

        return string.Join(", ", new[] { status, uptime, cpu, memory }.Where(p => !string.IsNullOrEmpty(p)));
    }
}