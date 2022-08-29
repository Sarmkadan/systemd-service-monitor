// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.Json;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Middleware;

/// <summary>
/// Token bucket rate limiting middleware that prevents API abuse.
/// Tracks requests per IP address and enforces configurable rate limits.
/// </summary>
public class RateLimitingMiddleware(
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
            context.Response.Headers.Add("Retry-After", "60");

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

        await context.Next();
    }

    /// <summary>
    /// Token bucket for implementing rate limiting.
    /// Refills tokens at a fixed interval.
    /// </summary>
    private class TokenBucket
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
    }
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
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        RateLimitOptions? options = null)
    {
        var opts = options ?? new RateLimitOptions();
        return app.UseMiddleware<RateLimitingMiddleware>(opts);
    }

    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        Action<RateLimitOptions> configure)
    {
        var options = new RateLimitOptions();
        configure(options);
        return app.UseRateLimiting(options);
    }
}
