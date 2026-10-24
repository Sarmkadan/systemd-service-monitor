#nullable enable

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// One entry of the systemd Manager.ListUnits result.
/// </summary>
public sealed record SystemdUnitSummary(
    string Name,
    string Description,
    string LoadState,
    string ActiveState,
    string SubState);
