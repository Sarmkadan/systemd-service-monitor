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
/// REST API controller for service log retrieval and filtering.
/// Provides endpoints for accessing systemd journal logs for specific services.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LogsController(
    IServiceLogService logService,
    ILogger<LogsController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves logs for a specific service with optional filtering and pagination.
    /// </summary>
    [HttpGet("{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ServiceLog>>> GetServiceLogs(
        string serviceName,
        [FromQuery] int lines = 100,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? severity = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchText = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<List<ServiceLog>>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            // Validate line count to prevent excessive data retrieval
            lines = Math.Clamp(lines, 1, 10000);

            var logs = await logService.GetServiceLogsAsync(serviceName, lines);

            // Apply filters
            if (!string.IsNullOrEmpty(severity))
            {
                logs = logs.Where(l => l.Severity.Contains(severity, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (startDate.HasValue)
            {
                logs = logs.Where(l => l.Timestamp >= startDate).ToList();
            }

            if (endDate.HasValue)
            {
                logs = logs.Where(l => l.Timestamp <= endDate).ToList();
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                logs = logs.Where(l =>
                    l.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    l.Unit.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = logs.Count;
            var paginatedLogs = logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new PaginatedResponse<ServiceLog>
            {
                Data = paginatedLogs,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Success = true,
                Message = $"Retrieved {paginatedLogs.Count} log entries for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving logs for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<List<ServiceLog>>
            {
                Success = false,
                Message = "Failed to retrieve service logs",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves recent logs for all services (limited for performance).
    /// </summary>
    [HttpGet("recent/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ServiceLog>>>> GetRecentLogs(
        [FromQuery] int minutes = 30,
        [FromQuery] int maxEntries = 1000)
    {
        try
        {
            // Clamp values to prevent excessive queries
            minutes = Math.Clamp(minutes, 1, 1440);
            maxEntries = Math.Clamp(maxEntries, 10, 10000);

            var cutoffTime = DateTime.UtcNow.AddMinutes(-minutes);
            var logs = await logService.GetRecentLogsAsync(maxEntries);

            var filteredLogs = logs
                .Where(l => l.Timestamp >= cutoffTime)
                .OrderByDescending(l => l.Timestamp)
                .Take(maxEntries)
                .ToList();

            return Ok(new ApiResponse<List<ServiceLog>>
            {
                Data = filteredLogs,
                Success = true,
                Message = $"Retrieved {filteredLogs.Count} recent log entries from the last {minutes} minutes"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent logs");
            return StatusCode(500, new ApiResponse<List<ServiceLog>>
            {
                Success = false,
                Message = "Failed to retrieve recent logs",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Retrieves error/warning logs for a specific service.
    /// Useful for quickly identifying issues.
    /// </summary>
    [HttpGet("{serviceName}/errors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<ServiceLog>>> GetServiceErrors(
        string serviceName,
        [FromQuery] int maxResults = 100,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new ApiResponse<List<ServiceLog>>
                {
                    Success = false,
                    Message = "Service name cannot be empty"
                });
            }

            maxResults = Math.Clamp(maxResults, 10, 10000);

            var logs = await logService.GetServiceLogsAsync(serviceName, maxResults);

            // Filter for error and warning level logs
            var errorLogs = logs.Where(l =>
                l.Severity.Contains("ERR", StringComparison.OrdinalIgnoreCase) ||
                l.Severity.Contains("WARN", StringComparison.OrdinalIgnoreCase) ||
                l.Severity.Contains("CRIT", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            var totalCount = errorLogs.Count;
            var paginatedErrors = errorLogs
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new PaginatedResponse<ServiceLog>
            {
                Data = paginatedErrors,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Success = true,
                Message = $"Retrieved {paginatedErrors.Count} error/warning entries for service '{serviceName}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving error logs for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<List<ServiceLog>>
            {
                Success = false,
                Message = "Failed to retrieve service error logs",
                ErrorDetails = ex.Message
            });
        }
    }

    /// <summary>
    /// Exports service logs in a specified format (JSON, CSV, XML).
    /// </summary>
    [HttpGet("{serviceName}/export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportServiceLogs(
        string serviceName,
        [FromQuery] string format = "json",
        [FromQuery] int lines = 500)
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

            if (!new[] { "json", "csv", "xml" }.Contains(format.ToLower()))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Format must be 'json', 'csv', or 'xml'"
                });
            }

            lines = Math.Clamp(lines, 10, 10000);
            var logs = await logService.GetServiceLogsAsync(serviceName, lines);

            var fileName = $"{serviceName}-logs-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

            return format.ToLower() switch
            {
                "csv" => File(
                    GenerateCsv(logs),
                    "text/csv",
                    $"{fileName}.csv"),
                "xml" => File(
                    GenerateXml(logs),
                    "application/xml",
                    $"{fileName}.xml"),
                _ => File(
                    System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })),
                    "application/json",
                    $"{fileName}.json")
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting logs for service {ServiceName}", serviceName);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to export service logs",
                ErrorDetails = ex.Message
            });
        }
    }

    private static byte[] GenerateCsv(List<ServiceLog> logs)
    {
        var csv = "Timestamp,Unit,Severity,Message\n";
        foreach (var log in logs)
        {
            var message = log.Message.Replace("\"", "\"\"");
            csv += $"\"{log.Timestamp:o}\",\"{log.Unit}\",\"{log.Severity}\",\"{message}\"\n";
        }
        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    private static byte[] GenerateXml(List<ServiceLog> logs)
    {
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<logs>\n";
        foreach (var log in logs)
        {
            xml += $"  <log>\n";
            xml += $"    <timestamp>{System.Security.SecurityElement.Escape(log.Timestamp.ToString("o"))}</timestamp>\n";
            xml += $"    <unit>{System.Security.SecurityElement.Escape(log.Unit)}</unit>\n";
            xml += $"    <severity>{System.Security.SecurityElement.Escape(log.Severity)}</severity>\n";
            xml += $"    <message>{System.Security.SecurityElement.Escape(log.Message)}</message>\n";
            xml += $"  </log>\n";
        }
        xml += "</logs>";
        return System.Text.Encoding.UTF8.GetBytes(xml);
    }
}
