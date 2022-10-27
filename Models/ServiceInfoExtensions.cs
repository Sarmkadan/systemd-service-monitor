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
    /// <param name="service">The service to check</param>
    /// <returns>True if the service is active, false otherwise</returns>
    public static bool IsActive(this ServiceInfo service)
    {
        return service.State == ServiceState.Active || service.State == ServiceState.Activating;
    }

    /// <summary>
    /// Determines if the service is currently failed or in a failure state.
    /// </summary>
    /// <param name="service">The service to check</param>
    /// <returns>True if the service is failed, false otherwise</returns>
    public static bool IsFailed(this ServiceInfo service)
    {
        return service.State == ServiceState.Failed || service.SubState == ServiceSubState.Failed;
    }

    /// <summary>
    /// Determines if the service is enabled to start on boot.
    /// </summary>
    /// <param name="service">The service to check</param>
    /// <returns>True if the service is enabled to start on boot</returns>
    public static bool IsEnabled(this ServiceInfo service)
    {
        return service.AutoStart && service.LoadState == ServiceLoadState.Loaded;
    }

    /// <summary>
    /// Gets the formatted uptime as a human-readable string.
    /// </summary>
    /// <param name="service">The service to format</param>
    /// <returns>Formatted uptime string (e.g., "2h 30m 15s")</returns>
    public static string GetFormattedUptime(this ServiceInfo service)
    {
        if (service.UptimeSeconds <= 0)
        {
            return "0s";
        }

        var uptime = TimeSpan.FromSeconds(service.UptimeSeconds);
        return uptime.ToString("d\\.hh\\:mm\\:ss");
    }

    /// <summary>
    /// Determines if the service is in a restartable state.
    /// </summary>
    /// <param name="service">The service to check</param>
    /// <returns>True if the service can be restarted</returns>
    public static bool CanRestart(this ServiceInfo service)
    {
        return service.IsActive() && service.Restart && service.RestartPolicy != RestartPolicy.No;
    }

    /// <summary>
    /// Gets a status summary string for the service.
    /// </summary>
    /// <param name="service">The service to summarize</param>
    /// <returns>Human-readable status summary</returns>
    public static string GetStatusSummary(this ServiceInfo service)
    {
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
        var cpu = service.CpuUsagePercent > 0 ? $"{service.CpuUsagePercent:F1}% CPU" : "";
        var memory = service.MemoryUsageMb > 0 ? $"{service.MemoryUsageMb}MB memory" : "";

        var parts = new List<string> { status, uptime };
        if (!string.IsNullOrEmpty(cpu)) parts.Add(cpu);
        if (!string.IsNullOrEmpty(memory)) parts.Add(memory);

        return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }
}