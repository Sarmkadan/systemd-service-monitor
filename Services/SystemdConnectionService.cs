#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Tmds.DBus;
using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Exceptions;
using SystemdServiceMonitor.Integration; // Add this for DBusConnectionManager

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of systemd D-Bus connection management.
/// </summary>
public class SystemdConnectionService : ISystemdConnectionService
{
    private readonly ILogger<SystemdConnectionService> _logger;
    private readonly SystemdOptions _options;
    private readonly DBusConnectionManager _dbusConnectionManager;
    private bool _isConnected = false;
    private DateTime? _connectedSince;

    // D-Bus constants for systemd Manager
    private const string SystemdService = "org.freedesktop.systemd1";
    private const string SystemdPath = "/org/freedesktop/systemd1";
    private const string SystemdManagerInterface = "org.freedesktop.systemd1.Manager";

    /// <summary>
    /// Minimal D-Bus interface for fetching systemd Manager properties.
    /// </summary>
    [Tmds.DBus.DBusInterface(SystemdManagerInterface)] // Fully qualify DBusInterface
    public interface ISystemdManagerProperties : Tmds.DBus.IDBusObject // Fully qualify IDBusObject
    {
        // D-Bus properties are exposed as Get methods in Tmds.DBus
        Task<string> GetVersionAsync();
    }

    public bool IsConnected => _isConnected;
    public DateTime? ConnectedSince => _connectedSince;

    public SystemdConnectionService(ILogger<SystemdConnectionService> logger, SystemdOptions options, DBusConnectionManager dbusConnectionManager)
    {
        _logger = logger;
        _options = options;
        _dbusConnectionManager = dbusConnectionManager;
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        if (_isConnected)
            return true;

        try
        {
            await _dbusConnectionManager.GetConnectionAsync();
            _isConnected = true;
            _connectedSince = DateTime.UtcNow;
            _logger.LogInformation("Successfully connected to systemd D-Bus via DBusConnectionManager");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to systemd D-Bus via DBusConnectionManager");
            throw new DBusConnectionException("Failed to establish D-Bus connection to systemd", ex);
        }
    }

    public async Task<bool> VerifyConnectionAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            return false;

        try
        {
            // Verify connection status with the manager
            if (!await _dbusConnectionManager.IsConnectedAsync())
            {
                _isConnected = false;
                _connectedSince = null;
                return false;
            }

            // Attempt a simple D-Bus call to verify connection is active, e.g., get systemd version
            var version = await GetSystemdVersionAsync(ct);
            return !string.IsNullOrEmpty(version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "D-Bus connection verification failed. Reconnecting...");
            _isConnected = false;
            _connectedSince = null;
            await _dbusConnectionManager.ReconnectAsync(); // Attempt reconnection
            return false;
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            return;

        _logger.LogInformation("SystemdConnectionService state set to disconnected.");
        _isConnected = false;
        _connectedSince = null;
        await Task.CompletedTask; // The DBusConnectionManager handles actual D-Bus connection disposal.
    }

    public async Task<T?> CallMethodAsync<T>(string methodName, params object?[] args)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        _logger.LogWarning("CallMethodAsync is a generic placeholder and requires specific D-Bus interface definitions for robust implementation. Method: {MethodName}", methodName);
        throw new NotImplementedException($"Generic D-Bus method call '{methodName}' is not robustly implemented without specific D-Bus interface definitions. Please consider using a specific D-Bus proxy method.");
    }

    public async Task SubscribeToSignalsAsync(string signalName, Action<dynamic> handler, CancellationToken ct = default)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        _logger.LogWarning("SubscribeToSignalsAsync is a generic placeholder and requires specific D-Bus interface definitions for robust implementation. Signal: {SignalName}", signalName);
        throw new NotImplementedException($"Generic D-Bus signal subscription for '{signalName}' is not robustly implemented without specific D-Bus interface definitions. Please consider using a specific D-Bus proxy event.");
    }

    public async Task<string> GetSystemdVersionAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        try
        {
            _logger.LogDebug("Fetching systemd version using D-Bus");
            var connection = await _dbusConnectionManager.GetConnectionAsync();
            var managerProxy = connection.CreateProxy<ISystemdManagerProperties>(SystemdService, SystemdPath);
            string version = await managerProxy.GetVersionAsync();
            _logger.LogInformation("Systemd version: {Version}", version);
            return version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve systemd version via D-Bus");
            throw new ServiceMonitorException("Failed to retrieve systemd version", ex);
        }
    }
}
