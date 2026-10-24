#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Alert rules declared in configuration.
/// </summary>
/// <remarks>
/// The <c>AlertRules</c> section is a JSON array at the root, which cannot bind to this type's
/// properties directly. <c>AddAlertRulesEngine</c> reads it into
/// <see cref="Rules"/> explicitly. Options are validated on start by <see cref="AlertRulesOptionsValidator"/>.
/// </remarks>
public sealed class AlertRulesOptions
{
    /// <summary>Configuration section key used during DI registration.</summary>
    public const string SectionName = "AlertRules";

    /// <summary>Rules declared in configuration, in declaration order.</summary>
    public List<AlertRuleOptions> Rules { get; set; } = [];

    /// <summary>
    /// Converts every configured rule into an <see cref="AlertRule"/>.
    /// </summary>
    /// <returns>A new list of rules. Call only on options that have passed validation.</returns>
    public IReadOnlyList<AlertRule> ToAlertRules()
        => Rules.Select(static rule => rule.ToAlertRule()).ToList();
}
