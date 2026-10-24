#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Configuration-bound form of an <see cref="AlertRule"/>.
/// </summary>
/// <remarks>
/// <see cref="Condition"/> and <see cref="Severity"/> are kept as strings so that unknown values
/// reach <see cref="AlertRulesOptionsValidator"/> and are reported together with every other
/// problem, instead of failing the configuration binder on the first bad enum value.
/// Call <see cref="ToAlertRule"/> only on options that have passed validation.
/// </remarks>
public sealed class AlertRuleOptions
{
    /// <summary>Human-readable display name. Must be unique across all configured rules.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional description of the business reason for this rule.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>systemd unit name or glob-style pattern (<c>*</c> or a trailing wildcard such as <c>nginx*</c>).</summary>
    public string ServicePattern { get; set; } = string.Empty;

    /// <summary>Name of an <see cref="AlertCondition"/> value, for example <c>ServiceFailed</c>.</summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>Numeric threshold for quantitative conditions. Ignored for state-based conditions.</summary>
    public decimal Threshold { get; set; }

    /// <summary>Name of an <see cref="AlertSeverity"/> value.</summary>
    public string Severity { get; set; } = nameof(AlertSeverity.Medium);

    /// <summary>Optional escalation policy identifier.</summary>
    public Guid? EscalationPolicyId { get; set; }

    /// <summary>When <c>false</c> the rule is skipped during evaluation.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Minutes before the same rule can open a second incident for the same service. Must be positive.</summary>
    public int CooldownMinutes { get; set; } = 15;

    /// <summary>Consecutive matching evaluations required before an incident fires. Must be at least 1.</summary>
    public int ConsecutiveEvaluationsRequired { get; set; } = 1;

    /// <summary>Free-text tags for grouping, filtering, and routing rules.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Converts this validated configuration entry into an <see cref="AlertRule"/> with a new identifier.
    /// </summary>
    /// <returns>A new <see cref="AlertRule"/> populated from this entry.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="Condition"/> or <see cref="Severity"/> is not a known value. Validate the options first.
    /// </exception>
    public AlertRule ToAlertRule()
    {
        var now = DateTime.UtcNow;

        return new AlertRule
        {
            Name = Name,
            Description = Description,
            ServicePattern = ServicePattern,
            Condition = Enum.Parse<AlertCondition>(Condition, ignoreCase: true),
            Threshold = Threshold,
            Severity = Enum.Parse<AlertSeverity>(Severity, ignoreCase: true),
            EscalationPolicyId = EscalationPolicyId,
            IsEnabled = IsEnabled,
            CooldownMinutes = CooldownMinutes,
            ConsecutiveEvaluationsRequired = ConsecutiveEvaluationsRequired,
            Tags = [.. Tags],
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
