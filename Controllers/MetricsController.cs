#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// REST API controller for resource metrics and system monitoring.
/// Provides endpoints for CPU, memory, disk usage, and other system metrics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricsController(
    IResourceMonitorService resourceService,
    ILogger<MetricsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves system-wide resource metrics (CPU, memory, disk usage).
    /// </summary>
    [HttpGet("system")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SystemResource>>> GetSystemMetrics()
    {
        try
        {
            var metrics = await resourceService.GetSystemResourcesAsync();

            return Ok(new ApiResponse<SystemResource>
            {
                Data = metrics,
                Success = true,
                Message = "Retrieved system resource metrics"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system metrics");
            return StatusCode(500, new ApiResponse<SystemResource>
            {
                Success = false,
                Message = "Failed to retrieve system metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves resource metrics for a specific service.
    /// </summary>
    [HttpGet("service/{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ServiceMetric>>> GetServiceMetrics(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<ServiceMetric>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            var metrics = await resourceService.GetServiceMetricsAsync(serviceName);

            if (metrics is null)
            {
                return NotFound(new ApiResponse<ServiceMetric>
                {
                    Success = false,
                    Message = $"Metrics not available for service '{serviceName}'"
                });
            }

            return Ok(new ApiResponse<ServiceMetric>
            {
                Data = metrics,
                Success = true,
                Message = $"Retrieved metrics for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving metrics for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<ServiceMetric>
            {
                Success = false,
                Message = "Failed to retrieve service metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves metrics for all monitored services.
    /// Includes CPU, memory, and other resource usage statistics.
    /// </summary>
    [HttpGet("services")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetAllServiceMetrics(
        [FromQuery] string? sortBy = "cpu",
        [FromQuery] bool descending = true)
    {
        try
        {
            var metrics = await resourceService.GetAllServiceMetricsAsync();

            // Apply sorting based on query parameter
            metrics = (sortBy?.ToLower()) switch
            {
                "memory" => descending
                    ? metrics.OrderByDescending(m => m.MemoryUsageMb).ToList()
                    : metrics.OrderBy(m => m.MemoryUsageMb).ToList(),
                "cpu" => descending
                    ? metrics.OrderByDescending(m => m.CpuPercentage).ToList()
                    : metrics.OrderBy(m => m.CpuPercentage).ToList(),
                "name" => descending
                    ? metrics.OrderByDescending(m => m.ServiceName).ToList()
                    : metrics.OrderBy(m => m.ServiceName).ToList(),
                _ => descending
                    ? metrics.OrderByDescending(m => m.CpuPercentage).ToList()
                    : metrics.OrderBy(m => m.CpuPercentage).ToList()
            };

            return Ok(new ApiResponse<List<ServiceMetric>>
            {
                Data = metrics,
                Success = true,
                Message = $"Retrieved metrics for {metrics.Count} services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all service metrics");
            return StatusCode(500, new ApiResponse<List<ServiceMetric>>
            {
                Success = false,
                Message = "Failed to retrieve service metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves memory usage metrics for all services with optional filtering.
    /// Useful for identifying memory-intensive services.
    /// </summary>
    [HttpGet("memory/top")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetTopMemoryConsumers(
        [FromQuery] int limit = 10)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 100);
            var metrics = await resourceService.GetAllServiceMetricsAsync();

            var topMemory = metrics
                .OrderByDescending(m => m.MemoryUsageMb)
                .Take(limit)
                .ToList();

            return Ok(new ApiResponse<List<ServiceMetric>>
            {
                Data = topMemory,
                Success = true,
                Message = $"Retrieved top {topMemory.Count} memory-consuming services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving top memory consumers");
            return StatusCode(500, new ApiResponse<List<ServiceMetric>>
            {
                Success = false,
                Message = "Failed to retrieve memory metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves CPU usage metrics for all services with optional filtering.
    /// Useful for identifying CPU-intensive services.
    /// </summary>
    [HttpGet("cpu/top")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceMetric>>>> GetTopCpuConsumers(
        [FromQuery] int limit = 10)
    {
        try
        {
            limit = Math.Clamp(limit, 1, 100);
            var metrics = await resourceService.GetAllServiceMetricsAsync();

            var topCpu = metrics
                .OrderByDescending(m => m.CpuPercentage)
                .Take(limit)
                .ToList();

            return Ok(new ApiResponse<List<ServiceMetric>>
            {
                Data = topCpu,
                Success = true,
                Message = $"Retrieved top {topCpu.Count} CPU-consuming services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving top CPU consumers");
            return StatusCode(500, new ApiResponse<List<ServiceMetric>>
            {
                Success = false,
                Message = "Failed to retrieve CPU metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves disk I/O metrics for a specific service.
    /// </summary>
    [HttpGet("disk/{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetServiceDiskMetrics(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            var metrics = await resourceService.GetServiceMetricsAsync(serviceName);

            if (metrics is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Disk metrics not available for service '{serviceName}'"
                });
            }

            var diskMetrics = new
            {
                ServiceName = serviceName,
                DiskReadBytesPerSec = metrics.DiskReadBytesPerSec,
                DiskWriteBytesPerSec = metrics.DiskWriteBytesPerSec,
                Timestamp = metrics.Timestamp
            };

            return Ok(new ApiResponse<object>
            {
                Data = diskMetrics,
                Success = true,
                Message = $"Retrieved disk metrics for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving disk metrics for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve disk metrics",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves network I/O metrics for a specific service.
    /// </summary>
    [HttpGet("network/{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetServiceNetworkMetrics(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            var metrics = await resourceService.GetServiceMetricsAsync(serviceName);

            if (metrics is null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Network metrics not available for service '{serviceName}'"
                });
            }

            var networkMetrics = new
            {
                ServiceName = serviceName,
                NetworkBytesIn = metrics.NetworkBytesIn,
                NetworkBytesOut = metrics.NetworkBytesOut,
                Timestamp = metrics.Timestamp
            };

            return Ok(new ApiResponse<object>
            {
                Data = networkMetrics,
                Success = true,
                Message = $"Retrieved network metrics for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving network metrics for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to retrieve network metrics",
                ErrorDetails = ex.Message
            });
        }
    }
}
