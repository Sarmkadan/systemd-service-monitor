#nullable enable

using Tmds.DBus;

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// <see cref="ISystemdUnitClient"/> backed by the Tmds.DBus proxies for systemd.
/// </summary>
public sealed class DBusSystemdUnitClient : ISystemdUnitClient
{
    private readonly DBusConnectionManager _connectionManager;

    public DBusSystemdUnitClient(DBusConnectionManager connectionManager)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        _connectionManager = connectionManager;
    }

    public async Task<IReadOnlyList<SystemdUnitSummary>> ListUnitsAsync(CancellationToken ct = default)
    {
        var connection = await _connectionManager.GetConnectionAsync();
        var manager = connection.CreateProxy<ISystemdManager>("org.freedesktop.systemd1", "/org/freedesktop/systemd1");

        var units = await manager.ListUnitsAsync();
        return units
            .Select(u => new SystemdUnitSummary(u.Name, u.Description, u.LoadState, u.ActiveState, u.SubState))
            .ToList();
    }

    public async Task<IDictionary<string, object>> GetUnitPropertiesAsync(string unitName, CancellationToken ct = default)
    {
        var connection = await _connectionManager.GetConnectionAsync();

        // Kept exactly as the original RefreshServiceListAsync queried it: this reads the bus object
        // "/org/freedesktop/DBus" and ignores unitName. Pointing it at the unit object is a behavior
        // change and is tracked separately.
        var properties = connection.CreateProxy<IProperties>("org.freedesktop.DBus.Properties", "/org/freedesktop/DBus");
        return await properties.GetAllAsync("org.freedesktop.systemd1.Unit");
    }
}
