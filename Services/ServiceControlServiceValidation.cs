#nullable enable

using System.Collections.Generic;
using System.Globalization;
using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Provides validation helpers for <see cref="ServiceControlService"/> instances.
/// </summary>
public static class ServiceControlServiceValidation
{
    /// <summary>
    /// Validates a <see cref="ServiceControlService"/> instance.
    /// </summary>
    /// <param name="value">The service control service to validate.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ServiceControlService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // ServiceControlService itself has no state to validate beyond null check
        // Validation is for method parameters when calling its methods

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ServiceControlService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service control service to check.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(this ServiceControlService? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ServiceControlService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service control service to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    public static void EnsureValid(this ServiceControlService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ServiceControlService is invalid. Validation errors:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.StartServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to start.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateStartServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.StartServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to start.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidStartServiceParameters(string? unitName)
    {
        return ValidateStartServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.StartServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to start.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidStartServiceParameters(string? unitName)
    {
        var errors = ValidateStartServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"StartServiceAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.StopServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to stop.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateStopServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.StopServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to stop.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidStopServiceParameters(string? unitName)
    {
        return ValidateStopServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.StopServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to stop.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidStopServiceParameters(string? unitName)
    {
        var errors = ValidateStopServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"StopServiceAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.RestartServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRestartServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.RestartServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidRestartServiceParameters(string? unitName)
    {
        return ValidateRestartServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.RestartServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidRestartServiceParameters(string? unitName)
    {
        var errors = ValidateRestartServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RestartServiceAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.ReloadServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to reload.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateReloadServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.ReloadServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to reload.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidReloadServiceParameters(string? unitName)
    {
        return ValidateReloadServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.ReloadServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to reload.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidReloadServiceParameters(string? unitName)
    {
        var errors = ValidateReloadServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ReloadServiceAsync parameters are valid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.EnableServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to enable.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateEnableServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.EnableServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to enable.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidEnableServiceParameters(string? unitName)
    {
        return ValidateEnableServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.EnableServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to enable.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidEnableServiceParameters(string? unitName)
    {
        var errors = ValidateEnableServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"EnableServiceAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.DisableServiceAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to disable.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateDisableServiceParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.DisableServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to disable.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidDisableServiceParameters(string? unitName)
    {
        return ValidateDisableServiceParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.DisableServiceAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to disable.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidDisableServiceParameters(string? unitName)
    {
        var errors = ValidateDisableServiceParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DisableServiceAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.RestartWithStrategyAsync(string, RestartStrategy, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <param name="strategy">The restart strategy to use.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateRestartWithStrategyParameters(string? unitName, RestartStrategy strategy)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        // strategy is an enum, always valid

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.RestartWithStrategyAsync(string, RestartStrategy, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <param name="strategy">The restart strategy to use.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidRestartWithStrategyParameters(string? unitName, RestartStrategy strategy)
    {
        return ValidateRestartWithStrategyParameters(unitName, strategy).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.RestartWithStrategyAsync(string, RestartStrategy, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to restart.</param>
    /// <param name="strategy">The restart strategy to use.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidRestartWithStrategyParameters(string? unitName, RestartStrategy strategy)
    {
        var errors = ValidateRestartWithStrategyParameters(unitName, strategy);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"RestartWithStrategyAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.GracefulShutdownAsync(string, int, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit to shut down.</param>
    /// <param name="timeoutSeconds">The timeout in seconds for graceful shutdown.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGracefulShutdownParameters(string? unitName, int timeoutSeconds)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        if (timeoutSeconds <= 0)
        {
            errors.Add("Timeout seconds must be greater than zero.");
        }
        else if (timeoutSeconds > 3600) // 1 hour maximum
        {
            errors.Add("Timeout seconds cannot exceed 3600 (1 hour).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.GracefulShutdownAsync(string, int, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to shut down.</param>
    /// <param name="timeoutSeconds">The timeout in seconds for graceful shutdown.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidGracefulShutdownParameters(string? unitName, int timeoutSeconds)
    {
        return ValidateGracefulShutdownParameters(unitName, timeoutSeconds).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.GracefulShutdownAsync(string, int, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit to shut down.</param>
    /// <param name="timeoutSeconds">The timeout in seconds for graceful shutdown.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidGracefulShutdownParameters(string? unitName, int timeoutSeconds)
    {
        var errors = ValidateGracefulShutdownParameters(unitName, timeoutSeconds);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GracefulShutdownAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.GetLastOperationStatusAsync(string, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitName">The name of the service unit.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateGetLastOperationStatusParameters(string? unitName)
    {
        ArgumentException.ThrowIfNullOrEmpty(unitName, nameof(unitName));

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(unitName))
        {
            errors.Add("Unit name cannot be empty or whitespace.");
        }
        else if (unitName.Any(c => char.IsWhiteSpace(c)))
        {
            errors.Add("Unit name cannot contain whitespace characters.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.GetLastOperationStatusAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidGetLastOperationStatusParameters(string? unitName)
    {
        return ValidateGetLastOperationStatusParameters(unitName).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.GetLastOperationStatusAsync(string, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitName">The name of the service unit.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidGetLastOperationStatusParameters(string? unitName)
    {
        var errors = ValidateGetLastOperationStatusParameters(unitName);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GetLastOperationStatusAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }

    /// <summary>
    /// Validates parameters for <see cref="ServiceControlService.BulkRestartAsync(IEnumerable{string}, int, CancellationToken)"/>.
    /// </summary>
    /// <param name="unitNames">The names of the service units to restart.</param>
    /// <param name="maxConcurrency">The maximum number of concurrent restarts.</param>
    /// <returns>A list of validation errors; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateBulkRestartParameters(IEnumerable<string>? unitNames, int maxConcurrency)
    {
        var errors = new List<string>();

        if (unitNames == null)
        {
            errors.Add("Unit names collection cannot be null.");
        }
        else
        {
            var list = unitNames.ToList();
            if (list.Count == 0)
            {
                errors.Add("Unit names collection cannot be empty.");
            }

            foreach (var unitName in list)
            {
                if (string.IsNullOrWhiteSpace(unitName))
                {
                    errors.Add("Unit name cannot be empty or whitespace.");
                    break; // Don't add multiple errors for the same issue
                }
                else if (unitName.Any(c => char.IsWhiteSpace(c)))
                {
                    errors.Add("Unit name cannot contain whitespace characters.");
                    break;
                }
            }
        }

        if (maxConcurrency <= 0)
        {
            errors.Add("Max concurrency must be greater than zero.");
        }
        else if (maxConcurrency > 50) // Reasonable upper limit
        {
            errors.Add("Max concurrency cannot exceed 50.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether parameters for <see cref="ServiceControlService.BulkRestartAsync(IEnumerable{string}, int, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitNames">The names of the service units to restart.</param>
    /// <param name="maxConcurrency">The maximum number of concurrent restarts.</param>
    /// <returns>True if parameters are valid; otherwise false.</returns>
    public static bool IsValidBulkRestartParameters(IEnumerable<string>? unitNames, int maxConcurrency)
    {
        return ValidateBulkRestartParameters(unitNames, maxConcurrency).Count == 0;
    }

    /// <summary>
    /// Ensures that parameters for <see cref="ServiceControlService.BulkRestartAsync(IEnumerable{string}, int, CancellationToken)"/> are valid.
    /// </summary>
    /// <param name="unitNames">The names of the service units to restart.</param>
    /// <param name="maxConcurrency">The maximum number of concurrent restarts.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValidBulkRestartParameters(IEnumerable<string>? unitNames, int maxConcurrency)
    {
        var errors = ValidateBulkRestartParameters(unitNames, maxConcurrency);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BulkRestartAsync parameters are invalid:\n- {string.Join("\n- ", errors)}");
        }
    }
}
