#nullable enable

using SystemdServiceMonitor.Integration;

/// <summary>
/// In-memory stand-in for the systemd D-Bus manager. Tests script unit state with
/// <see cref="SetUnit"/> and make a unit disappear from ListUnits with <see cref="RemoveUnit"/>.
/// </summary>
public sealed class FakeSystemdUnitClient : ISystemdUnitClient
{
    private readonly Dictionary<string, SystemdUnitSummary> _units = [];
    private readonly Dictionary<string, IDictionary<string, object>> _properties = [];

    /// <summary>
    /// Sets (or replaces) a unit as systemd would report it.
    /// </summary>
    /// <param name="name">The unit name, e.g. "demo.service".</param>
    /// <param name="activeState">The ActiveState string, e.g. "active" or "failed".</param>
    /// <param name="subState">The SubState string, e.g. "running" or "failed".</param>
    /// <param name="activeEnterUtc">When the unit last entered the active state, if it is active.</param>
    public void SetUnit(string name, string activeState, string subState, DateTime? activeEnterUtc = null)
    {
        _units[name] = new SystemdUnitSummary(name, $"Fake unit {name}", "loaded", activeState, subState);

        var properties = new Dictionary<string, object> { ["MainPID"] = 4242u };
        if (activeEnterUtc is { } since)
        {
            // systemd reports ActiveEnterTimestamp in microseconds since the Unix epoch.
            properties["ActiveEnterTimestamp"] = (ulong)((since.ToUniversalTime() - DateTime.UnixEpoch).Ticks / 10);
        }
        _properties[name] = properties;
    }

    /// <summary>
    /// Removes a unit so ListUnits no longer returns it.
    /// </summary>
    /// <param name="name">The unit name to remove.</param>
    public void RemoveUnit(string name)
    {
        _units.Remove(name);
        _properties.Remove(name);
    }

    public Task<IReadOnlyList<SystemdUnitSummary>> ListUnitsAsync(CancellationToken ct = default)
    {
        IReadOnlyList<SystemdUnitSummary> units = _units.Values.ToList();
        return Task.FromResult(units);
    }

    public Task<IDictionary<string, object>> GetUnitPropertiesAsync(string unitName, CancellationToken ct = default)
    {
        IDictionary<string, object> properties = _properties.TryGetValue(unitName, out var found)
            ? found
            : new Dictionary<string, object>();
        return Task.FromResult(properties);
    }
}
