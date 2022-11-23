using System;
using System.Collections.Generic;
using System.Globalization;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="AlertRule"/> instances.
/// </summary>
public static class AlertRuleValidation
{
    /// <summary>
    /// Validates an <see cref="AlertRule"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The rule to validate.</param>
    /// <returns>A read-only list of validation errors; empty if the rule is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this AlertRule value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required properties
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name is required and cannot be empty or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.ServicePattern))
        {
            errors.Add("ServicePattern is required and cannot be empty or whitespace.");
        }

        if (value.Condition == default)
        {
            errors.Add("Condition is required and cannot be default.");
        }

        // Validate numeric properties based on condition type
        switch (value.Condition)
        {
            case AlertCondition.CpuThresholdExceeded:
            case AlertCondition.MemoryThresholdExceeded:
            case AlertCondition.RestartCountExceeded:
            case AlertCondition.UptimeBelowMinimum:
                if (value.Threshold <= 0)
                {
                    errors.Add("Threshold must be greater than 0 for quantitative conditions.");
                }
                break;
        }

        // Validate severity
        if (!Enum.IsDefined(value.Severity))
        {
            errors.Add("Severity must be a valid AlertSeverity value.");
        }

        // Validate cooldown minutes (should be reasonable)
        if (value.CooldownMinutes < 0)
        {
            errors.Add("CooldownMinutes cannot be negative.");
        }

        // Validate consecutive evaluations required (should be positive)
        if (value.ConsecutiveEvaluationsRequired < 1)
        {
            errors.Add("ConsecutiveEvaluationsRequired must be at least 1.");
        }

        // Validate tags collection
        if (value.Tags is null)
        {
            errors.Add("Tags collection cannot be null.");
        }
        else
        {
            foreach (var tag in value.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    errors.Add("Tags cannot contain null, empty, or whitespace entries.");
                    break;
                }
            }
        }

        // Validate timestamps (should not be default)
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be default DateTime.");
        }

        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be default DateTime.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="AlertRule"/> is valid.
    /// </summary>
    /// <param name="value">The rule to check.</param>
    /// <returns><c>true</c> if the rule is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this AlertRule value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="AlertRule"/> is valid, throwing an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if it is not.
    /// </summary>
    /// <param name="value">The rule to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the rule has validation errors.</exception>
    public static void EnsureValid(this AlertRule value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AlertRule is invalid. Problems:\n- {string.Join("\n- ", errors)}",
                nameof(value));
        }
    }
}