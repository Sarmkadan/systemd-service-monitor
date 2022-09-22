#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Dtos;

/// <summary>
/// Read-only projection of an <see cref="AlertRule"/> returned by the alerts API.
/// </summary>
public record AlertRuleDto(
    Guid Id,
    string Name,
    string Description,
    string ServicePattern,
    AlertCondition Condition,
    decimal Threshold,
    AlertSeverity Severity,
    Guid? EscalationPolicyId,
    bool IsEnabled,
    int CooldownMinutes,
    int ConsecutiveEvaluationsRequired,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Input payload for creating a new alert rule.
/// </summary>
public record CreateAlertRuleDto(
    string Name,
    string Description,
    string ServicePattern,
    AlertCondition Condition,
    decimal Threshold,
    AlertSeverity Severity,
    Guid? EscalationPolicyId = null,
    bool IsEnabled = true,
    int CooldownMinutes = 15,
    int ConsecutiveEvaluationsRequired = 1,
    List<string>? Tags = null
);

/// <summary>
/// Partial-update payload for an existing alert rule.
/// Only non-null fields are applied; omitted fields retain their current values.
/// </summary>
public record UpdateAlertRuleDto(
    string? Name = null,
    string? Description = null,
    string? ServicePattern = null,
    AlertCondition? Condition = null,
    decimal? Threshold = null,
    AlertSeverity? Severity = null,
    Guid? EscalationPolicyId = null,
    bool? IsEnabled = null,
    int? CooldownMinutes = null,
    int? ConsecutiveEvaluationsRequired = null,
    List<string>? Tags = null
);

/// <summary>
/// Read-only projection of an <see cref="AlertIncident"/> returned by the alerts API.
/// </summary>
public record AlertIncidentDto(
    Guid Id,
    Guid AlertRuleId,
    string RuleName,
    string ServiceName,
    AlertCondition TriggerCondition,
    AlertSeverity Severity,
    AlertIncidentState State,
    int CurrentEscalationLevel,
    string Summary,
    decimal? ObservedValue,
    string? AcknowledgedBy,
    DateTime? AcknowledgedAt,
    string? ResolvedBy,
    DateTime? ResolvedAt,
    string ResolutionNotes,
    IReadOnlyList<EscalationHistoryDto> EscalationHistory,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Read-only projection of a single <see cref="Models.EscalationHistory"/> step.
/// </summary>
public record EscalationHistoryDto(
    Guid Id,
    int LevelReached,
    string LevelName,
    NotificationChannel Channel,
    string NotificationTarget,
    bool NotificationDelivered,
    string? DeliveryError,
    DateTime OccurredAt
);

/// <summary>
/// Request payload for acknowledging an open incident.
/// </summary>
public record AcknowledgeIncidentDto(
    /// <summary>Display name or user identifier of the responder.</summary>
    string AcknowledgedBy,
    /// <summary>Optional free-text note added at acknowledgement time.</summary>
    string? Notes = null
);

/// <summary>
/// Request payload for resolving an incident.
/// </summary>
public record ResolveIncidentDto(
    /// <summary>Display name or user identifier of the responder.</summary>
    string ResolvedBy,
    /// <summary>Root cause or remediation notes captured at resolution.</summary>
    string ResolutionNotes = ""
);

/// <summary>
/// Read-only projection of an <see cref="EscalationPolicy"/>.
/// </summary>
public record EscalationPolicyDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<EscalationLevelDto> Levels,
    bool RepeatEscalation,
    int MaxRepeatCycles,
    DateTime CreatedAt
);

/// <summary>
/// Read-only projection of a single <see cref="EscalationLevel"/>.
/// </summary>
public record EscalationLevelDto(
    int Order,
    string Name,
    int EscalateAfterMinutes,
    IReadOnlyList<NotificationChannel> Channels,
    IReadOnlyDictionary<string, string> NotificationTargets,
    Guid? OnCallScheduleId
);

/// <summary>
/// Input payload for creating a new escalation policy.
/// </summary>
public record CreateEscalationPolicyDto(
    string Name,
    string Description,
    List<EscalationLevelDto> Levels,
    bool RepeatEscalation = false,
    int MaxRepeatCycles = 3
);

/// <summary>
/// Read-only projection of an <see cref="OnCallSchedule"/>, including the currently
/// active on-call entry resolved at the time of the request.
/// </summary>
public record OnCallScheduleDto(
    Guid Id,
    string Name,
    string TimeZone,
    IReadOnlyList<OnCallEntryDto> Entries,
    /// <summary>The entry currently in effect, or <c>null</c> if no shift covers the current moment.</summary>
    OnCallEntryDto? CurrentOnCall,
    DateTime CreatedAt
);

/// <summary>
/// Read-only projection of a single <see cref="OnCallEntry"/>.
/// </summary>
public record OnCallEntryDto(
    Guid Id,
    string ResponderName,
    string ContactTarget,
    NotificationChannel PreferredChannel,
    DateTime ShiftStart,
    DateTime ShiftEnd,
    bool IsOverride,
    string Notes
);

/// <summary>
/// Input payload for creating a new on-call schedule.
/// </summary>
public record CreateOnCallScheduleDto(
    string Name,
    string TimeZone,
    List<OnCallEntryDto> Entries
);

/// <summary>
/// Input payload for adding a one-time override shift to an existing schedule.
/// </summary>
public record AddOnCallOverrideDto(
    string ResponderName,
    string ContactTarget,
    NotificationChannel PreferredChannel,
    DateTime ShiftStart,
    DateTime ShiftEnd,
    string Notes = ""
);

/// <summary>
/// Compact metrics snapshot used by dashboard summary widgets.
/// </summary>
public record AlertSummaryDto(
    /// <summary>Total number of alert rules configured in the engine.</summary>
    int TotalRules,
    /// <summary>Number of rules currently enabled.</summary>
    int EnabledRules,
    /// <summary>Active incidents in Open or Escalated state.</summary>
    int OpenIncidents,
    /// <summary>Subset of open incidents with <see cref="AlertSeverity.Critical"/> severity.</summary>
    int CriticalIncidents,
    /// <summary>Incidents currently in Acknowledged state.</summary>
    int AcknowledgedIncidents,
    /// <summary>Incidents resolved within the last 24 hours.</summary>
    int ResolvedLast24Hours,
    /// <summary>UTC time at which this summary was computed.</summary>
    DateTime AsOf
);
