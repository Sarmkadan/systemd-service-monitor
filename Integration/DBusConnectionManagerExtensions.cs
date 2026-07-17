#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace SystemdServiceMonitor.Integration;

/// <summary>
/// Extension methods for <see cref="DBusConnectionManager"/> that provide additional functionality
/// for connection management, monitoring, and batch operations.
/// </summary>
/// <remarks>
/// All extension methods validate arguments and throw appropriate exceptions for invalid inputs.
/// </remarks>
public static class DBusConnectionManagerExtensions
{
    /// <summary>
    /// Gets the current connection state as a strongly-typed enum.
    /// </summary>
    /// <param name="manager">The connection manager instance.</param>
    /// <returns>The current connection state.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if the connection status cannot be retrieved.</exception>
    public static async Task<ConnectionState> GetConnectionStateAsync(this DBusConnectionManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        var status = await manager.GetStatusAsync().ConfigureAwait(false);
        return status.IsConnected
            ? ConnectionState.Connected
            : ConnectionState.Disconnected;
    }

    /// <summary>
    /// Gets the connection status information with additional derived properties.
    /// </summary>
    /// <param name="manager">The connection manager instance.</param>
    /// <returns>A connection status info object with additional computed properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown if the connection status cannot be retrieved.</exception>
    public static async Task<ExtendedConnectionStatusInfo> GetExtendedStatusAsync(this DBusConnectionManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        var status = await manager.GetStatusAsync().ConfigureAwait(false);

        return new ExtendedConnectionStatusInfo
        {
            IsConnected = status.IsConnected,
            State = status.State ?? "Unknown",
            LastStatusCheck = status.LastStatusCheck,
            ErrorMessage = status.ErrorMessage,
            ReconnectAttempts = status.ReconnectAttempts,
            Uptime = status.IsConnected && status.LastStatusCheck != default
                ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - status.LastStatusCheck.Ticks)
                : null,
            StatusAge = status.LastStatusCheck != default
                ? TimeSpan.FromTicks(DateTime.UtcNow.Ticks - status.LastStatusCheck.Ticks)
                : TimeSpan.Zero
        };
    }

    /// <summary>
    /// Attempts to reconnect with a maximum number of attempts, returning detailed failure information.
    /// </summary>
    /// <param name="manager">The connection manager instance.</param>
    /// <param name="maxAttempts">Maximum number of reconnection attempts. Use 0 for unlimited.</param>
    /// <returns>An enumerable of reconnection attempts with their outcomes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="manager"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxAttempts"/> is negative.</exception>
    public static async Task<IReadOnlyList<ReconnectionAttempt>> ReconnectWithDetailsAsync(
        this DBusConnectionManager manager,
        int maxAttempts = 5)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentOutOfRangeException.ThrowIfNegative(maxAttempts);

        var attempts = new List<ReconnectionAttempt>();

        if (maxAttempts == 0)
        {
            maxAttempts = int.MaxValue;
        }

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var attemptInfo = new ReconnectionAttempt
            {
                AttemptNumber = attempt + 1,
                Timestamp = DateTime.UtcNow,
                Success = false
            };

            try
            {
                var success = await manager.ReconnectAsync().ConfigureAwait(false);
                attemptInfo.Success = success;
                attemptInfo.Completed = true;

                if (success)
                {
                    attempts.Add(attemptInfo);
                    return attempts.AsReadOnly();
                }
            }
            catch (Exception ex)
            {
                attemptInfo.Error = ex.Message;
                attemptInfo.ExceptionType = ex.GetType().FullName;
            }

            attempts.Add(attemptInfo);

            if (attempt < maxAttempts - 1)
            {
                await Task.Delay(1000 * (int)Math.Pow(2, attempt)).ConfigureAwait(false);
            }
        }

        return attempts.AsReadOnly();
    }
}

/// <summary>
/// Represents the state of a D-Bus connection.
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connected
}

/// <summary>
/// Extended connection status information with additional computed properties.
/// </summary>
public class ExtendedConnectionStatusInfo : ConnectionStatusInfo
{
    /// <summary>
    /// Gets the uptime of the connection if connected, otherwise null.
    /// </summary>
    public TimeSpan? Uptime { get; set; }

    /// <summary>
    /// Gets the age of the status information.
    /// </summary>
    public TimeSpan StatusAge { get; set; }
}

/// <summary>
/// Information about a single reconnection attempt.
/// </summary>
public class ReconnectionAttempt
{
    /// <summary>
    /// Gets the attempt number (1-based).
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Gets the timestamp when the attempt was made.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets whether the attempt completed (regardless of success).
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Gets whether the attempt was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the error message if the attempt failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets the exception type name if an exception was thrown.
    /// </summary>
    public string? ExceptionType { get; set; }
}