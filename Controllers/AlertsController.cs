#nullable enable

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(
    IAlertRulesEngine alertRulesEngine,
    ILogger<AlertsController> logger) : ControllerBase
{
    [HttpGet("rules")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<AlertRuleDto>>>> GetRules(CancellationToken ct)
    {
        try
        {
            var rules = (await alertRulesEngine.GetRulesAsync(ct))
                .Select(ToAlertRuleDto)
                .ToList();

            return Ok(new ApiResponse<List<AlertRuleDto>>
            {
                Data = rules,
                Success = true,
                Message = $"Retrieved {rules.Count} alert rules"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving alert rules");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<List<AlertRuleDto>>("Failed to retrieve alert rules", ex));
        }
    }

    [HttpGet("rules/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> GetRuleById(Guid id, CancellationToken ct)
    {
        try
        {
            var rule = await alertRulesEngine.GetRuleByIdAsync(id, ct);
            if (rule is null)
            {
                return NotFound(new ApiResponse<AlertRuleDto>
                {
                    Success = false,
                    Message = $"Alert rule '{id}' not found"
                });
            }

            return Ok(new ApiResponse<AlertRuleDto>
            {
                Data = ToAlertRuleDto(rule),
                Success = true,
                Message = $"Retrieved alert rule '{rule.Name}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving alert rule {RuleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertRuleDto>("Failed to retrieve alert rule", ex));
        }
    }

    [HttpPost("rules")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> CreateRule([FromBody] CreateAlertRuleDto dto, CancellationToken ct)
    {
        try
        {
            var rule = await alertRulesEngine.AddRuleAsync(new AlertRule
            {
                Name = dto.Name,
                Description = dto.Description,
                ServicePattern = dto.ServicePattern,
                Condition = dto.Condition,
                Threshold = dto.Threshold,
                Severity = dto.Severity,
                EscalationPolicyId = dto.EscalationPolicyId,
                IsEnabled = dto.IsEnabled,
                CooldownMinutes = dto.CooldownMinutes,
                ConsecutiveEvaluationsRequired = dto.ConsecutiveEvaluationsRequired,
                Tags = dto.Tags ?? []
            }, ct);

            var response = new ApiResponse<AlertRuleDto>
            {
                Data = ToAlertRuleDto(rule),
                Success = true,
                Message = $"Created alert rule '{rule.Name}'"
            };

            return CreatedAtAction(nameof(GetRuleById), new { id = rule.Id }, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating alert rule {RuleName}", dto.Name);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertRuleDto>("Failed to create alert rule", ex));
        }
    }

    [HttpPut("rules/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertRuleDto>>> UpdateRule(Guid id, [FromBody] UpdateAlertRuleDto dto, CancellationToken ct)
    {
        try
        {
            var rule = await alertRulesEngine.UpdateRuleAsync(id, dto, ct);
            if (rule is null)
            {
                return NotFound(new ApiResponse<AlertRuleDto>
                {
                    Success = false,
                    Message = $"Alert rule '{id}' not found"
                });
            }

            return Ok(new ApiResponse<AlertRuleDto>
            {
                Data = ToAlertRuleDto(rule),
                Success = true,
                Message = $"Updated alert rule '{rule.Name}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating alert rule {RuleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertRuleDto>("Failed to update alert rule", ex));
        }
    }

    [HttpDelete("rules/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRule(Guid id, CancellationToken ct)
    {
        try
        {
            var removed = await alertRulesEngine.RemoveRuleAsync(id, ct);
            if (!removed)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    Success = false,
                    Message = $"Alert rule '{id}' not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                Success = true,
                Message = $"Deleted alert rule '{id}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting alert rule {RuleId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<bool>("Failed to delete alert rule", ex));
        }
    }

    [HttpGet("incidents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<AlertIncidentDto>>>> GetActiveIncidents(CancellationToken ct)
    {
        try
        {
            var incidents = (await alertRulesEngine.GetActiveIncidentsAsync(ct))
                .Select(ToAlertIncidentDto)
                .ToList();

            return Ok(new ApiResponse<List<AlertIncidentDto>>
            {
                Data = incidents,
                Success = true,
                Message = $"Retrieved {incidents.Count} active incidents"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active incidents");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<List<AlertIncidentDto>>("Failed to retrieve active incidents", ex));
        }
    }

    [HttpGet("incidents/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertIncidentDto>>> GetIncidentById(Guid id, CancellationToken ct)
    {
        try
        {
            var incident = await alertRulesEngine.GetIncidentByIdAsync(id, ct);
            if (incident is null)
            {
                return NotFound(new ApiResponse<AlertIncidentDto>
                {
                    Success = false,
                    Message = $"Alert incident '{id}' not found"
                });
            }

            return Ok(new ApiResponse<AlertIncidentDto>
            {
                Data = ToAlertIncidentDto(incident),
                Success = true,
                Message = $"Retrieved alert incident '{id}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving incident {IncidentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertIncidentDto>("Failed to retrieve alert incident", ex));
        }
    }

    [HttpPost("incidents/{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertIncidentDto>>> AcknowledgeIncident(Guid id, [FromBody] AcknowledgeIncidentDto dto, CancellationToken ct)
    {
        try
        {
            var incident = await alertRulesEngine.GetIncidentByIdAsync(id, ct);
            if (incident is null)
            {
                return NotFound(new ApiResponse<AlertIncidentDto>
                {
                    Success = false,
                    Message = $"Alert incident '{id}' not found"
                });
            }

            var acknowledged = await alertRulesEngine.AcknowledgeIncidentAsync(id, dto.AcknowledgedBy, ct);
            if (!acknowledged)
            {
                return Conflict(new ApiResponse<AlertIncidentDto>
                {
                    Success = false,
                    Message = $"Alert incident '{id}' cannot be acknowledged"
                });
            }

            var updatedIncident = await alertRulesEngine.GetIncidentByIdAsync(id, ct) ?? incident;
            return Ok(new ApiResponse<AlertIncidentDto>
            {
                Data = ToAlertIncidentDto(updatedIncident),
                Success = true,
                Message = $"Acknowledged alert incident '{id}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error acknowledging incident {IncidentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertIncidentDto>("Failed to acknowledge alert incident", ex));
        }
    }

    [HttpPost("incidents/{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertIncidentDto>>> ResolveIncident(Guid id, [FromBody] ResolveIncidentDto dto, CancellationToken ct)
    {
        try
        {
            var incident = await alertRulesEngine.GetIncidentByIdAsync(id, ct);
            if (incident is null)
            {
                return NotFound(new ApiResponse<AlertIncidentDto>
                {
                    Success = false,
                    Message = $"Alert incident '{id}' not found"
                });
            }

            var resolved = await alertRulesEngine.ResolveIncidentAsync(id, dto.ResolvedBy, dto.ResolutionNotes, ct);
            if (!resolved)
            {
                return Conflict(new ApiResponse<AlertIncidentDto>
                {
                    Success = false,
                    Message = $"Alert incident '{id}' cannot be resolved"
                });
            }

            var updatedIncident = await alertRulesEngine.GetIncidentByIdAsync(id, ct) ?? incident;
            return Ok(new ApiResponse<AlertIncidentDto>
            {
                Data = ToAlertIncidentDto(updatedIncident),
                Success = true,
                Message = $"Resolved alert incident '{id}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resolving incident {IncidentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertIncidentDto>("Failed to resolve alert incident", ex));
        }
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AlertSummaryDto>>> GetSummary(CancellationToken ct)
    {
        try
        {
            var summary = await alertRulesEngine.GetSummaryAsync(ct);
            return Ok(new ApiResponse<AlertSummaryDto>
            {
                Data = summary,
                Success = true,
                Message = "Retrieved alert summary"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving alert summary");
            return StatusCode(StatusCodes.Status500InternalServerError, ErrorResponse<AlertSummaryDto>("Failed to retrieve alert summary", ex));
        }
    }

    private static ApiResponse<T> ErrorResponse<T>(string message, Exception ex) => new()
    {
        Success = false,
        Message = message,
        ErrorDetails = ex.Message
    };

    private static AlertRuleDto ToAlertRuleDto(AlertRule rule) => new(
        rule.Id,
        rule.Name,
        rule.Description,
        rule.ServicePattern,
        rule.Condition,
        rule.Threshold,
        rule.Severity,
        rule.EscalationPolicyId,
        rule.IsEnabled,
        rule.CooldownMinutes,
        rule.ConsecutiveEvaluationsRequired,
        rule.Tags,
        rule.CreatedAt,
        rule.UpdatedAt);

    private static AlertIncidentDto ToAlertIncidentDto(AlertIncident incident) => new(
        incident.Id,
        incident.AlertRuleId,
        string.Empty,
        incident.ServiceName,
        incident.TriggerCondition,
        incident.Severity,
        incident.State,
        incident.CurrentEscalationLevel,
        incident.Summary,
        incident.ObservedValue,
        incident.AcknowledgedBy,
        incident.AcknowledgedAt,
        incident.ResolvedBy,
        incident.ResolvedAt,
        incident.ResolutionNotes,
        incident.EscalationHistory.Select(history => new EscalationHistoryDto(
            history.Id,
            history.LevelReached,
            history.LevelName,
            history.Channel,
            history.NotificationTarget,
            history.NotificationDelivered,
            history.DeliveryError,
            history.OccurredAt)).ToList(),
        incident.CreatedAt,
        incident.UpdatedAt);
}
