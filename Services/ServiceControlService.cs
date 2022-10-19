#nullable enable

using Microsoft.Extensions.Logging;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Exceptions;
using SystemdServiceMonitor.Integration; // Add this for ISystemdManager
using Tmds.DBus; // Add this

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Implementation of systemd service control operations.
/// </summary>
public class ServiceControlService : IServiceControlService
{
    private readonly ILogger<ServiceControlService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly SystemdOptions _options;
    private readonly Dictionary<string, OperationResult> _lastOperations = [];

    public ServiceControlService(
        ILogger<ServiceControlService> logger,
        ISystemdConnectionService connectionService,
        SystemdOptions options)
    {
        _logger = logger;
        _connectionService = connectionService;
        _options = options;
    }

    private async Task<ISystemdManager> GetSystemdManagerProxy()
    {
        var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync();
        return connection.CreateProxy<ISystemdManager>("org.freedesktop.systemd1", "/org/freedesktop/systemd1");
    }

    public async Task<bool> StartServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Start", async () =>
        {
            _logger.LogInformation("Starting service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.StartUnitAsync(unitName, "replace"); // "replace" mode for unit operations
            return true;
        }, ct);
    }

    public async Task<bool> StopServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Stop", async () =>
        {
            _logger.LogInformation("Stopping service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.StopUnitAsync(unitName, "replace");
            return true;
        }, ct);
    }

    public async Task<bool> RestartServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Restart", async () =>
        {
            _logger.LogInformation("Restarting service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.RestartUnitAsync(unitName, "replace");
            return true;
        }, ct);
    }

    public async Task<bool> ReloadServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Reload", async () =>
        {
            _logger.LogInformation("Reloading service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.ReloadUnitAsync(unitName, "replace");
            return true;
        }, ct);
    }

    public async Task<bool> EnableServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Enable", async () =>
        {
            _logger.LogInformation("Enabling service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            // runtime: false - persist across reboots, force: true - create symlinks even if already existing
            var (success, failures) = await manager.EnableUnitFilesAsync(new[] { unitName }, false, true);
            if (!success)
            {
                _logger.LogError("Failed to enable service {ServiceName}. Failures: {Failures}", unitName, string.Join(", ", failures));
            }
            return success;
        }, ct);
    }

    public async Task<bool> DisableServiceAsync(string unitName, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, "Disable", async () =>
        {
            _logger.LogInformation("Disabling service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            // runtime: false - persist across reboots
            var (success, failures) = await manager.DisableUnitFilesAsync(new[] { unitName }, false);
            if (!success)
            {
                _logger.LogError("Failed to disable service {ServiceName}. Failures: {Failures}", unitName, string.Join(", ", failures));
            }
            return success;
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
                // Graceful and RollingRestart strategies would need more complex logic
                // involving monitoring service status and potentially dependent services.
                // For now, treat them as a direct restart.
                RestartStrategy.Graceful => await RestartServiceAsync(unitName, ct), // Simplified
                RestartStrategy.RollingRestart => await RestartServiceAsync(unitName, ct), // Simplified
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

            var manager = await GetSystemdManagerProxy();
            await manager.KillUnitAsync(unitName, "SIGTERM"); // Send SIGTERM for graceful shutdown
            // In a real scenario, we might wait and check status here, then send SIGKILL if needed.
            // For this implementation, we just send SIGTERM.
            return true;
        }, ct);
    }

    public async Task<OperationResult?> GetLastOperationStatusAsync(string unitName, CancellationToken ct = default)
    {
        return _lastOperations.TryGetValue(unitName, out var result) ? result : null;
    }

    public async Task<BulkOperationResult> BulkRestartAsync(
        IEnumerable<string> unitNames,
        int maxConcurrency = 3,
        CancellationToken ct = default)
    {
        var units = unitNames?.ToList() ?? [];
        if (units.Count == 0)
            return new BulkOperationResult { Results = [] };

        maxConcurrency = Math.Clamp(maxConcurrency, 1, 20);

        _logger.LogInformation(
            "Bulk restart requested for {Count} services (maxConcurrency: {Concurrency})",
            units.Count, maxConcurrency);

        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var results = new System.Collections.Concurrent.ConcurrentBag<OperationResult>();

        var tasks = units.Select(async unitName =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    var success = await RestartServiceAsync(unitName, ct);
                    results.Add(new OperationResult
                    {
                        UnitName = unitName,
                        Operation = "BulkRestart",
                        Success = success,
                        Message = success ? "Restarted successfully" : "Restart failed",
                        ExitCode = success ? 0 : 1,
                        OperationTime = startTime,
                        DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new OperationResult
                    {
                        UnitName = unitName,
                        Operation = "BulkRestart",
                        Success = false,
                        Message = $"Restart failed: {ex.Message}",
                        ExitCode = 1,
                        OperationTime = startTime,
                        DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                    });
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        var sortedResults = results.OrderBy(r => units.IndexOf(r.UnitName)).ToList();
        _logger.LogInformation(
            "Bulk restart completed: {Success} succeeded, {Failure} failed",
            sortedResults.Count(r => r.Success), sortedResults.Count(r => !r.Success));

        return new BulkOperationResult { Results = sortedResults };
    }

    private async Task<bool> ExecuteOperationAsync(
        string unitName,
        string operation,
        Func<Task<bool>> action,
        CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Starting operation {Operation} on service {ServiceName}", operation, unitName);

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

            _logger.LogInformation("Operation {Operation} on {ServiceName} completed: {Success} (duration: {Duration}ms)",
                operation, unitName, result, duration.TotalMilliseconds);
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

            _logger.LogError(ex, "Operation {Operation} on {ServiceName} failed after {Duration}ms",
                operation, unitName, duration.TotalMilliseconds);
            throw new ServiceOperationException(unitName, operation, ex.Message, ex);
        }
    }
}
