#nullable enable

using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Validates <see cref="ServiceMonitorOptions"/> at startup.
/// </summary>
/// <remarks>
/// Every problem is collected into a single <see cref="ValidateOptionsResult"/>. Each message names the
/// offending property using its configuration key (for example <c>ServiceMonitor:PollingIntervalSeconds</c>).
/// </remarks>
public sealed class ServiceMonitorOptionsValidator : IValidateOptions<ServiceMonitorOptions>
{
    private const string Section = ServiceMonitorOptions.SectionName;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ServiceMonitorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.PollingIntervalSeconds < 1)
        {
            failures.Add($"{Section}:PollingIntervalSeconds must be at least 1, but was {options.PollingIntervalSeconds}.");
        }

        if (options.MonitoredUnits.Count == 0)
        {
            failures.Add($"{Section}:MonitoredUnits must contain at least one unit name.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
