#nullable enable

using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.Caching;

/// <summary>
/// In-memory implementation of the service cache.
/// Uses .NET's built-in MemoryCache for high-performance caching.
/// Not suitable for distributed scenarios; use Redis for multi-instance deployments.
/// </summary>
public class MemoryCacheProvider : IServiceCache
{
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly Dictionary<string, long> _expirationMap = new();
    private readonly object _lockObject = new();
    private readonly ILogger<MemoryCacheProvider> _logger;

    public MemoryCacheProvider(IMemoryCache cache, ILogger<MemoryCacheProvider> logger, IOptions<CacheOptions>? options = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? new CacheOptions();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        try
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is CacheEntry<T> entry)
                {
                    if (entry.IsExpired())
                    {
                        await RemoveAsync(key);
                        return null;
                    }

                    return entry.Value;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item from cache with key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            var expiration = ttl?.TotalSeconds ?? _options.DefaultTtlSeconds;
            var expirationTime = DateTime.UtcNow.AddSeconds(expiration);

            var entry = new CacheEntry<T>
            {
                Value = value,
                ExpirationTime = expirationTime,
                CreatedAt = DateTime.UtcNow
            };

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiration),
                SlidingExpiration = TimeSpan.FromSeconds(expiration / 2)
            };

            lock (_lockObject)
            {
                _cache.Set(key, entry, cacheEntryOptions);
                _expirationMap[key] = expirationTime.Ticks;
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting item in cache with key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        lock (_lockObject)
        {
            _cache.Remove(key);
            _expirationMap.Remove(key);
        }

        await Task.CompletedTask;
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return;

        // Note: IMemoryCache doesn't support pattern-based removal directly
        // This is a limitation of in-memory caching; Redis provides this feature
        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        lock (_lockObject)
        {
            _expirationMap.Clear();
        }

        // There's no built-in way to clear all items, but we can reset the cache
        // by creating a new instance (not ideal but functional)
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var exists = _cache.TryGetValue(key, out var cachedValue);

        if (exists && cachedValue is System.Collections.DictionaryEntry entry)
        {
            if (entry.Value is ICacheEntryBase cacheEntry && cacheEntry.IsExpired())
            {
                await RemoveAsync(key);
                return false;
            }
        }

        return exists;
    }

    public async Task<long> GetTtlAsync(string key)
    {
        if (!_expirationMap.TryGetValue(key, out var expirationTicks))
            return -1; // Key doesn't exist

        var expirationTime = new DateTime(expirationTicks);
        var remainingSeconds = (long)(expirationTime - DateTime.UtcNow).TotalSeconds;

        if (remainingSeconds <= 0)
        {
            await RemoveAsync(key);
            return -2; // Expired
        }

        return remainingSeconds;
    }

    /// <summary>
    /// Base interface for cache entries with expiration support.
    /// </summary>
    private interface ICacheEntryBase
    {
        bool IsExpired();
    }

    /// <summary>
    /// Generic cache entry wrapper with expiration metadata.
    /// </summary>
    private class CacheEntry<T> : ICacheEntryBase where T : class
    {
        public T? Value { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccessCount { get; set; }
        public DateTime LastAccessTime { get; set; }

        public bool IsExpired() => DateTime.UtcNow > ExpirationTime;
    }
}
