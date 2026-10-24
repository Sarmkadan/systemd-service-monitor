#nullable enable

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// Seam over the systemd D-Bus calls that service refresh depends on,
/// so the monitor can be driven by an in-memory fake in tests.
/// </summary>
public interface ISystemdUnitClient
{
    /// <summary>
    /// Lists all units known to systemd (org.freedesktop.systemd1.Manager.ListUnits).
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The unit summaries.</returns>
    Task<IReadOnlyList<SystemdUnitSummary>> ListUnitsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the org.freedesktop.systemd1.Unit properties for a unit.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The raw property dictionary.</returns>
    Task<IDictionary<string, object>> GetUnitPropertiesAsync(string unitName, CancellationToken ct = default);
}
