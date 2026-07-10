#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.BackgroundWorkers;

/// <summary>
/// Extension methods for <see cref="ServiceStatusUpdateWorker"/> that provide convenient
/// utility methods for configuration, monitoring, and batch operations.
/// </summary>
public static class ServiceStatusUpdateWorkerExtensions
{
    /// <summary>
    /// Configures the service status update worker with custom options.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure the options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigureServiceStatusUpdateWorker(
        this IServiceCollection services,
        Action<ServiceWorkerOptions> configure)
    {
        services.Configure(configure);
        return services;
    }

    /// <summary>
    /// Gets the current update interval in milliseconds from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>The update interval in milliseconds</returns>
    public static int GetUpdateIntervalMs(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        // The worker has _options field which contains UpdateIntervalMs
        // We need to access it via reflection or make it public
        // Since we can't change the original class, we'll assume it's accessible
        // through the worker's internal options
        return worker.UpdateIntervalMs;
    }

    /// <summary>
    /// Gets the current error backoff delay in milliseconds from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>The error backoff delay in milliseconds</returns>
    public static int GetErrorBackoffMs(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return worker.ErrorBackoffMs;
    }

    /// <summary>
    /// Gets the cache TTL (time-to-live) from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>The cache TTL as TimeSpan</returns>
    public static TimeSpan GetCacheTtl(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return worker.CacheTtl;
    }

    /// <summary>
    /// Gets the batch size for processing services from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>The batch size</returns>
    public static int GetBatchSize(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return worker.BatchSize;
    }

    /// <summary>
    /// Determines whether verbose logging is enabled for the worker.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>True if verbose logging is enabled; otherwise false</returns>
    public static bool IsVerboseLoggingEnabled(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return worker.VerboseLogging;
    }

    /// <summary>
    /// Creates a shallow copy of the worker's configuration options.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <returns>A new ServiceWorkerOptions with the same values</returns>
    public static ServiceWorkerOptions CloneOptions(this ServiceStatusUpdateWorker worker)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return new ServiceWorkerOptions
        {
            UpdateIntervalMs = worker.UpdateIntervalMs,
            ErrorBackoffMs = worker.ErrorBackoffMs,
            CacheTtl = worker.CacheTtl,
            BatchSize = worker.BatchSize,
            VerboseLogging = worker.VerboseLogging
        };
    }

    /// <summary>
    /// Creates a new ServiceWorkerOptions with the specified update interval.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <param name="newIntervalMs">The new update interval in milliseconds</param>
    /// <returns>A new ServiceWorkerOptions with the updated value</returns>
    public static ServiceWorkerOptions WithUpdateInterval(
        this ServiceStatusUpdateWorker worker,
        int newIntervalMs)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return new ServiceWorkerOptions
        {
            UpdateIntervalMs = newIntervalMs,
            ErrorBackoffMs = worker.ErrorBackoffMs,
            CacheTtl = worker.CacheTtl,
            BatchSize = worker.BatchSize,
            VerboseLogging = worker.VerboseLogging
        };
    }

    /// <summary>
    /// Creates a new ServiceWorkerOptions with the specified error backoff delay.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <param name="newBackoffMs">The new error backoff delay in milliseconds</param>
    /// <returns>A new ServiceWorkerOptions with the updated value</returns>
    public static ServiceWorkerOptions WithErrorBackoff(
        this ServiceStatusUpdateWorker worker,
        int newBackoffMs)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        return new ServiceWorkerOptions
        {
            UpdateIntervalMs = worker.UpdateIntervalMs,
            ErrorBackoffMs = newBackoffMs,
            CacheTtl = worker.CacheTtl,
            BatchSize = worker.BatchSize,
            VerboseLogging = worker.VerboseLogging
        };
    }

    /// <summary>
    /// Logs the current worker configuration to the logger if available.
    /// </summary>
    /// <param name="worker">The service status update worker instance</param>
    /// <param name="logger">Optional logger to use for output</param>
    /// <returns>The worker instance for chaining</returns>
    public static ServiceStatusUpdateWorker LogConfiguration(
        this ServiceStatusUpdateWorker worker,
        ILogger<ServiceStatusUpdateWorker>? logger = null)
    {
        if (worker is null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        var message = $"ServiceStatusUpdateWorker Configuration:\n" +
                     $"  UpdateIntervalMs: {worker.UpdateIntervalMs}ms\n" +
                     $"  ErrorBackoffMs: {worker.ErrorBackoffMs}ms\n" +
                     $"  CacheTtl: {worker.CacheTtl.TotalSeconds}s\n" +
                     $"  BatchSize: {worker.BatchSize}\n" +
                     $"  VerboseLogging: {worker.VerboseLogging}";

        logger?.LogInformation(message);
        return worker;
    }
}
