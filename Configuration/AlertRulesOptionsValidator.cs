#nullable enable

using Microsoft.Extensions.Options;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Validates <see cref="AlertRulesOptions"/> at startup.
/// </summary>
/// <remarks>
/// Every problem in every rule is collected into a single <see cref="ValidateOptionsResult"/>, so one
/// startup failure lists all invalid rules rather than only the first. Each message names the rule
/// (by <see cref="AlertRuleOptions.Name"/>, or by position when the name is empty).
/// </remarks>
public sealed class AlertRulesOptionsValidator : IValidateOptions<AlertRulesOptions>
{
    private static readonly AlertCondition[] QuantitativeConditions =
    [
        AlertCondition.CpuThresholdExceeded,
        AlertCondition.MemoryThresholdExceeded,
        AlertCondition.RestartCountExceeded,
        AlertCondition.UptimeBelowMinimum
    ];

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AlertRulesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        for (var i = 0; i < options.Rules.Count; i++)
        {
            ValidateRule(options.Rules[i], i, failures);
        }

        ValidateUniqueNames(options.Rules, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateRule(AlertRuleOptions rule, int index, List<string> failures)
    {
        var label = string.IsNullOrWhiteSpace(rule.Name)
            ? $"AlertRules[{index}]"
            : $"Rule '{rule.Name}'";

        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            failures.Add($"{label}: Name is required.");
        }

        if (string.IsNullOrWhiteSpace(rule.ServicePattern))
        {
            failures.Add($"{label}: ServicePattern (unit name) is required.");
        }
        else if (rule.ServicePattern.Any(char.IsWhiteSpace))
        {
            failures.Add($"{label}: ServicePattern '{rule.ServicePattern}' cannot contain whitespace.");
        }

        var conditionIsKnown = TryParseEnum<AlertCondition>(rule.Condition, out var condition);
        if (!conditionIsKnown)
        {
            failures.Add(string.IsNullOrWhiteSpace(rule.Condition)
                ? $"{label}: Condition is required. Valid values: {ValidValues<AlertCondition>()}."
                : $"{label}: unknown condition type '{rule.Condition}'. Valid values: {ValidValues<AlertCondition>()}.");
        }
        else if (QuantitativeConditions.Contains(condition) && rule.Threshold <= 0)
        {
            failures.Add($"{label}: Threshold must be greater than 0 for {condition}, but was {rule.Threshold}.");
        }

        if (!TryParseEnum<AlertSeverity>(rule.Severity, out _))
        {
            failures.Add(string.IsNullOrWhiteSpace(rule.Severity)
                ? $"{label}: Severity is required. Valid values: {ValidValues<AlertSeverity>()}."
                : $"{label}: unknown severity '{rule.Severity}'. Valid values: {ValidValues<AlertSeverity>()}.");
        }

        if (rule.CooldownMinutes <= 0)
        {
            failures.Add($"{label}: CooldownMinutes must be greater than 0, but was {rule.CooldownMinutes}.");
        }

        if (rule.ConsecutiveEvaluationsRequired < 1)
        {
            failures.Add($"{label}: ConsecutiveEvaluationsRequired must be at least 1, but was {rule.ConsecutiveEvaluationsRequired}.");
        }
    }

    private static void ValidateUniqueNames(IReadOnlyList<AlertRuleOptions> rules, List<string> failures)
    {
        var duplicates = rules
            .Select((rule, index) => (Name: rule.Name.Trim(), Position: index + 1))
            .Where(entry => entry.Name.Length > 0)
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            var positions = string.Join(", ", group.Select(entry => entry.Position));
            failures.Add($"Rule '{group.Key}' is defined {group.Count()} times (positions {positions}); rule names must be unique.");
        }
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        // Enum.TryParse accepts numeric strings such as "99", so IsDefined rejects values outside the enum.
        if (Enum.TryParse(value, ignoreCase: true, out result) && Enum.IsDefined(result))
        {
            return true;
        }

        result = default;
        return false;
    }

    private static string ValidValues<TEnum>() where TEnum : struct, Enum
        => string.Join(", ", Enum.GetNames<TEnum>());
}
