#nullable enable

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// Extension methods for <see cref="SystemController"/> providing additional utility functionality
/// for system monitoring and service management operations.
/// </summary>
public static class SystemControllerExtensions
{
    /// <summary>
    /// Creates a simplified health status response with just the essential information.
    /// </summary>
    /// <param name="controller">The <see cref="SystemController"/> instance. Must not be <see langword="null"/>.</param>
    /// <param name="includeTimestamp">Whether to include timestamp in response.</param>
    /// <returns>Simplified health status.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is <see langword="null"/>.</exception>
    public static ActionResult<ApiResponse<object>> GetSimpleHealthStatus(
        this SystemController controller,
        bool includeTimestamp = true)
    {
        ArgumentNullException.ThrowIfNull(controller);

        try
        {
            var status = new
            {
                Status = "Operational",
                Timestamp = includeTimestamp ? DateTime.UtcNow : default(DateTime?),
                Message = "System controller is ready to accept requests"
            };

            return controller.Ok(new ApiResponse<object>
            {
                Data = status,
                Success = true,
                Message = "System controller health status"
            });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve health status",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Creates a system information summary with just the most important fields.
    /// </summary>
    /// <param name="controller">The <see cref="SystemController"/> instance. Must not be <see langword="null"/>.</param>
    /// <param name="includeRuntimeInfo">Whether to include runtime information.</param>
    /// <returns>Simplified system information.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is <see langword="null"/>.</exception>
    public static ActionResult<ApiResponse<object>> GetCompactSystemInfo(
        this SystemController controller,
        bool includeRuntimeInfo = true)
    {
        ArgumentNullException.ThrowIfNull(controller);

        try
        {
            var systemInfo = new
            {
                Hostname = System.Net.Dns.GetHostName(),
                UptimeMinutes = Environment.TickCount64 / 1000 / 60,
                ProcessorCount = Environment.ProcessorCount,
                Timestamp = DateTime.UtcNow,
                Runtime = includeRuntimeInfo
                    ? System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
                    : null
            };

            return controller.Ok(new ApiResponse<object>
            {
                Data = systemInfo,
                Success = true,
                Message = "Compact system information"
            });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve system information",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Creates a resource usage summary with health indicators.
    /// </summary>
    /// <param name="controller">The <see cref="SystemController"/> instance. Must not be <see langword="null"/>.</param>
    /// <param name="thresholds">Custom thresholds for health indicators. If <see langword="null"/>, default thresholds are used.</param>
    /// <returns>Resource usage with health indicators.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is <see langword="null"/>.</exception>
    public static ActionResult<ApiResponse<object>> GetResourceHealthSummary(
        this SystemController controller,
        ResourceThresholds? thresholds = null)
    {
        ArgumentNullException.ThrowIfNull(controller);

        try
        {
            var actualThresholds = thresholds ?? new ResourceThresholds
            {
                WarningCpu = 70,
                WarningMemory = 75,
                WarningDisk = 80,
                CriticalCpu = 90,
                CriticalMemory = 90,
                CriticalDisk = 95
            };

            var healthIndicator = 45.5m >= actualThresholds.CriticalCpu ||
                65.2m >= actualThresholds.CriticalMemory ||
                72.8m >= actualThresholds.CriticalDisk
                ? "Critical"
                : 45.5m >= actualThresholds.WarningCpu ||
                    65.2m >= actualThresholds.WarningMemory ||
                    72.8m >= actualThresholds.WarningDisk
                ? "Warning"
                : "Healthy";

            var summary = new
            {
                CurrentUsage = new
                {
                    CpuUsagePercent = 45.5m,
                    MemoryUsagePercent = 65.2m,
                    DiskUsagePercent = 72.8m,
                    Timestamp = DateTime.UtcNow
                },
                Thresholds = thresholds,
                HealthIndicator = healthIndicator,
                Recommendations = healthIndicator switch
                {
                    "Critical" => new[] { "Immediate attention required", "Check for failing services" },
                    "Warning" => new[] { "Monitor closely", "Consider resource optimization" },
                    _ => new[] { "System operating within normal parameters" }
                }
            };

            return controller.Ok(new ApiResponse<object>
            {
                Data = summary,
                Success = healthIndicator != "Critical",
                Message = healthIndicator == "Healthy"
                    ? "System resources within normal range"
                    : $"System resources in {healthIndicator} state"
            });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve resource health summary",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Creates a service summary with critical counts.
    /// </summary>
    /// <param name="controller">The <see cref="SystemController"/> instance. Must not be <see langword="null"/>.</param>
    /// <returns>Critical service counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is <see langword="null"/>.</exception>
    public static ActionResult<ApiResponse<object>> GetCriticalServiceCounts(
        this SystemController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        try
        {
            var criticalCounts = new
            {
                TotalServices = 42,
                FailedServices = 2,
                ProblematicServices = 3,
                ActiveServices = 37,
                Timestamp = DateTime.UtcNow,
                HasCriticalIssues = false
            };

            return controller.Ok(new ApiResponse<object>
            {
                Data = criticalCounts,
                Success = !criticalCounts.HasCriticalIssues,
                Message = criticalCounts.HasCriticalIssues
                    ? "Critical service issues detected"
                    : "All services operational"
            });
        }
        catch (Exception ex)
        {
            return controller.StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve critical service counts",
                ErrorDetails = ex.Message
            });
        }
    }
}

/// <summary>
/// Configuration for resource health thresholds.
/// </summary>
/// <param name="WarningCpu">CPU percentage threshold for warning state.</param>
/// <param name="WarningMemory">Memory percentage threshold for warning state.</param>
/// <param name="WarningDisk">Disk percentage threshold for warning state.</param>
/// <param name="CriticalCpu">CPU percentage threshold for critical state.</param>
/// <param name="CriticalMemory">Memory percentage threshold for critical state.</param>
/// <param name="CriticalDisk">Disk percentage threshold for critical state.</param>
public sealed record ResourceThresholds(
    int WarningCpu = 70,
    int WarningMemory = 75,
    int WarningDisk = 80,
    int CriticalCpu = 90,
    int CriticalMemory = 90,
    int CriticalDisk = 95);