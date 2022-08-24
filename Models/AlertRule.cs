// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Defines the condition evaluated by an alert rule to determine whether an incident
/// should be opened for a given service.
/// </summary>
public enum AlertCondition
{
    /// <summary>Service transitions to the Failed state.</summary>
    ServiceFailed,

    /// <summary>Service becomes inactive when it is expected to be running.</summary>
    ServiceInactive,

    /// <summary>CPU usage percentage exceeds the rule's configured threshold.</summary>
    CpuThresholdExceeded,

    /// <summary>Memory usage in MB exceeds the rule's configured threshold.</summary>
    MemoryThresholdExceeded,

    /// <summary>Cumulative restart count within the observation window exceeds the threshold.</summary>
    RestartCountExceeded,

    /// <summary>Health check reports <see cref="HealthStatus.Unhealthy"/>.</summary>
    HealthCheckUnhealthy,

    /// <summary>Health check reports <see cref="HealthStatus.Degraded"/> or worse.</summary>
    HealthCheckDegraded,

    /// <summary>Service uptime in seconds falls below the configured minimum while running.</summary>
    UptimeBelowMinimum,

    /// <summary>Any observable state transition on the service.</summary>
    AnyStateChange
}

/// <summary>
/// Priority classification of an alert. Used to determine notification urgency
/// and on-call escalation paths.
/// </summary>
public enum AlertSeverity
{
    /// <summary>Informational — no immediate action required.</summary>
    Info = 0,

    /// <summary>Low impact — address during normal working hours.</summary>
    Low = 1,

    /// <summary>Moderate impact — investigate before end of shift.</summary>
    Medium = 2,

    /// <summary>High impact — requires prompt attention within the hour.</summary>
    High = 3,

    /// <summary>Service-affecting — requires immediate 24/7 response.</summary>
    Critical = 4
}

/// <summary>
/// Lifecycle state of an <see cref="AlertIncident"/> from creation through resolution.
/// </summary>
public enum AlertIncidentState
{
    /// <summary>Incident is open and waiting for acknowledgement.</summary>
    Open,

    /// <summary>Incident has been acknowledged; escalation is paused.</summary>
    Acknowledged,

    /// <summary>Incident has been escalated to a higher on-call level.</summary>
    Escalated,

    /// <summary>Underlying condition cleared; incident auto-resolved by the engine.</summary>
    AutoResolved,

    /// <summary>Incident manually resolved by a responder.</summary>
    Resolved,

    /// <summary>Incident suppressed without formal resolution.</summary>
    Silenced
}

/// <summary>
/// Transport channel used to deliver alert notifications to responders.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Emit a structured log entry only.</summary>
    Log,

    /// <summary>HTTP POST a JSON payload to a configured URL.</summary>
    Webhook,

    /// <summary>Deliver via SMTP email.</summary>
    Email,

    /// <summary>Invoke a local shell script or executable.</summary>
    Script
}

/// <summary>
/// Defines a condition and policy for raising an alert when a monitored service
/// meets or exceeds a configured threshold.
/// </summary>
public class AlertRule
{
    /// <summary>Unique identifier for this rule.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Human-readable display name shown in dashboards and notifications.</summary>
    public required string Name { get; set; }

    /// <summary>Optional description of the business reason for this rule.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// systemd unit name or glob-style pattern to match against.
    /// Use <c>*</c> to match all services, or a trailing wildcard such as <c>nginx*</c>.
    /// </summary>
    public required string ServicePattern { get; set; }

    /// <summary>Condition evaluated against each incoming <see cref="ServiceStatus"/> snapshot.</summary>
    public AlertCondition Condition { get; set; }

    /// <summary>
    /// Numeric threshold for quantitative conditions
    /// (<see cref="AlertCondition.CpuThresholdExceeded"/>, <see cref="AlertCondition.MemoryThresholdExceeded"/>,
    /// <see cref="AlertCondition.RestartCountExceeded"/>, <see cref="AlertCondition.UptimeBelowMinimum"/>).
    /// Ignored for state-based conditions.
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>Severity label applied to all incidents opened by this rule.</summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    /// <summary>
    /// Reference to the <see cref="EscalationPolicy"/> executed when an incident is opened.
    /// When <c>null</c> the engine falls back to log-only notification.
    /// </summary>
    public Guid? EscalationPolicyId { get; set; }

    /// <summary>When <c>false</c> the rule is skipped during evaluation.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Minimum minutes before the same rule can open a second incident for the same service.
    /// Prevents alert storms during sustained outages.
    /// </summary>
    public int CooldownMinutes { get; set; } = 15;

    /// <summary>
    /// Number of consecutive matching evaluations required before an incident fires.
    /// A value of 1 triggers on the first match; higher values add hysteresis.
    /// </summary>
    public int ConsecutiveEvaluationsRequired { get; set; } = 1;

    /// <summary>Free-text tags for grouping, filtering, and routing rules.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>UTC timestamp of rule creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent update.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Configures a multi-level notification and escalation path for incidents raised by
/// <see cref="AlertRule"/> instances that reference this policy.
/// </summary>
public class EscalationPolicy
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name used in the UI and notification payloads.</summary>
    public required string Name { get; set; }

    /// <summary>Optional description of when and why this policy is used.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ordered escalation levels. Level at index 0 is notified immediately when the
    /// incident opens; subsequent levels are reached after their individual delay expires.
    /// </summary>
    public List<EscalationLevel> Levels { get; set; } = [];

    /// <summary>
    /// When <c>true</c> the policy cycles back to level 0 after exhausting all levels,
    /// up to <see cref="MaxRepeatCycles"/> total cycles.
    /// </summary>
    public bool RepeatEscalation { get; set; } = false;

    /// <summary>Maximum number of full escalation cycles before notification stops.</summary>
    public int MaxRepeatCycles { get; set; } = 3;

    /// <summary>UTC timestamp of policy creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single tier within an <see cref="EscalationPolicy"/>, describing which responders
/// are notified and when the engine advances to the next tier.
/// </summary>
public class EscalationLevel
{
    /// <summary>Zero-based index that determines evaluation order within the policy.</summary>
    public int Order { get; set; }

    /// <summary>Human-readable label for this tier (e.g., "Primary On-Call").</summary>
    public required string Name { get; set; }

    /// <summary>
    /// Minutes after incident creation (or prior escalation) before this level activates.
    /// Level 0 always activates immediately regardless of this value.
    /// </summary>
    public int EscalateAfterMinutes { get; set; } = 5;

    /// <summary>Delivery channels to attempt for this level.</summary>
    public List<NotificationChannel> Channels { get; set; } = [NotificationChannel.Log];

    /// <summary>
    /// Keyed notification targets per <see cref="NotificationChannel"/>.
    /// Keys are channel names (e.g., "Webhook", "Email"); values are the addresses or URLs.
    /// </summary>
    public Dictionary<string, string> NotificationTargets { get; set; } = [];

    /// <summary>
    /// Optional on-call schedule whose current active entry overrides <see cref="NotificationTargets"/>
    /// at notification time, enabling dynamic on-call routing.
    /// </summary>
    public Guid? OnCallScheduleId { get; set; }
}

/// <summary>
/// Defines a rotating on-call schedule that maps time windows to named responders,
/// used by <see cref="EscalationLevel"/> instances to resolve dynamic notification targets.
/// </summary>
public class OnCallSchedule
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name for the schedule (e.g., "Platform Team Rotation").</summary>
    public required string Name { get; set; }

    /// <summary>IANA timezone identifier used when evaluating shift boundaries (e.g., "America/New_York").</summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>All shift entries including overrides. Overrides take priority over regular entries.</summary>
    public List<OnCallEntry> Entries { get; set; } = [];

    /// <summary>UTC timestamp of schedule creation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A single shift assignment within an <see cref="OnCallSchedule"/>.
/// </summary>
public class OnCallEntry
{
    /// <summary>Unique identifier for this entry.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Full name of the on-call responder.</summary>
    public required string ResponderName { get; set; }

    /// <summary>Primary contact address — email address or webhook URL depending on <see cref="PreferredChannel"/>.</summary>
    public required string ContactTarget { get; set; }

    /// <summary>Preferred delivery channel for this responder.</summary>
    public NotificationChannel PreferredChannel { get; set; } = NotificationChannel.Email;

    /// <summary>UTC start time of this responder's shift.</summary>
    public DateTime ShiftStart { get; set; }

    /// <summary>UTC end time of this responder's shift (exclusive).</summary>
    public DateTime ShiftEnd { get; set; }

    /// <summary>
    /// When <c>true</c> this entry is a one-time override that supersedes any overlapping
    /// regular rotation entries for its window.
    /// </summary>
    public bool IsOverride { get; set; } = false;

    /// <summary>Optional free-text note (e.g., "covering for Alice while she is on leave").</summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Tracks the full lifecycle of a single alert, from the moment the triggering condition
/// is detected through acknowledgement, escalation, and final resolution.
/// </summary>
public class AlertIncident
{
    /// <summary>Unique identifier for this incident.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The <see cref="AlertRule"/> that opened this incident.</summary>
    public required Guid AlertRuleId { get; set; }

    /// <summary>Unit name of the service that triggered the alert.</summary>
    public required string ServiceName { get; set; }

    /// <summary>Condition that matched at the time of trigger.</summary>
    public required AlertCondition TriggerCondition { get; set; }

    /// <summary>Severity inherited from the triggering rule at the time of firing.</summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>Current lifecycle state.</summary>
    public AlertIncidentState State { get; set; } = AlertIncidentState.Open;

    /// <summary>Index of the current escalation level within the associated policy.</summary>
    public int CurrentEscalationLevel { get; set; } = 0;

    /// <summary>Human-readable description of the triggering condition and observed values.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Observed metric value at trigger time for threshold-based conditions.</summary>
    public decimal? ObservedValue { get; set; }

    /// <summary>Responder who acknowledged this incident.</summary>
    public string? AcknowledgedBy { get; set; }

    /// <summary>UTC timestamp of acknowledgement.</summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>Responder who resolved this incident, or <c>null</c> if auto-resolved.</summary>
    public string? ResolvedBy { get; set; }

    /// <summary>UTC timestamp of resolution.</summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Notes captured at resolution describing the root cause or remediation taken.</summary>
    public string ResolutionNotes { get; set; } = string.Empty;

    /// <summary>Chronological log of every escalation step taken for this incident.</summary>
    public List<EscalationHistory> EscalationHistory { get; set; } = [];

    /// <summary>UTC timestamp when this incident was first opened.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent state change.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// An immutable record of a single escalation step within an <see cref="AlertIncident"/>,
/// capturing which level was reached, which channel was used, and whether delivery succeeded.
/// </summary>
public class EscalationHistory
{
    /// <summary>Unique identifier for this history entry.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Zero-based index of the escalation level that was reached.</summary>
    public int LevelReached { get; set; }

    /// <summary>Display name of the level reached.</summary>
    public string LevelName { get; set; } = string.Empty;

    /// <summary>Channel used to deliver the notification.</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>Notification target (email address, webhook URL, etc.).</summary>
    public string NotificationTarget { get; set; } = string.Empty;

    /// <summary>Indicates whether the notification was successfully delivered.</summary>
    public bool NotificationDelivered { get; set; }

    /// <summary>Error detail when delivery failed; <c>null</c> on success.</summary>
    public string? DeliveryError { get; set; }

    /// <summary>UTC timestamp when this escalation step occurred.</summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
