#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Tmds.DBus;

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// Manages D-Bus connections for systemd service monitoring.
/// Handles connection lifecycle, reconnection logic, and connection pooling.
/// D-Bus is the inter-process communication system used by systemd.
/// </summary>
public class DBusConnectionManager : IDisposable
{
    private readonly ILogger<DBusConnectionManager> _logger;
    private Task<Connection>? _connectionTask;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private volatile bool _disposed;
    private int _reconnectAttempts;
    private const int MaxReconnectAttempts = 5;
    private const int ReconnectDelayMs = 1000;

    public DBusConnectionManager(ILogger<DBusConnectionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current D-Bus connection, establishing if necessary.
    /// If the cached connection task has faulted it is discarded so that a
    /// fresh connection is attempted, preventing leaked failed connection
    /// objects from exhausting D-Bus resources.
    /// </summary>
    public async Task<Connection> GetConnectionAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DBusConnectionManager));
        }

        await _connectionLock.WaitAsync();
        try
        {
            // Discard a faulted or cancelled task so a new connection is created.
            if (_connectionTask == null || _connectionTask.IsFaulted || _connectionTask.IsCanceled)
            {
                _connectionTask = CreateConnectionAsync();
            }
            return await _connectionTask;
        }
        catch (Exception ex)
        {
            // Ensure the faulted task is cleared so the next caller gets a
            // fresh attempt rather than the same failed connection object.
            _connectionTask = null;
            _logger.LogError(ex, "Failed to establish D-Bus connection");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<Connection> CreateConnectionAsync()
    {
        _logger.LogInformation("Initializing D-Bus connection to systemd");
        return await Connection.GetSessionConnectionAsync();
    }

    /// <summary>
    /// Attempts to reconnect to D-Bus with exponential backoff retry logic.
    /// The existing connection task is discarded before each attempt so that
    /// failed connection objects are not kept alive.
    /// </summary>
    public async Task<bool> ReconnectAsync()
    {
        if (_disposed)
        {
            return false;
        }

        for (int attempt = 0; attempt < MaxReconnectAttempts; attempt++)
        {
            try
            {
                _logger.LogInformation("Reconnecting to D-Bus (attempt {Attempt}/{MaxAttempts})",
                    attempt + 1, MaxReconnectAttempts);

                // Dispose and clear the old connection before attempting a new one.
                await _connectionLock.WaitAsync();
                try
                {
                    if (_connectionTask is { IsCompletedSuccessfully: true })
                    {
                        try { _connectionTask.Result?.Dispose(); } catch { /* ignore disposal errors */ }
                    }
                    _connectionTask = null;
                }
                finally
                {
                    _connectionLock.Release();
                }

                var connection = await GetConnectionAsync();
                _reconnectAttempts = 0;
                _logger.LogInformation("Successfully reconnected to D-Bus");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reconnection attempt {Attempt} failed", attempt + 1);

                if (attempt < MaxReconnectAttempts - 1)
                {
                    // Exponential backoff: 1s, 2s, 4s, 8s, 16s
                    var delayMs = ReconnectDelayMs * (int)Math.Pow(2, attempt);
                    await Task.Delay(delayMs);
                }
            }
        }

        _logger.LogError("Failed to reconnect to D-Bus after {MaxAttempts} attempts", MaxReconnectAttempts);
        return false;
    }

    /// <summary>
    /// Checks if the connection is currently active.
    /// </summary>
    public async Task<bool> IsConnectedAsync()
    {
        if (_disposed)
        {
            return false;
        }

        try
        {
            var connection = await GetConnectionAsync();
            return connection.State == DBusConnectionState.Connected;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets connection status information.
    /// </summary>
    public async Task<ConnectionStatusInfo> GetStatusAsync()
    {
        try
        {
            var connection = await GetConnectionAsync();
            return new ConnectionStatusInfo
            {
                IsConnected = connection.State == DBusConnectionState.Connected,
                State = connection.State.ToString(),
                LastStatusCheck = DateTime.UtcNow,
                ReconnectAttempts = _reconnectAttempts
            };
        }
        catch (Exception ex)
        {
            return new ConnectionStatusInfo
            {
                IsConnected = false,
                State = "Failed",
                LastStatusCheck = DateTime.UtcNow,
                ErrorMessage = ex.Message,
                ReconnectAttempts = _reconnectAttempts
            };
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _connectionLock.Wait();
        try
        {
            if (_connectionTask is { IsCompletedSuccessfully: true })
            {
                try { _connectionTask.Result?.Dispose(); } catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing D-Bus connection");
                }
            }
            _connectionTask = null;
        }
        finally
        {
            _connectionLock.Release();
            _connectionLock.Dispose();
        }
    }
}

/// <summary>
/// Connection status information.
/// </summary>
public class ConnectionStatusInfo
{
    public bool IsConnected { get; set; }
    public string? State { get; set; }
    public DateTime LastStatusCheck { get; set; }
    public string? ErrorMessage { get; set; }
    public int ReconnectAttempts { get; set; }
}

/// <summary>
/// D-Bus connection states for status tracking.
/// </summary>
public enum DBusConnectionState
{
    Connected,
    Disconnected,
    Connecting,
    Error
}
