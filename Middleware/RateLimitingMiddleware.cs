#nullable enable

using System.Collections.Concurrent;
using System.Text.Json;
using SystemdServiceMonitor.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.Middleware;

/// <summary>
/// Token bucket rate limiting middleware that prevents API abuse.
/// Tracks requests per IP address and enforces configurable rate limits.
/// </summary>
public class RateLimitingMiddleware(
    RequestDelegate next,
    ILogger<RateLimitingMiddleware> logger,
    RateLimitOptions options)
{
    // Store request tokens per IP address
    private static readonly ConcurrentDictionary<string, TokenBucket> TokenBuckets = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Get or create token bucket for this IP
        var bucket = TokenBuckets.GetOrAdd(ipAddress, _ => new TokenBucket(
            options.RequestsPerMinute,
            options.RefillIntervalSeconds));

        if (!bucket.TryConsumeToken())
        {
            logger.LogWarning("Rate limit exceeded for IP: {IpAddress}", ipAddress);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers["Retry-After"] = "60";

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Rate limit exceeded",
                ErrorDetails = $"Maximum {options.RequestsPerMinute} requests per minute allowed"
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsJsonAsync(response, jsonOptions);
            return;
        }

        logger.LogDebug("Rate limiting token consumed for IP {IpAddress}. Remaining tokens: {Remaining}",
            ipAddress, bucket.RemainingTokens);

        await next(context);
    }
}

/// <summary>
/// Token bucket for implementing rate limiting.
/// Refills tokens at a fixed interval.
/// </summary>
public class TokenBucket
{
    private readonly int _capacity;
    private readonly int _refillIntervalSeconds;
    private int _tokens;
    private DateTime _lastRefillTime;

    public TokenBucket(int capacity, int refillIntervalSeconds)
    {
        _capacity = capacity;
        _tokens = capacity;
        _refillIntervalSeconds = refillIntervalSeconds;
        _lastRefillTime = DateTime.UtcNow;
    }

    public bool TryConsumeToken()
    {
        RefillTokens();

        if (_tokens > 0)
        {
            _tokens--;
            return true;
        }

        return false;
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timeSinceLastRefill = (now - _lastRefillTime).TotalSeconds;

        if (timeSinceLastRefill >= _refillIntervalSeconds)
        {
            _tokens = _capacity;
            _lastRefillTime = now;
        }
    }

    // Expose remaining tokens for debug logging
    public int RemainingTokens => _tokens;

    // Expose configuration for statistics
    public int RequestsPerMinute => _capacity;
    public int RefillIntervalSeconds => _refillIntervalSeconds;
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed per minute per IP address.
    /// </summary>
    public int RequestsPerMinute { get; set; } = 300;

    /// <summary>
    /// Interval in seconds at which tokens are refilled.
    /// </summary>
    public int RefillIntervalSeconds { get; set; } = 60;
}

/// <summary>
/// Extension methods for rate limiting middleware registration.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware with default configuration (300 requests per minute).
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiting(new RateLimitOptions());
    }

    /// <summary>
    /// Adds rate limiting middleware with custom configuration.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="options">Rate limit configuration options</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        RateLimitOptions options)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return app.UseMiddleware<RateLimitingMiddleware>(options);
    }

    /// <summary>
    /// Adds rate limiting middleware with configuration via action delegate.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configure">Action to configure rate limit options</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        Action<RateLimitOptions> configure)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var options = new RateLimitOptions();
        configure(options);
        return app.UseRateLimiting(options);
    }

    /// <summary>
    /// Adds rate limiting middleware with configuration from IOptions pattern.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <typeparam name="TOptions">Rate limit options type</typeparam>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting<TOptions>(
        this IApplicationBuilder app)
        where TOptions : class, IRateLimitOptions
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var options = app.ApplicationServices.GetRequiredService<IOptions<TOptions>>().Value;
        return app.UseMiddleware<RateLimitingMiddleware>(options);
    }

    /// <summary>
    /// Adds rate limiting middleware with configuration from IOptions pattern and optional override.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configure">Optional action to override configured options</param>
    /// <typeparam name="TOptions">Rate limit options type</typeparam>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRateLimiting<TOptions>(
        this IApplicationBuilder app,
        Action<TOptions>? configure = null)
        where TOptions : class, IRateLimitOptions, new()
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var options = new TOptions();
        var configuredOptions = app.ApplicationServices.GetService<IOptions<TOptions>>()?.Value ?? options;

        configure?.Invoke(configuredOptions);

        return app.UseMiddleware<RateLimitingMiddleware>(configuredOptions);
    }

    /// <summary>
    /// Gets the current token bucket for a specific IP address.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance</param>
    /// <param name="ipAddress">The IP address to check</param>
    /// <returns>The token bucket if found, null otherwise</returns>
    public static TokenBucket? GetTokenBucketForIp(
        this RateLimitingMiddleware middleware,
        string ipAddress)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new ArgumentException("IP address cannot be null or empty", nameof(ipAddress));
        }

        // Access the private static field via reflection
        var field = typeof(RateLimitingMiddleware).GetField(
            "TokenBuckets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (field?.GetValue(null) is ConcurrentDictionary<string, TokenBucket> buckets)
        {
            buckets.TryGetValue(ipAddress, out var bucket);
            return bucket;
        }

        return null;
    }

    /// <summary>
    /// Gets statistics about rate limiting for all IP addresses.
    /// </summary>
    /// <param name="middleware">The rate limiting middleware instance</param>
    /// <returns>Dictionary of IP addresses to their token bucket statistics</returns>
    public static Dictionary<string, TokenBucketStats> GetRateLimitStatistics(
        this RateLimitingMiddleware middleware)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        var field = typeof(RateLimitingMiddleware).GetField(
            "TokenBuckets",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        var stats = new Dictionary<string, TokenBucketStats>();

        if (field?.GetValue(null) is ConcurrentDictionary<string, TokenBucket> buckets)
        {
            foreach (var kvp in buckets)
            {
                stats[kvp.Key] = new TokenBucketStats
                {
                    Capacity = kvp.Value.RequestsPerMinute,
                    RemainingTokens = kvp.Value.RemainingTokens,
                    RefillIntervalSeconds = kvp.Value.RefillIntervalSeconds
                };
            }
        }

        return stats;
    }
}

/// <summary>
/// Statistics about a token bucket.
/// </summary>
public class TokenBucketStats
{
    /// <summary>
    /// The maximum capacity of the token bucket (requests per minute).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// The number of remaining tokens in the bucket.
    /// </summary>
    public int RemainingTokens { get; set; }

    /// <summary>
    /// The interval in seconds at which tokens are refilled.
    /// </summary>
    public int RefillIntervalSeconds { get; set; }
}

/// <summary>
/// Interface for rate limit options to support generic configuration.
/// </summary>
public interface IRateLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed per minute per IP address.
    /// </summary>
    int RequestsPerMinute { get; set; }

    /// <summary>
    /// Interval in seconds at which tokens are refilled.
    /// </summary>
    int RefillIntervalSeconds { get; set; }
}