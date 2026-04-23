#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for managing D-Bus connections to systemd.
/// </summary>
public interface ISystemdConnectionService
{
    /// <summary>
    /// Establishes connection to systemd D-Bus service.
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Verifies the D-Bus connection is active and healthy.
    /// </summary>
    Task<bool> VerifyConnectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Closes the D-Bus connection.
    /// </summary>
    Task DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Time when the connection was established.
    /// </summary>
    DateTime? ConnectedSince { get; }

    /// <summary>
    /// Executes a D-Bus method call on the systemd manager.
    /// </summary>
    Task<T?> CallMethodAsync<T>(string methodName, params object?[] args);

    /// <summary>
    /// Subscribes to systemd D-Bus signals.
    /// </summary>
    Task SubscribeToSignalsAsync(string signalName, Action<dynamic> handler, CancellationToken ct = default);

    /// <summary>
    /// Gets systemd version information.
    /// </summary>
    Task<string> GetSystemdVersionAsync(CancellationToken ct = default);
}
