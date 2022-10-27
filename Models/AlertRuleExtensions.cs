#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="AlertRule"/> to simplify common operations
/// such as severity comparison, tag management, and rule evaluation utilities.
/// </summary>
public static class AlertRuleExtensions
{
    /// <summary>
    /// Determines whether this alert rule is more severe than or equal to the specified severity.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <param name="severity">The severity to compare against.</param>
    /// <returns><c>true</c> if this rule's severity is greater than or equal to the specified severity; otherwise, <c>false</c>.</returns>
    public static bool IsSeverityAtLeast(this AlertRule rule, AlertSeverity severity)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.Severity >= severity;
    }

    /// <summary>
    /// Determines whether this alert rule is more severe than the specified severity.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <param name="severity">The severity to compare against.</param>
    /// <returns><c>true</c> if this rule's severity is strictly greater than the specified severity; otherwise, <c>false</c>.</returns>
    public static bool IsSeverityGreaterThan(this AlertRule rule, AlertSeverity severity)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.Severity > severity;
    }

    /// <summary>
    /// Checks if this alert rule has any of the specified tags.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <param name="tags">The tags to search for.</param>
    /// <returns><c>true</c> if the rule has any of the specified tags; otherwise, <c>false</c>.</returns>
    public static bool HasAnyTag(this AlertRule rule, params string[] tags)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        if (tags == null || tags.Length == 0)
        {
            return false;
        }

        return rule.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this alert rule has all of the specified tags.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <param name="tags">The tags that must all be present.</param>
    /// <returns><c>true</c> if the rule has all the specified tags; otherwise, <c>false</c>.</returns>
    public static bool HasAllTags(this AlertRule rule, params string[] tags)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        if (tags == null || tags.Length == 0)
        {
            return true;
        }

        return tags.All(tag => rule.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the effective cooldown period in seconds for this alert rule.
    /// </summary>
    /// <param name="rule">The alert rule.</param>
    /// <returns>The cooldown period in seconds.</returns>
    public static int GetCooldownSeconds(this AlertRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.CooldownMinutes * 60;
    }

    /// <summary>
    /// Determines whether this alert rule is enabled and ready for evaluation.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <returns><c>true</c> if the rule is enabled; otherwise, <c>false</c>.</returns>
    public static bool IsActive(this AlertRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.IsEnabled && !string.IsNullOrWhiteSpace(rule.ServicePattern);
    }

    /// <summary>
    /// Gets a display-friendly summary of this alert rule including name, severity, and condition.
    /// </summary>
    /// <param name="rule">The alert rule.</param>
    /// <returns>A formatted string summary.</returns>
    public static string GetSummary(this AlertRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return $"{rule.Name} ({rule.Severity}) - {rule.Condition} (Threshold: {rule.Threshold}) for {rule.ServicePattern}";
    }

    /// <summary>
    /// Determines whether this alert rule requires consecutive evaluations before triggering.
    /// </summary>
    /// <param name="rule">The alert rule to check.</param>
    /// <returns><c>true</c> if the rule requires consecutive evaluations; otherwise, <c>false</c>.</returns>
    public static bool RequiresConsecutiveEvaluations(this AlertRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.ConsecutiveEvaluationsRequired > 1;
    }

    /// <summary>
    /// Gets the number of consecutive evaluations required before this rule can trigger an alert.
    /// </summary>
    /// <param name="rule">The alert rule.</param>
    /// <returns>The number of consecutive evaluations required.</returns>
    public static int GetRequiredEvaluationCount(this AlertRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return rule.ConsecutiveEvaluationsRequired;
    }
}