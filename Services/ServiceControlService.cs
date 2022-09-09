#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Exceptions;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of systemd service control operations.
/// </summary>
public class ServiceControlService : IServiceControlService
{
    private readonly ILogger<ServiceControlService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly SystemdOptions _options;
    private readonly ConcurrentDictionary<string, OperationResult> _lastOperations = new();

    public ServiceControlService(
        ILogger<ServiceControlService> logger,
        ISystemdConnectionService connectionService,
        SystemdOptions options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<bool> StartServiceAsync(string unitName, CancellationToken ct = default)
    {
        ValidateUnitName(unitName);
        return await ExecuteOperationAsync(unitName, "Start", async () =>
        {
            _logger.LogInformation("Starting service: {ServiceName}", unitName);
            // Placeholder: would call systemd D-Bus StartUnit method
            return true;
        }, ct);
    }

    public async Task<bool> StopServiceAsync(string unitName, CancellationToken ct = default)
    {
        ValidateUnitName(unitName);
        return await ExecuteOperationAsync(unitName, "Stop", async () =>
        {
            _logger.LogInformation("Stopping service: {ServiceName}", unitName);
            // Placeholder: would call systemd D-Bus StopUnit method
            return true;
        }, ct);
    }

    public async Task<bool> RestartServiceAsync(string unitName, CancellationToken ct = default)
    {
        ValidateUnitName(unitName);
        return await ExecuteOperationAsync(unitName, "Restart", async () =>
        {
            _logger.LogInformation("Restarting service: {ServiceName}", unitName);
            // Placeholder: would call systemd D-Bus RestartUnit method
            return true;
        }, ct);
    }

    public async Task<bool> ReloadServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Reload", async () =>
        {
            _logger.LogInformation("Reloading service: {ServiceName}", unitName);
            // Placeholder: would call systemd D-Bus ReloadUnit method
            return true;
        }, ct);
    }

    public async Task<bool> EnableServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Enable", async () =>
        {
            _logger.LogInformation("Enabling service: {ServiceName}", unitName);
            // Placeholder: would call systemctl enable via D-Bus
            return true;
        }, ct);
    }

    public async Task<bool> DisableServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Disable", async () =>
        {
            _logger.LogInformation("Disabling service: {ServiceName}", unitName);
            // Placeholder: would call systemctl disable via D-Bus
            return true;
        }, ct);
    }

    public async Task<bool> RestartWithStrategyAsync(string unitName, RestartStrategy strategy, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, $"Restart({strategy})", async () =>
        {
            _logger.LogInformation("Restarting service with strategy {Strategy}: {ServiceName}",
                strategy, unitName);

            return strategy switch
            {
                RestartStrategy.Immediate => await RestartServiceAsync(unitName, ct),
                RestartStrategy.Graceful => await GracefulShutdownAsync(unitName, 30, ct),
                RestartStrategy.RollingRestart => await RollingRestartAsync(unitName, ct),
                _ => throw new ArgumentException($"Unknown restart strategy: {strategy}")
            };
        }, ct);
    }

    public async Task<bool> GracefulShutdownAsync(string unitName, int timeoutSeconds = 30, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "GracefulShutdown", async () =>
        {
            _logger.LogInformation("Gracefully shutting down service: {ServiceName} (timeout: {TimeoutSeconds}s)",
                unitName, timeoutSeconds);

            // Placeholder: would send SIGTERM and wait for timeout before SIGKILL
            await Task.Delay(100, ct);
            return true;
        }, ct);
    }

    public async Task<OperationResult?> GetLastOperationStatusAsync(string unitName, CancellationToken ct = default)
    {
        return _lastOperations.TryGetValue(unitName, out var result) ? result : null;
    }

    private async Task<bool> ExecuteOperationAsync(
        string unitName,
        string operation,
        Func<Task<bool>> action,
        CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            if (!_connectionService.IsConnected)
            {
                _logger.LogWarning("D-Bus not connected, attempting reconnection");
                await _connectionService.ConnectAsync(ct);
            }

            var result = await action();

            var duration = DateTime.UtcNow - startTime;
            _lastOperations[unitName] = new OperationResult
            {
                UnitName = unitName,
                Operation = operation,
                Success = result,
                Message = result ? "Operation succeeded" : "Operation failed",
                ExitCode = result ? 0 : 1,
                OperationTime = startTime,
                DurationMs = (long)duration.TotalMilliseconds
            };

            _logger.LogInformation("Operation {Operation} on {ServiceName} completed: {Success}",
                operation, unitName, result);
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _lastOperations[unitName] = new OperationResult
            {
                UnitName = unitName,
                Operation = operation,
                Success = false,
                Message = $"Operation failed: {ex.Message}",
                ExitCode = 1,
                OperationTime = startTime,
                DurationMs = (long)duration.TotalMilliseconds
            };

            _logger.LogError(ex, "Operation {Operation} on {ServiceName} failed",
                operation, unitName);
            throw new ServiceOperationException(unitName, operation, ex.Message, ex);
        }
    }

    private async Task<bool> RollingRestartAsync(string unitName, CancellationToken ct)
    {
        // Placeholder for rolling restart strategy
        // This would involve:
        // 1. Stopping dependent services gracefully
        // 2. Restarting the main service
        // 3. Restarting dependent services
        await Task.Delay(100, ct);
        return true;
    }

    /// <summary>
    /// Validates the unit name to prevent injection attacks and invalid D-Bus calls.
    /// Systemd unit names must contain only alphanumeric chars, dashes, underscores,
    /// dots, and the @ symbol (for template instances).
    /// </summary>
    private static void ValidateUnitName(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
            throw new ArgumentException("Unit name cannot be null or empty", nameof(unitName));

        if (unitName.Length > 256)
            throw new ArgumentException("Unit name exceeds maximum length of 256 characters", nameof(unitName));

        foreach (var c in unitName)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.' && c != '@')
                throw new ArgumentException(
                    $"Unit name contains invalid character '{c}'. Only alphanumeric, dash, underscore, dot, and @ are allowed.",
                    nameof(unitName));
        }
    }
}
