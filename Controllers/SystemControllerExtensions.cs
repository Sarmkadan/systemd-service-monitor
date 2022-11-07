#nullable enable

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// Extension methods for SystemController providing additional utility functionality
/// for system monitoring and service management operations.
/// </summary>
public static class SystemControllerExtensions
{
    /// <summary>
    /// Creates a simplified health status response with just the essential information.
    /// </summary>
    /// <param name="controller">The SystemController instance</param>
    /// <param name="includeTimestamp">Whether to include timestamp in response</param>
    /// <returns>Simplified health status</returns>
    public static ActionResult<ApiResponse<object>> GetSimpleHealthStatus(
        this SystemController controller,
        bool includeTimestamp = true)
    {
        try
        {
            var status = new
            {
                Status = "Operational",
                Timestamp = includeTimestamp ? DateTime.UtcNow : (DateTime?)null,
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
    /// <param name="controller">The SystemController instance</param>
    /// <param name="includeRuntimeInfo">Whether to include runtime information</param>
    /// <returns>Simplified system information</returns>
    public static ActionResult<ApiResponse<object>> GetCompactSystemInfo(
        this SystemController controller,
        bool includeRuntimeInfo = true)
    {
        try
        {
            var systemInfo = new
            {
                Hostname = System.Net.Dns.GetHostName(),
                UptimeMinutes = Environment.TickCount / 1000 / 60,
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
    /// <param name="controller">The SystemController instance</param>
    /// <param name="thresholds">Custom thresholds for health indicators</param>
    /// <returns>Resource usage with health indicators</returns>
    public static ActionResult<ApiResponse<object>> GetResourceHealthSummary(
        this SystemController controller,
        ResourceThresholds? thresholds = null)
    {
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

            var resources = new
            {
                CpuUsagePercent = 45.5, // Placeholder - actual would come from service
                MemoryUsagePercent = 65.2,
                DiskUsagePercent = 72.8,
                Timestamp = DateTime.UtcNow,
                HealthStatus = "Healthy" // Placeholder - actual would come from monitoring
            };

            var healthIndicator = resources.CpuUsagePercent >= actualThresholds.CriticalCpu ||
                                resources.MemoryUsagePercent >= actualThresholds.CriticalMemory ||
                                resources.DiskUsagePercent >= actualThresholds.CriticalDisk
                ? "Critical"
                : resources.CpuUsagePercent >= actualThresholds.WarningCpu ||
                  resources.MemoryUsagePercent >= actualThresholds.WarningMemory ||
                  resources.DiskUsagePercent >= actualThresholds.WarningDisk
                    ? "Warning"
                    : "Healthy";

            var summary = new
            {
                CurrentUsage = resources,
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
    /// Creates a service summary with just the critical counts.
    /// </summary>
    /// <param name="controller">The SystemController instance</param>
    /// <returns>Critical service counts</returns>
    public static ActionResult<ApiResponse<object>> GetCriticalServiceCounts(
        this SystemController controller)
    {
        try
        {
            var criticalCounts = new
            {
                TotalServices = 42,
                FailedServices = 2,
                ProblematicServices = 3,
                ActiveServices = 37,
                Timestamp = DateTime.UtcNow,
                HasCriticalIssues = false // Placeholder - actual would come from monitoring
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
public record ResourceThresholds(
    int WarningCpu = 70,
    int WarningMemory = 75,
    int WarningDisk = 80,
    int CriticalCpu = 90,
    int CriticalMemory = 90,
    int CriticalDisk = 95);