#nullable enable

using System.Globalization;

namespace SystemdServiceMonitor.Dtos;

/// <summary>
/// Provides validation helpers for <see cref="LogStreamFilter"/> instances.
/// </summary>
public static class LogStreamFilterValidation
{
    /// <summary>
    /// Validates the specified <see cref="LogStreamFilter"/> instance.
    /// </summary>
    /// <param name="value">The filter to validate.</param>
    /// <returns>
    /// An empty list if the filter is valid; otherwise, a list of human-readable
    /// error messages describing each validation failure.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this LogStreamFilter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate ServiceName
        if (!string.IsNullOrEmpty(value.ServiceName) && value.ServiceName.Length > 255)
        {
            errors.Add(
                $"ServiceName must be at most 255 characters, but was {value.ServiceName.Length}.");
        }

        // Validate SearchTerm
        if (!string.IsNullOrEmpty(value.SearchTerm) && value.SearchTerm.Length > 1024)
        {
            errors.Add(
                $"SearchTerm must be at most 1024 characters, but was {value.SearchTerm.Length}.");
        }

        // Validate MinLevel
        if (value.MinLevel.HasValue)
        {
            var level = value.MinLevel.Value;
            if (!Enum.IsDefined(level))
            {
                errors.Add(
                    $"MinLevel has invalid value {(int)level}: {level}.");
            }
        }

        // Validate BufferSize (clamped to [0, 500])
        if (value.BufferSize < 0 || value.BufferSize > 500)
        {
            errors.Add(
                $"BufferSize must be between 0 and 500, but was {value.BufferSize}.");
        }

        // Validate PollingIntervalMs (clamped to [500, 30000])
        if (value.PollingIntervalMs < 500 || value.PollingIntervalMs > 30_000)
        {
            errors.Add(
                $"PollingIntervalMs must be between 500 and 30000, but was {value.PollingIntervalMs}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="LogStreamFilter"/> instance is valid.
    /// </summary>
    /// <param name="value">The filter to check.</param>
    /// <returns>
    /// <c>true</c> if the filter is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this LogStreamFilter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="LogStreamFilter"/> instance is valid.
    /// </summary>
    /// <param name="value">The filter to validate.</param>
    /// <exception cref="ArgumentException">
    /// The filter is invalid. The exception message lists all validation failures.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static void EnsureValid(this LogStreamFilter value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"LogStreamFilter is invalid. Validation failed:\n\t- {
                string.Join("\n\t- ", errors)
            }");
    }
}
