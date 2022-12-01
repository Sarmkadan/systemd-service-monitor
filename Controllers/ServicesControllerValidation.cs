#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="ServicesController"/> instances.
/// Validates controller state including service names and concurrency settings.
/// </summary>
public static class ServicesControllerValidation
{
    /// <summary>
    /// Validates the specified <see cref="ServicesController"/> instance.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServicesController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate MaxConcurrency
        if (value.MaxConcurrency < 1 || value.MaxConcurrency > 20)
        {
            problems.Add(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "MaxConcurrency must be between 1 and 20, but was {0}.",
                    value.MaxConcurrency));
        }

        // Validate ServiceNames
        if (value.ServiceNames is null)
        {
            problems.Add("ServiceNames collection cannot be null.");
        }
        else if (!value.ServiceNames.Any())
        {
            problems.Add("ServiceNames collection cannot be empty.");
        }
        else
        {
            var invalidNames = value.ServiceNames
                .Where(name => string.IsNullOrWhiteSpace(name))
                .ToList();

            if (invalidNames.Count > 0)
            {
                problems.Add(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "ServiceNames collection contains {0} null or whitespace entries.",
                        invalidNames.Count));
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ServicesController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ServicesController value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ServicesController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the controller is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ServicesController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Join(" ", problems),
                nameof(value));
        }
    }
}
