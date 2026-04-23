#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Exceptions;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of systemd D-Bus connection management.
/// </summary>
public class SystemdConnectionService : ISystemdConnectionService
{
    private readonly ILogger<SystemdConnectionService> _logger;
    private readonly SystemdOptions _options;
    private bool _isConnected = false;
    private DateTime? _connectedSince;

    public bool IsConnected => _isConnected;
    public DateTime? ConnectedSince => _connectedSince;

    public SystemdConnectionService(ILogger<SystemdConnectionService> logger, SystemdOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        if (_isConnected)
            return true;

        int retryCount = 0;
        while (retryCount < _options.ConnectionRetryCount)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to systemd D-Bus (attempt {Attempt}/{MaxAttempts})",
                    retryCount + 1, _options.ConnectionRetryCount);

                // Simulated connection - in production this would initialize Tmds.DBus connection
                _isConnected = true;
                _connectedSince = DateTime.UtcNow;
                _logger.LogInformation("Successfully connected to systemd D-Bus");
                return true;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "D-Bus connection attempt failed");

                if (retryCount < _options.ConnectionRetryCount)
                {
                    await Task.Delay(_options.ConnectionRetryDelayMs, ct);
                }
            }
        }

        _logger.LogError("Failed to connect to systemd D-Bus after {RetryCount} attempts", _options.ConnectionRetryCount);
        throw new DBusConnectionException("Failed to establish D-Bus connection to systemd");
    }

    public async Task<bool> VerifyConnectionAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            return false;

        try
        {
            // Attempt a simple D-Bus call to verify connection is active
            var version = await GetSystemdVersionAsync(ct);
            return !string.IsNullOrEmpty(version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "D-Bus connection verification failed");
            _isConnected = false;
            return false;
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            return;

        try
        {
            _logger.LogInformation("Disconnecting from systemd D-Bus");
            _isConnected = false;
            _connectedSince = null;
            await Task.CompletedTask; // Placeholder for actual disconnection logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during D-Bus disconnection");
        }
    }

    public async Task<T?> CallMethodAsync<T>(string methodName, params object?[] args)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        try
        {
            _logger.LogDebug("Calling D-Bus method: {MethodName}", methodName);
            // Placeholder for actual D-Bus method invocation via Tmds.DBus
            await Task.Delay(10);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "D-Bus method call failed: {MethodName}", methodName);
            throw new ServiceMonitorException($"D-Bus method '{methodName}' call failed", ex);
        }
    }

    public async Task SubscribeToSignalsAsync(string signalName, Action<dynamic> handler, CancellationToken ct = default)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        try
        {
            _logger.LogInformation("Subscribing to D-Bus signal: {SignalName}", signalName);
            // Placeholder for actual signal subscription
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to D-Bus signal: {SignalName}", signalName);
            throw new ServiceMonitorException($"Signal subscription failed: {signalName}", ex);
        }
    }

    public async Task<string> GetSystemdVersionAsync(CancellationToken ct = default)
    {
        if (!_isConnected)
            throw new DBusConnectionException("Not connected to systemd D-Bus");

        try
        {
            _logger.LogDebug("Fetching systemd version");
            // Placeholder - would call systemd --version in production
            return await Task.FromResult("255 (systemd 255 or later)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve systemd version");
            throw new ServiceMonitorException("Failed to retrieve systemd version", ex);
        }
    }
}
