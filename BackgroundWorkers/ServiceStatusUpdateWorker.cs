#nullable enable

using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.BackgroundWorkers;

/// <summary>
/// Background worker that periodically updates the status of all monitored services.
/// Runs at configurable intervals to keep service status information fresh.
/// </summary>
public class ServiceStatusUpdateWorker : BackgroundService
{
    private readonly ILogger<ServiceStatusUpdateWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceWorkerOptions _options;

    public ServiceStatusUpdateWorker(
        ILogger<ServiceStatusUpdateWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<ServiceWorkerOptions>? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? new ServiceWorkerOptions();
    }

    /// <summary>
    /// Gets the update interval in milliseconds from the configuration.
    /// </summary>
    public int UpdateIntervalMs => _options.UpdateIntervalMs;

    /// <summary>
    /// Gets the error backoff delay in milliseconds from the configuration.
    /// </summary>
    public int ErrorBackoffMs => _options.ErrorBackoffMs;

    /// <summary>
    /// Gets the cache TTL (time-to-live) from the configuration.
    /// </summary>
    public TimeSpan CacheTtl => _options.CacheTtl;

    /// <summary>
    /// Gets the batch size for processing services from the configuration.
    /// </summary>
    public int BatchSize => _options.BatchSize;

    /// <summary>
    /// Gets a value indicating whether verbose logging is enabled from the configuration.
    /// </summary>
    public bool VerboseLogging => _options.VerboseLogging;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ServiceStatusUpdateWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Resolve services within scope for DI
                    // var monitorService = scope.ServiceProvider.GetRequiredService<IServiceMonitorService>();
                    // var cache = scope.ServiceProvider.GetRequiredService<IServiceCache>();

                    // var services = await monitorService.GetAllServicesAsync();
                    // _logger.LogDebug("Updated status for {ServiceCount} services", services.Count);

                    // Cache the results for quick API access
                    // await cache.SetAsync("all-services", services, _options.CacheTtl);
                }

                await Task.Delay(_options.UpdateIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ServiceStatusUpdateWorker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ServiceStatusUpdateWorker");
                // Continue after error, with backoff
                await Task.Delay(_options.ErrorBackoffMs, stoppingToken);
            }
        }

        _logger.LogInformation("ServiceStatusUpdateWorker stopped");
    }
}

/// <summary>
/// Configuration options for background service workers.
/// </summary>
public class ServiceWorkerOptions
{
    /// <summary>
    /// Interval in milliseconds between status updates.
    /// Default: 30 seconds
    /// </summary>
    public int UpdateIntervalMs { get; set; } = 30000;

    /// <summary>
    /// Backoff delay in milliseconds after an error.
    /// Default: 10 seconds
    /// </summary>
    public int ErrorBackoffMs { get; set; } = 10000;

    /// <summary>
    /// TTL for cached data.
    /// Default: 5 minutes
    /// </summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of services to process in one batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Enable detailed logging of worker operations.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
}
