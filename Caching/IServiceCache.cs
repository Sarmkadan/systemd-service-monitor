// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Caching;

/// <summary>
/// Interface for service caching layer.
/// Provides abstraction for caching service data with TTL support.
/// </summary>
public interface IServiceCache
{
    /// <summary>
    /// Gets a cached value by key.
    /// Returns null if the key doesn't exist or has expired.
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a cached value with optional TTL.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all cached values matching a pattern.
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Clears the entire cache.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets the remaining TTL for a cached item in seconds.
    /// Returns -1 if the key doesn't exist, -2 if it has no expiration.
    /// </summary>
    Task<long> GetTtlAsync(string key);
}

/// <summary>
/// Cache configuration options.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Default TTL for cache entries in seconds.
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Maximum size of the cache in MB.
    /// </summary>
    public int MaxSizeMb { get; set; } = 100;

    /// <summary>
    /// Whether to compress values in cache.
    /// </summary>
    public bool UseCompression { get; set; } = false;

    /// <summary>
    /// Cache expiration policy (LRU, FIFO, etc.).
    /// </summary>
    public CacheEvictionPolicy EvictionPolicy { get; set; } = CacheEvictionPolicy.LRU;
}

/// <summary>
/// Cache eviction policies.
/// </summary>
public enum CacheEvictionPolicy
{
    /// <summary>
    /// Least Recently Used - removes least recently accessed items when cache is full.
    /// </summary>
    LRU,

    /// <summary>
    /// First In, First Out - removes oldest items when cache is full.
    /// </summary>
    FIFO,

    /// <summary>
    /// Least Frequently Used - removes least frequently accessed items.
    /// </summary>
    LFU
}
