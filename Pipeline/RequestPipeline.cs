// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Pipeline;

/// <summary>
/// Request handler delegate that processes a request and returns a result.
/// </summary>
public delegate Task<TResult> RequestHandler<in TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
    where TRequest : class
    where TResult : class;

/// <summary>
/// Request handler middleware that can wrap other handlers.
/// Enables building a pipeline of handlers for cross-cutting concerns.
/// </summary>
public delegate Task<TResult> RequestPipelineStep<in TRequest, TResult>(
    TRequest request,
    RequestHandler<TRequest, TResult> next,
    CancellationToken cancellationToken = default)
    where TRequest : class
    where TResult : class;

/// <summary>
/// Builder for constructing request pipelines with multiple middleware steps.
/// Implements the Chain of Responsibility pattern for request processing.
/// </summary>
public class RequestPipelineBuilder<TRequest, TResult>
    where TRequest : class
    where TResult : class
{
    private readonly List<RequestPipelineStep<TRequest, TResult>> _steps = new();
    private readonly ILogger<RequestPipelineBuilder<TRequest, TResult>> _logger;

    public RequestPipelineBuilder(ILogger<RequestPipelineBuilder<TRequest, TResult>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds a middleware step to the pipeline.
    /// Steps are executed in the order they are added.
    /// </summary>
    public RequestPipelineBuilder<TRequest, TResult> Use(RequestPipelineStep<TRequest, TResult> middleware)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        _steps.Add(middleware);
        return this;
    }

    /// <summary>
    /// Adds a validation middleware that validates the request before processing.
    /// </summary>
    public RequestPipelineBuilder<TRequest, TResult> UseValidation(
        Func<TRequest, Task<(bool IsValid, string ErrorMessage)>> validator)
    {
        if (validator == null)
            throw new ArgumentNullException(nameof(validator));

        return Use(async (request, next, ct) =>
        {
            var (isValid, errorMessage) = await validator(request);

            if (!isValid)
            {
                _logger.LogWarning("Validation failed: {ErrorMessage}", errorMessage);
                throw new InvalidOperationException($"Validation failed: {errorMessage}");
            }

            return await next(request, ct);
        });
    }

    /// <summary>
    /// Adds a logging middleware that logs request and response information.
    /// </summary>
    public RequestPipelineBuilder<TRequest, TResult> UseLogging(string operationName)
    {
        return Use(async (request, next, ct) =>
        {
            _logger.LogInformation("Pipeline {OperationName} started", operationName);

            try
            {
                var result = await next(request, ct);
                _logger.LogInformation("Pipeline {OperationName} completed successfully", operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline {OperationName} failed", operationName);
                throw;
            }
        });
    }

    /// <summary>
    /// Adds a caching middleware that caches results.
    /// </summary>
    public RequestPipelineBuilder<TRequest, TResult> UseCaching(
        Func<TRequest, string> cacheKeySelector,
        TimeSpan? cacheDuration = null)
    {
        if (cacheKeySelector == null)
            throw new ArgumentNullException(nameof(cacheKeySelector));

        return Use(async (request, next, ct) =>
        {
            var cacheKey = cacheKeySelector(request);
            _logger.LogDebug("Checking cache for key: {CacheKey}", cacheKey);

            // In a real implementation, check cache here
            // if (cache.TryGetValue(cacheKey, out var cachedResult))
            //     return (TResult)cachedResult;

            var result = await next(request, ct);

            // Store in cache
            // cache.Set(cacheKey, result, cacheDuration);

            return result;
        });
    }

    /// <summary>
    /// Adds an exception handling middleware.
    /// </summary>
    public RequestPipelineBuilder<TRequest, TResult> UseExceptionHandling(
        Func<Exception, Task<TResult>>? errorHandler = null)
    {
        return Use(async (request, next, ct) =>
        {
            try
            {
                return await next(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in pipeline");

                if (errorHandler != null)
                {
                    return await errorHandler(ex);
                }

                throw;
            }
        });
    }

    /// <summary>
    /// Builds the pipeline and returns a handler function.
    /// </summary>
    public RequestHandler<TRequest, TResult> Build(RequestHandler<TRequest, TResult> innerHandler)
    {
        if (innerHandler == null)
            throw new ArgumentNullException(nameof(innerHandler));

        // Build pipeline by wrapping handlers in reverse order
        RequestHandler<TRequest, TResult> pipeline = innerHandler;

        for (int i = _steps.Count - 1; i >= 0; i--)
        {
            var step = _steps[i];
            var next = pipeline;

            pipeline = (request, ct) => step(request, next, ct);
        }

        return pipeline;
    }
}

/// <summary>
/// Request pipeline execution context.
/// Carries contextual information through the pipeline.
/// </summary>
public class PipelineContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    public Dictionary<string, object> Items { get; set; } = new();

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public TimeSpan ElapsedTime => DateTime.UtcNow - StartTime;

    public void SetItem(string key, object value) => Items[key] = value;

    public T? GetItem<T>(string key) where T : class =>
        Items.TryGetValue(key, out var value) ? value as T : null;
}
