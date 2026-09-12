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

    private const string SystemdServiceName = "org.freedesktop.systemd1";
    private const string SystemdServicePath = "/org/freedesktop/systemd1";
    private const string SystemdUnitInterface = "org.freedesktop.systemd1.Unit";
    private const string ReplaceMode = "replace";
    private const string SigtermSignal = "SIGTERM";
    private const string SigkillSignal = "SIGKILL";
    private const string ActiveState = "active";
    private const string DeactivatingState = "deactivating";
    private const string ActiveStateProperty = "ActiveState";
    private const string StartOperation = "Start";
    private const string StopOperation = "Stop";
    private const string RestartOperation = "Restart";
    private const string ReloadOperation = "Reload";
    private const string EnableOperation = "Enable";
    private const string DisableOperation = "Disable";
    private const string GracefulShutdownOperation = "GracefulShutdown";
    private const string BulkRestartOperation = "BulkRestart";
    private const string OperationSucceededMessage = "Operation succeeded";
    private const string OperationFailedMessage = "Operation failed";
    private const string RestartedSuccessfullyMessage = "Restarted successfully";
    private const string RestartFailedMessage = "Restart failed";
    private const int SuccessExitCode = 0;
    private const int FailureExitCode = 1;
    private const int DefaultShutdownTimeoutSeconds = 30;
    private const int DefaultMaxConcurrency = 3;
    private const int MinConcurrency = 1;
    private const int MaxConcurrency = 20;
    private const int PollIntervalSeconds = 1;

    public ServiceControlService(
        ILogger<ServiceControlService> logger,
        ISystemdConnectionService connectionService,
        SystemdOptions options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(connectionService);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _connectionService = connectionService;
        _options = options;
    }

    private async Task<ISystemdManager> GetSystemdManagerProxy()
    {
        var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync();
        return connection.CreateProxy<ISystemdManager>(SystemdServiceName, SystemdServicePath);
    }

    public async Task<bool> StartServiceAsync(string unitName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, StartOperation, async () =>
        {
            _logger.LogInformation("Starting service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.StartUnitAsync(unitName, ReplaceMode); // "replace" mode for unit operations
            return true;
        }, ct);
    }

    public async Task<bool> StopServiceAsync(string unitName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, StopOperation, async () =>
        {
            _logger.LogInformation("Stopping service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.StopUnitAsync(unitName, ReplaceMode);
            return true;
        }, ct);
    }

    public async Task<bool> RestartServiceAsync(string unitName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, RestartOperation, async () =>
        {
            _logger.LogInformation("Restarting service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.RestartUnitAsync(unitName, ReplaceMode);
            return true;
        }, ct);
    }

    public async Task<bool> ReloadServiceAsync(string unitName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, ReloadOperation, async () =>
        {
            _logger.LogInformation("Reloading service: {ServiceName}", unitName);
            var manager = await GetSystemdManagerProxy();
            await manager.ReloadUnitAsync(unitName, ReplaceMode);
            return true;
        }, ct);
    }

    public async Task<bool> EnableServiceAsync(string unitName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, EnableOperation, async () =>
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
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
        return await ExecuteOperationAsync(unitName, DisableOperation, async () =>
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
        ArgumentException.ThrowIfNullOrWhiteSpace(unitName);
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

    public async Task<bool> GracefulShutdownAsync(string unitName, int timeoutSeconds = DefaultShutdownTimeoutSeconds, CancellationToken ct = default)
    {
        return await ExecuteOperationAsync(unitName, GracefulShutdownOperation, async () =>
        {
            _logger.LogInformation("Gracefully shutting down service: {ServiceName} (timeout: {TimeoutSeconds}s)",
                unitName, timeoutSeconds);

            var manager = await GetSystemdManagerProxy();
            await manager.KillUnitAsync(unitName, SigtermSignal); // Send SIGTERM for graceful shutdown

            var connection = await _connectionService.DBusConnectionManager.GetConnectionAsync();
            var unitPath = await manager.GetUnitAsync(unitName);
            var unitProxy = connection.CreateProxy<IProperties>(SystemdUnitInterface, unitPath);

            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            while (DateTime.UtcNow < deadline)
            {
                if (ct.IsCancellationRequested) break;

                var unitProperties = await unitProxy.GetAllAsync(SystemdUnitInterface);
                if (unitProperties.TryGetValue(ActiveStateProperty, out var stateVal) &&
                    stateVal is string state && !string.Equals(state, ActiveState, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(state, DeactivatingState, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Service {ServiceName} stopped gracefully with state {State}", unitName, state);
                    return true;
                }

                await Task.Delay(TimeSpan.FromSeconds(PollIntervalSeconds), ct);
            }

            _logger.LogWarning("Service {ServiceName} did not stop within {TimeoutSeconds}s, sending SIGKILL", unitName, timeoutSeconds);
            await manager.KillUnitAsync(unitName, SigkillSignal);
            return true;
        }, ct);
    }

    public async Task<OperationResult?> GetLastOperationStatusAsync(string unitName, CancellationToken ct = default)
    {
        return _lastOperations.TryGetValue(unitName, out var result) ? result : null;
    }

    public async Task<BulkOperationResult> BulkRestartAsync(
        IEnumerable<string> unitNames,
        int maxConcurrency = DefaultMaxConcurrency,
        CancellationToken ct = default)
    {
        var units = unitNames?.ToList() ?? [];
        if (units.Count == 0)
            return new BulkOperationResult { Results = [] };

        maxConcurrency = Math.Clamp(maxConcurrency, MinConcurrency, MaxConcurrency);

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
                        Operation = BulkRestartOperation,
                        Success = success,
                        Message = success ? RestartedSuccessfullyMessage : RestartFailedMessage,
                        ExitCode = success ? SuccessExitCode : FailureExitCode,
                        OperationTime = startTime,
                        DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new OperationResult
                    {
                        UnitName = unitName,
                        Operation = BulkRestartOperation,
                        Success = false,
                        Message = $"Restart failed: {ex.Message}",
                        ExitCode = FailureExitCode,
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
                Message = result ? OperationSucceededMessage : OperationFailedMessage,
                ExitCode = result ? SuccessExitCode : FailureExitCode,
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
                ExitCode = FailureExitCode,
                OperationTime = startTime,
                DurationMs = (long)duration.TotalMilliseconds
            };

            _logger.LogError(ex, "Operation {Operation} on {ServiceName} failed after {Duration}ms",
                operation, unitName, duration.TotalMilliseconds);
            throw new ServiceOperationException(unitName, operation, ex.Message, ex);
        }
    }
}
