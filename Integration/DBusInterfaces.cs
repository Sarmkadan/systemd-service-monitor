#nullable enable
using Tmds.DBus;

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// D-Bus interface for org.freedesktop.systemd1.Manager.
/// Used to list, get, and control systemd units.
/// </summary>
[DBusInterface("org.freedesktop.systemd1.Manager")]
public interface ISystemdManager : IDBusObject
{
    Task<(string Name, string Description, string LoadState, string ActiveState, string SubState, string Followed, string Path, ObjectPath JobPath)[]> ListUnitsAsync();
    Task<ObjectPath> GetUnitAsync(string name);
    Task StartUnitAsync(string name, string mode);
    Task StopUnitAsync(string name, string mode);
    Task RestartUnitAsync(string name, string mode);
    Task ReloadUnitAsync(string name, string mode);
    Task KillUnitAsync(string name, string signal);
    Task<(bool Success, string[] Failures)> EnableUnitFilesAsync(string[] names, bool runtime, bool force);
    Task<(bool Success, string[] Failures)> DisableUnitFilesAsync(string[] names, bool runtime);
}

/// <summary>
/// D-Bus interface for org.freedesktop.DBus.Properties.
/// Used to get all properties of a D-Bus object.
/// </summary>
[DBusInterface("org.freedesktop.DBus.Properties")]
public interface IProperties : IDBusObject
{
    Task<IDictionary<string, object>> GetAllAsync(string @interface);
}

/// <summary>
/// D-Bus interface for a systemd Unit (e.g., org.freedesktop.systemd1.Unit).
/// Note: Specific unit interfaces like org.freedesktop.systemd1.Service
/// are typically inherited from this or provide more specific properties.
/// This is a generic representation for fetching common unit properties.
/// </summary>
[DBusInterface("org.freedesktop.systemd1.Unit")]
public interface ISystemdUnit : IDBusObject
{
    // Common properties that can be fetched via IProperties.GetAllAsync
    // Adding them here just for conceptual clarity, they will be read via GetAllAsync
    // Task<string> GetActiveStateAsync();
    // Task<string> GetSubStateAsync();
    // Task<uint> GetMainPIDAsync();
    // Task<ulong> GetCPUUsageNsecAsync();
    // Task<ulong> GetMemoryCurrentAsync();
    // Task<ulong> GetActiveEnterTimestampAsync();
}
