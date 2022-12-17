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
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection ConfigureServiceStatusUpdateWorker(
        this IServiceCollection services,
        Action<ServiceWorkerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        return services;
    }

    /// <summary>
    /// Gets the current update interval in milliseconds from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>The update interval in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static int GetUpdateIntervalMs(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.UpdateIntervalMs;
    }

    /// <summary>
    /// Gets the current error backoff delay in milliseconds from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>The error backoff delay in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static int GetErrorBackoffMs(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.ErrorBackoffMs;
    }

    /// <summary>
    /// Gets the cache TTL (time-to-live) from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>The cache TTL as TimeSpan.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static TimeSpan GetCacheTtl(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.CacheTtl;
    }

    /// <summary>
    /// Gets the batch size for processing services from the worker's configuration.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>The batch size.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static int GetBatchSize(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.BatchSize;
    }

    /// <summary>
    /// Determines whether verbose logging is enabled for the worker.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>True if verbose logging is enabled; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static bool IsVerboseLoggingEnabled(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.VerboseLogging;
    }

    /// <summary>
    /// Creates a shallow copy of the worker's configuration options.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <returns>A new <see cref="ServiceWorkerOptions"/> with the same values.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static ServiceWorkerOptions CloneOptions(this ServiceStatusUpdateWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

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
    /// Creates a new <see cref="ServiceWorkerOptions"/> with the specified update interval.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <param name="newIntervalMs">The new update interval in milliseconds.</param>
    /// <returns>A new <see cref="ServiceWorkerOptions"/> with the updated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static ServiceWorkerOptions WithUpdateInterval(
        this ServiceStatusUpdateWorker worker,
        int newIntervalMs)
    {
        ArgumentNullException.ThrowIfNull(worker);

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
    /// Creates a new <see cref="ServiceWorkerOptions"/> with the specified error backoff delay.
    /// </summary>
    /// <param name="worker">The service status update worker instance.</param>
    /// <param name="newBackoffMs">The new error backoff delay in milliseconds.</param>
    /// <returns>A new <see cref="ServiceWorkerOptions"/> with the updated value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static ServiceWorkerOptions WithErrorBackoff(
        this ServiceStatusUpdateWorker worker,
        int newBackoffMs)
    {
        ArgumentNullException.ThrowIfNull(worker);

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
    /// <param name="worker">The service status update worker instance.</param>
    /// <param name="logger">Optional logger to use for output.</param>
    /// <returns>The worker instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="worker"/> is null.</exception>
    public static ServiceStatusUpdateWorker LogConfiguration(
        this ServiceStatusUpdateWorker worker,
        ILogger<ServiceStatusUpdateWorker>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(worker);

        var message = $"ServiceStatusUpdateWorker Configuration:{Environment.NewLine}" +
                     $"  UpdateIntervalMs: {worker.UpdateIntervalMs}ms{Environment.NewLine}" +
                     $"  ErrorBackoffMs: {worker.ErrorBackoffMs}ms{Environment.NewLine}" +
                     $"  CacheTtl: {worker.CacheTtl.TotalSeconds}s{Environment.NewLine}" +
                     $"  BatchSize: {worker.BatchSize}{Environment.NewLine}" +
                     $"  VerboseLogging: {worker.VerboseLogging}";

        logger?.LogInformation(message);
        return worker;
    }
}
