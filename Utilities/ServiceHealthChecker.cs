#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Utility class for evaluating service health status.
/// Implements health assessment logic based on service state, restart count, and other metrics.
/// </summary>
public static class ServiceHealthChecker
{
    // Constants for service states
    private const string StateFailed = "Failed";
    private const string StateActivating = "Activating";
    private const string StateDeactivating = "Deactivating";
    private const string StateActive = "Active";
    private const string StateInactive = "Inactive";

    // Constants for restart policy
    private const string RestartPolicyNo = "No";

    // Constants for service result
    private const string ResultSuccess = "success";

    // Constants for restart count thresholds
    private const int CriticalRestartThreshold = 10;
    private const int WarningRestartThreshold = 5;
    private const int HealthyRestartThreshold = 2;

    // Constants for reliability calculation
    private const int ReliabilityDeductionPerRestart = 5;
    private const int MaxReliabilityDeductionFromRestarts = 50;
    private const int ReliabilityDeductionForFailedState = 50;
    private const int ReliabilityDeductionForInactiveAutoStart = 25;

    /// <summary>
    /// Evaluates the health of a service based on its current state and history.
    /// </summary>
    public static ServiceHealthStatus GetHealthStatus(ServiceInfo service)
    {
        if (service is null)
            return ServiceHealthStatus.Unknown;

        // Check for critical issues first
        if (service.State.ToString() == StateFailed)
            return ServiceHealthStatus.Critical;

        // Check if service is restarting
        if (service.State.ToString() == StateActivating || service.State.ToString() == StateDeactivating)
            return ServiceHealthStatus.Warning;

        // Check for frequent restarts (unstable service)
        if (service.RestartCount > CriticalRestartThreshold)
            return ServiceHealthStatus.Critical;

        if (service.RestartCount > WarningRestartThreshold)
            return ServiceHealthStatus.Warning;

        // Check if service is active and stable
        if (service.State.ToString() == StateActive && service.RestartCount <= HealthyRestartThreshold)
            return ServiceHealthStatus.Healthy;

        // Service is active but has restart issues
        if (service.State.ToString() == StateActive)
            return ServiceHealthStatus.Warning;

        // Service is inactive (might be disabled)
        if (service.State.ToString() == StateInactive)
        {
            return service.AutoStart ? ServiceHealthStatus.Warning : ServiceHealthStatus.Healthy;
        }

        return ServiceHealthStatus.Unknown;
    }

    /// <summary>
    /// Generates a human-readable health summary for a service.
    /// </summary>
    public static string GetHealthSummary(ServiceInfo service)
    {
        if (service is null)
            return "Service information unavailable";

        var status = GetHealthStatus(service);
        var icon = status switch
        {
            ServiceHealthStatus.Healthy => "✓",
            ServiceHealthStatus.Warning => "⚠",
            ServiceHealthStatus.Critical => "✗",
            _ => "?"
        };

        var statusText = status switch
        {
            ServiceHealthStatus.Healthy => "Healthy",
            ServiceHealthStatus.Warning => "Warning",
            ServiceHealthStatus.Critical => "Critical",
            _ => "Unknown"
        };

        var details = new List<string> { $"{service.UnitName}: {icon} {statusText}" };

        if (service.State.ToString() != "Active")
            details.Add($"State: {service.State}");

        if (service.RestartCount > 0)
            details.Add($"Restarts: {service.RestartCount}");

        if (service.UptimeSeconds > 0)
            details.Add($"Uptime: {FormatUptime(service.UptimeSeconds)}");

        return string.Join(" | ", details);
    }

    /// <summary>
    /// Checks if a service is experiencing issues that need attention.
    /// </summary>
    public static bool IsProblematic(ServiceInfo service)
    {
        var status = GetHealthStatus(service);
        return status == ServiceHealthStatus.Critical || status == ServiceHealthStatus.Warning;
    }

    /// <summary>
    /// Gets recommended actions for a problematic service.
    /// </summary>
    public static List<string> GetRecommendedActions(ServiceInfo service)
    {
        var actions = new List<string>();

        if (service.State.ToString() == StateFailed)
        {
            actions.Add("Check service logs for error details");
            actions.Add("Restart the service and monitor logs");
            actions.Add("Verify service dependencies are running");
        }

        if (service.RestartCount > WarningRestartThreshold)
        {
            actions.Add("Review recent log entries for crash patterns");
            actions.Add("Check system resources (CPU, memory, disk)");
            actions.Add("Verify service configuration files");
        }

        if (service.RestartPolicy.ToString() == RestartPolicyNo && service.State.ToString() != StateActive)
        {
            actions.Add("Service has restart policy disabled - enable to auto-recover");
        }

        if (string.IsNullOrEmpty(service.Result) || service.Result == ResultSuccess)
        {
            if (service.State.ToString() == StateActive)
            {
                actions.Add("Service is running normally - no action needed");
            }
        }

        return actions;
    }

    /// <summary>
    /// Formats uptime as a human-readable string.
    /// </summary>
    public static string FormatUptime(long uptimeSeconds)
    {
        if (uptimeSeconds <= 0)
            return "Not running";

        var timespan = TimeSpan.FromSeconds(uptimeSeconds);

        if (timespan.TotalDays >= 1)
            return $"{(int)timespan.TotalDays}d {timespan.Hours}h";
        if (timespan.TotalHours >= 1)
            return $"{(int)timespan.TotalHours}h {timespan.Minutes}m";
        if (timespan.TotalMinutes >= 1)
            return $"{(int)timespan.TotalMinutes}m {timespan.Seconds}s";

        return $"{timespan.Seconds}s";
    }

    /// <summary>
    /// Calculates service reliability percentage based on restart history.
    /// </summary>
    public static double CalculateReliability(ServiceInfo service)
    {
        if (service is null)
            return 0;

        // Base reliability on restart count and status
        var baseReliability = 100.0;

        // Deduct for each restart
        baseReliability -= Math.Min(service.RestartCount * ReliabilityDeductionPerRestart, MaxReliabilityDeductionFromRestarts);

        // Deduct for failed state
        if (service.State.ToString() == StateFailed)
            baseReliability -= ReliabilityDeductionForFailedState;

        // Deduct for inactive state (if it should be active)
        if (service.AutoStart && service.State.ToString() == StateInactive)
            baseReliability -= ReliabilityDeductionForInactiveAutoStart;

        return Math.Max(0, baseReliability);
    }
}

/// <summary>
/// Health status levels for services.
/// </summary>
public enum ServiceHealthStatus
{
    Healthy,
    Warning,
    Critical,
    Unknown
}
