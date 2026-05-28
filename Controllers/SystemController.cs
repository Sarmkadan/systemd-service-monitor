#nullable enable

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// REST API controller for system-wide operations and health checks.
/// Provides endpoints for system status, health checks, and general diagnostics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SystemController(
    IServiceMonitorService monitorService,
    IResourceMonitorService resourceService,
    ILogger<SystemController> logger) : ControllerBase
{
    /// <summary>
    /// Performs a health check on the systemd connection and monitoring infrastructure.
    /// Returns detailed status information about the connection state.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ApiResponse<object>>> HealthCheck()
    {
        try
        {
            // Try to retrieve basic service info to verify D-Bus connection
            var services = await monitorService.GetAllServicesAsync();
            var systemResources = await resourceService.GetSystemResourcesAsync();

            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    MonitoredCount = services.Count(),
                    ActiveCount = services.Count(s => s.State.ToString() == "Active"),
                    FailedCount = services.Count(s => s.State.ToString() == "Failed")
                },
                System = new
                {
                    CpuPercentage = systemResources.CpuUsagePercent,
                    MemoryPercentage = systemResources.MemoryUsagePercent,
                    DiskPercentage = systemResources.DiskUsagePercent
                }
            };

            return Ok(new ApiResponse<object>
            {
                Data = healthStatus,
                Success = true,
                Message = "System is healthy"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return StatusCode(503, new ApiResponse<object>
            {
                Success = false,
                Message = "Service unavailable",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves detailed system information including OS details, uptime, and load average.
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ApiResponse<object>> GetSystemInfo()
    {
        try
        {
            var systemInfo = new
            {
                Hostname = System.Net.Dns.GetHostName(),
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                ProcessorCount = Environment.ProcessorCount,
                ManagedMemory = GC.GetTotalMemory(false),
                Uptime = Environment.TickCount / 1000 / 60, // in minutes
                RuntimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                Timestamp = DateTime.UtcNow
            };

            return Ok(new ApiResponse<object>
            {
                Data = systemInfo,
                Success = true,
                Message = "Retrieved system information"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system info");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve system information",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves current system resource utilization (CPU, memory, disk).
    /// </summary>
    [HttpGet("resources")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SystemResource>>> GetSystemResources()
    {
        try
        {
            var resources = await resourceService.GetSystemResourcesAsync();

            return Ok(new ApiResponse<SystemResource>
            {
                Data = resources,
                Success = true,
                Message = "Retrieved system resource information"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system resources");
            return StatusCode(500, new ApiResponse<SystemResource>
            {
                Success = false,
                Message = "Failed to retrieve system resources",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves a summary of the system state including service counts and overall health.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetSystemSummary()
    {
        try
        {
            var services = await monitorService.GetAllServicesAsync();
            var resources = await resourceService.GetSystemResourcesAsync();

            var summary = new
            {
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    Total = services.Count(),
                    Active = services.Count(s => s.State.ToString() == "Active"),
                    Inactive = services.Count(s => s.State.ToString() == "Inactive"),
                    Failed = services.Count(s => s.State.ToString() == "Failed"),
                    Restarting = services.Count(s => s.State.ToString() == "Activating" || s.State.ToString() == "Deactivating")
                },
                Resources = new
                {
                    CpuPercent = Math.Round(resources.CpuUsagePercent, 2),
                    MemoryPercent = Math.Round(resources.MemoryUsagePercent, 2),
                    DiskPercent = Math.Round(resources.DiskUsagePercent, 2),
                    MemoryAvailableMb = resources.AvailableMemoryMb,
                    DiskAvailableGb = resources.AvailableDiskGb
                }
            };

            return Ok(new ApiResponse<object>
            {
                Data = summary,
                Success = true,
                Message = "Retrieved system summary"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system summary");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve system summary",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves statistics about failed and problematic services.
    /// </summary>
    [HttpGet("failed-services")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceInfo>>>> GetFailedServices()
    {
        try
        {
            var services = await monitorService.GetAllServicesAsync();
            var failedServices = services
                .Where(s => s.State.ToString() == "Failed" || s.RestartCount > 5)
                .OrderByDescending(s => s.LastStopTime)
                .ToList();

            return Ok(new ApiResponse<List<ServiceInfo>>
            {
                Data = failedServices,
                Success = true,
                Message = $"Retrieved {failedServices.Count} failed services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving failed services");
            return StatusCode(500, new ApiResponse<List<ServiceInfo>>
            {
                Success = false,
                Message = "Failed to retrieve failed services",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves a list of services with recent restarts (problematic services).
    /// </summary>
    [HttpGet("problematic-services")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceInfo>>>> GetProblematicServices(
        [FromQuery] int minRestarts = 3)
    {
        try
        {
            var services = await monitorService.GetAllServicesAsync();
            var minRestartsValue = Math.Max(minRestarts, 1);

            var problematicServices = services
                .Where(s => s.RestartCount >= minRestartsValue)
                .OrderByDescending(s => s.RestartCount)
                .ToList();

            return Ok(new ApiResponse<List<ServiceInfo>>
            {
                Data = problematicServices,
                Success = true,
                Message = $"Retrieved {problematicServices.Count} problematic services with {minRestartsValue}+ restarts"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving problematic services");
            return StatusCode(500, new ApiResponse<List<ServiceInfo>>
            {
                Success = false,
                Message = "Failed to retrieve problematic services",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Returns application version and build information.
    /// </summary>
    [HttpGet("version")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> GetVersion()
    {
        var version = new
        {
            Application = "systemd-service-monitor",
            Version = "1.0.0",
            BuildDate = "2026-01-01",
            ApiVersion = "v1",
            FrameworkVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Timestamp = DateTime.UtcNow
        };

        return Ok(new ApiResponse<object>
        {
            Data = version,
            Success = true,
            Message = "Retrieved version information"
        });
    }

    /// <summary>
    /// Retrieves diagnostic information useful for troubleshooting.
    /// </summary>
    [HttpGet("diagnostics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetDiagnostics()
    {
        try
        {
            var services = await monitorService.GetAllServicesAsync();
            var resources = await resourceService.GetSystemResourcesAsync();

            var diagnostics = new
            {
                Timestamp = DateTime.UtcNow,
                ConnectionStatus = "Connected",
                ServiceMetrics = new
                {
                    TotalServices = services.Count(),
                    ActiveServices = services.Count(s => s.State.ToString() == "Active"),
                    FailedServices = services.Count(s => s.State.ToString() == "Failed"),
                    AverageRestartCount = Math.Round(services.Average(s => s.RestartCount), 2)
                },
                SystemMetrics = new
                {
                    CpuPercentage = Math.Round(resources.CpuUsagePercent, 2),
                    MemoryPercentage = Math.Round(resources.MemoryUsagePercent, 2),
                    DiskPercentage = Math.Round(resources.DiskUsagePercent, 2),
                    CpuCoreCount = Environment.ProcessorCount
                },
                Process = new
                {
                    WorkingSetMb = GC.GetTotalMemory(false) / (1024 * 1024),
                    ThreadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count
                }
            };

            return Ok(new ApiResponse<object>
            {
                Data = diagnostics,
                Success = true,
                Message = "Retrieved diagnostic information"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving diagnostics");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve diagnostics",
                ErrorDetails = ex.Message
            });
        }
    }
}
