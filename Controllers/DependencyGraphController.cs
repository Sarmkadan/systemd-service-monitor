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

[ApiController]
[Route("api/dependency-graph")]
public class DependencyGraphController(
    IServiceDependencyGraphService dependencyGraphService,
    ILogger<DependencyGraphController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ServiceDependencyGraph>>> GetGraph(CancellationToken ct)
    {
        try
        {
            var graph = await dependencyGraphService.BuildGraphAsync(ct);
            return Ok(new ApiResponse<ServiceDependencyGraph>
            {
                Data = graph,
                Success = true,
                Message = $"Retrieved dependency graph with {graph.TotalNodes} nodes"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error building dependency graph");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<ServiceDependencyGraph>("Failed to build dependency graph", ex));
        }
    }

    [HttpGet("roots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetRoots(CancellationToken ct)
    {
        try
        {
            var roots = (await dependencyGraphService.GetRootServicesAsync(ct)).ToList();
            return Ok(new ApiResponse<List<DependencyNode>>
            {
                Data = roots,
                Success = true,
                Message = $"Retrieved {roots.Count} root services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving root services");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<List<DependencyNode>>("Failed to retrieve root services", ex));
        }
    }

    [HttpGet("leaves")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetLeaves(CancellationToken ct)
    {
        try
        {
            var leaves = (await dependencyGraphService.GetLeafServicesAsync(ct)).ToList();
            return Ok(new ApiResponse<List<DependencyNode>>
            {
                Data = leaves,
                Success = true,
                Message = $"Retrieved {leaves.Count} leaf services"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving leaf services");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<List<DependencyNode>>("Failed to retrieve leaf services", ex));
        }
    }

    [HttpGet("path")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetPath([FromQuery] string from, [FromQuery] string to, CancellationToken ct)
    {
        try
        {
            var path = (await dependencyGraphService.GetDependencyChainAsync(from, to, ct)).ToList();
            if (path.Count == 0)
            {
                return NotFound(new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = $"No dependency path found from '{from}' to '{to}'"
                });
            }

            return Ok(new ApiResponse<List<string>>
            {
                Data = path,
                Success = true,
                Message = $"Retrieved dependency path from '{from}' to '{to}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dependency path from {FromService} to {ToService}", from, to);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<List<string>>("Failed to retrieve dependency path", ex));
        }
    }

    [HttpGet("{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ServiceDependencyGraph>>> GetSubgraph(string serviceName, [FromQuery] int depth = 3, CancellationToken ct = default)
    {
        try
        {
            var graph = await dependencyGraphService.BuildGraphForServiceAsync(serviceName, depth, ct);
            if (graph.TotalNodes == 0)
            {
                return NotFound(new ApiResponse<ServiceDependencyGraph>
                {
                    Success = false,
                    Message = $"Service '{serviceName}' not found in dependency graph"
                });
            }

            return Ok(new ApiResponse<ServiceDependencyGraph>
            {
                Data = graph,
                Success = true,
                Message = $"Retrieved dependency subgraph for '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dependency subgraph for {ServiceName}", serviceName);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<ServiceDependencyGraph>("Failed to retrieve dependency subgraph", ex));
        }
    }

    private static ApiResponse<T> ErrorResponse<T>(string message, Exception ex) => new()
    {
        Success = false,
        Message = message,
        ErrorDetails = ex.Message
    };
}
