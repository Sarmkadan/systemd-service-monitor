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
/// REST API controller for systemd service management and monitoring.
/// Provides endpoints for querying, controlling, and monitoring systemd services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ServicesController(
    IServiceMonitorService monitorService,
    IServiceControlService controlService,
    IServiceLogService logService,
    ILogger<ServicesController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves a list of all systemd services with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceInfo>>>> GetAllServices(
        [FromQuery] string? state = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var services = await monitorService.GetAllServicesAsync();

            if (!string.IsNullOrEmpty(state))
            {
                services = services.Where(s => s.State.ToString().Equals(state, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalCount = services.Count;
            var paginatedServices = services
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginatedResponse<ServiceInfo>
            {
                Data = paginatedServices,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Success = true,
                Message = $"Retrieved {paginatedServices.Count} services"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving services");
            return StatusCode(500, new ApiResponse<List<ServiceInfo>>
            {
                Success = false,
                Message = "Failed to retrieve services",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific service by name.
    /// </summary>
    [HttpGet("{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ServiceInfo>>> GetServiceDetails(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<ServiceInfo>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            var service = await monitorService.GetServiceAsync(serviceName);

            if (service is null)
            {
                return NotFound(new ApiResponse<ServiceInfo>
                {
                    Success = false,
                    Message = $"Service '{serviceName}' not found"
                });
            }

            return Ok(new ApiResponse<ServiceInfo>
            {
                Data = service,
                Success = true,
                Message = $"Retrieved details for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving service details for {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<ServiceInfo>
            {
                Success = false,
                Message = "Failed to retrieve service details",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Starts a stopped systemd service.
    /// Requires appropriate systemd permissions (typically root or systemd-related group membership).
    /// </summary>
    [HttpPost("{serviceName}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> StartService(string serviceName)
    {
        try
        {
            var result = await controlService.StartServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' started successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' started successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to start service '{serviceName}'"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized attempt to start service {ServiceName}", serviceName);
            return StatusCode(403, new ApiResponse<bool>
            {
                Success = false,
                Message = "Insufficient permissions to start service",
                ErrorDetails = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to start service",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Stops a running systemd service.
    /// </summary>
    [HttpPost("{serviceName}/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> StopService(string serviceName)
    {
        try
        {
            var result = await controlService.StopServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' stopped successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' stopped successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to stop service '{serviceName}'"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized attempt to stop service {ServiceName}", serviceName);
            return StatusCode(403, new ApiResponse<bool>
            {
                Success = false,
                Message = "Insufficient permissions to stop service",
                ErrorDetails = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to stop service",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Restarts a systemd service by stopping and then starting it.
    /// </summary>
    [HttpPost("{serviceName}/restart")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> RestartService(string serviceName)
    {
        try
        {
            var result = await controlService.RestartServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' restarted successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' restarted successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to restart service '{serviceName}'"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized attempt to restart service {ServiceName}", serviceName);
            return StatusCode(403, new ApiResponse<bool>
            {
                Success = false,
                Message = "Insufficient permissions to restart service",
                ErrorDetails = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error restarting service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to restart service",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Reloads service configuration without restarting (if supported by the service).
    /// </summary>
    [HttpPost("{serviceName}/reload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> ReloadService(string serviceName)
    {
        try
        {
            var result = await controlService.ReloadServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' reloaded successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' reloaded successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to reload service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reloading service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to reload service",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Enables a service to start automatically on boot.
    /// </summary>
    [HttpPost("{serviceName}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> EnableService(string serviceName)
    {
        try
        {
            var result = await controlService.EnableServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' enabled successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' enabled successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to enable service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enabling service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to enable service",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Disables a service from auto-starting on boot.
    /// </summary>
    [HttpPost("{serviceName}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DisableService(string serviceName)
    {
        try
        {
            var result = await controlService.DisableServiceAsync(serviceName);

            if (result)
            {
                logger.LogInformation("Service '{ServiceName}' disabled successfully", serviceName);
                return Ok(new ApiResponse<bool>
                {
                    Data = true,
                    Success = true,
                    Message = $"Service '{serviceName}' disabled successfully"
                });
            }

            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Failed to disable service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disabling service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to disable service",
                ErrorDetails = ex.Message
            });
        }
    }
}
