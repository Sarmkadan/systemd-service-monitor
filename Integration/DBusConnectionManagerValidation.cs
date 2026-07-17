#nullable enable

using System;
using System.Collections.Generic;

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// Provides validation helpers for <see cref="ConnectionStatusInfo"/> instances.
/// Validates connection state, error conditions, and configuration values.
/// </summary>
public static class DBusConnectionManagerValidation
{
    private const int MaxReconnectAttempts = 5;

    /// <summary>
    /// Validates the specified <see cref="ConnectionStatusInfo"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="ConnectionStatusInfo"/> to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ConnectionStatusInfo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate State
        if (string.IsNullOrEmpty(value.State))
        {
            problems.Add("State cannot be null or empty.");
        }

        // Validate ErrorMessage (should be null when no error)
        if (value.ErrorMessage is not null && value.ErrorMessage.Length > 0)
        {
            problems.Add("ErrorMessage should be null when there is no error.");
        }

        // Validate LastStatusCheck (should not be default/MinValue)
        if (value.LastStatusCheck == default)
        {
            problems.Add("LastStatusCheck must be set to a valid DateTime.");
        }

        // Validate ReconnectAttempts range
        if (value.ReconnectAttempts < 0)
        {
            problems.Add("ReconnectAttempts cannot be negative.");
        }
        else if (value.ReconnectAttempts > MaxReconnectAttempts)
        {
            problems.Add($"ReconnectAttempts ({value.ReconnectAttempts}) exceeds maximum of {MaxReconnectAttempts}.");
        }

        // Validate connection state consistency
        if (value.IsConnected && string.IsNullOrEmpty(value.State))
        {
            problems.Add("IsConnected is true but State is null or empty.");
        }

        if (!value.IsConnected && !string.IsNullOrEmpty(value.State) && value.State != "Failed")
        {
            problems.Add($"IsConnected is false but State is '{value.State}', expected 'Failed'.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ConnectionStatusInfo"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ConnectionStatusInfo"/> to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ConnectionStatusInfo? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="ConnectionStatusInfo"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="ConnectionStatusInfo"/> to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this ConnectionStatusInfo? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ConnectionStatusInfo validation failed:{Environment.NewLine}- {
                string.Join(
                    $"\n- ",
                    problems
                )
            }",
            nameof(value)
        );
    }
}