#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Top-level configuration options for the alert rules engine.
/// Bind from the <c>Alerts</c> section of <c>appsettings.json</c>.
/// </summary>
public sealed class AlertOptions
{
    /// <summary>Configuration section key used during DI registration.</summary>
    public const string SectionName = "Alerts";

    /// <summary>
    /// Master switch for the alert engine.  When <c>false</c> no rules are evaluated,
    /// no incidents are opened, and the escalation worker performs no work.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// When <c>true</c> the engine automatically resolves open or escalated incidents
    /// the next time their triggering condition evaluates as <c>false</c>.
    /// When <c>false</c> incidents must be resolved manually.
    /// </summary>
    public bool AutoResolveOnConditionCleared { get; set; } = true;

    /// <summary>
    /// Maximum number of incidents retained in the in-memory history.
    /// Oldest resolved incidents are pruned when this cap is exceeded.
    /// Has no effect on persistent repository-backed implementations.
    /// </summary>
    public int MaxIncidentHistorySize { get; set; } = 10_000;

    /// <summary>
    /// How often, in seconds, the <c>AlertEscalationWorker</c> checks for unacknowledged
    /// incidents that are due for escalation to the next policy level.
    /// </summary>
    public int EscalationCheckIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// How often, in seconds, the <c>AlertEscalationWorker</c> polls service statuses
    /// and feeds them into <see cref="Services.IAlertRulesEngine.EvaluateServiceAsync"/>.
    /// </summary>
    public int ServiceEvaluationIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Number of seconds to delay the start of the <c>AlertEscalationWorker</c> after
    /// the application boots, allowing other services to initialize first.
    /// </summary>
    public int StartupDelaySeconds { get; set; } = 10;

    /// <summary>Webhook delivery configuration applied to all webhook notifications.</summary>
    public WebhookNotificationOptions Webhook { get; set; } = new();

    /// <summary>Default values applied when creating escalation policies without explicit overrides.</summary>
    public EscalationDefaults EscalationDefaults { get; set; } = new();
}

/// <summary>
/// Configuration for outbound HTTP webhook notifications sent by the alert engine
/// when an incident reaches an escalation level that targets a webhook channel.
/// </summary>
public sealed class WebhookNotificationOptions
{
    /// <summary>Seconds before an outbound webhook POST request times out.</summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Number of automatic delivery retries on transient HTTP failures (5xx, network errors)
    /// before recording the escalation step as failed.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay in milliseconds between retry attempts, using linear back-off
    /// (attempt N waits <c>N × RetryDelayMs</c>).
    /// </summary>
    public int RetryDelayMs { get; set; } = 1_000;

    /// <summary>
    /// Static HTTP headers included in every outbound webhook request.
    /// Useful for authentication tokens or custom routing headers.
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = [];
}

/// <summary>
/// Default escalation timing values used when an <see cref="Models.EscalationPolicy"/>
/// level does not specify its own delay.
/// </summary>
public sealed class EscalationDefaults
{
    /// <summary>
    /// Minutes after incident creation before the engine attempts to escalate level-0
    /// incidents that have not been acknowledged.
    /// </summary>
    public int InitialEscalationDelayMinutes { get; set; } = 5;

    /// <summary>
    /// Minutes between each subsequent escalation level after the initial one.
    /// </summary>
    public int SubsequentEscalationDelayMinutes { get; set; } = 15;

    /// <summary>
    /// Hard cap on the number of escalation levels the engine will traverse for a single
    /// incident, regardless of policy configuration.
    /// </summary>
    public int MaxEscalationLevels { get; set; } = 4;

    /// <summary>
    /// Global default cooldown in minutes applied to rules that do not specify their own
    /// <see cref="Models.AlertRule.CooldownMinutes"/> value.
    /// </summary>
    public int DefaultCooldownMinutes { get; set; } = 15;
}
