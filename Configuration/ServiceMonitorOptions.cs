#nullable enable

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Settings for service monitoring, bound from the <c>ServiceMonitor</c> section.
/// </summary>
/// <remarks>
/// Validated on start by <see cref="ServiceMonitorOptionsValidator"/>; an invalid value aborts host startup.
/// </remarks>
public sealed class ServiceMonitorOptions
{
    /// <summary>Configuration section key used during DI registration.</summary>
    public const string SectionName = "ServiceMonitor";

    /// <summary>How often each monitored unit is polled, in seconds. Must be at least 1.</summary>
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>Names of the systemd units to monitor. Must contain at least one unit.</summary>
    public List<string> MonitoredUnits { get; set; } = [];
}
