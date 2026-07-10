# MemoryCacheProvider
The `MemoryCacheProvider` class is designed to provide a caching mechanism that stores data in memory, allowing for fast retrieval and manipulation of cached items. It offers a range of methods for getting, setting, removing, and checking the existence of cache entries, as well as properties to inspect the state of cached items.

## API
### Constructors
* `public MemoryCacheProvider`: Initializes a new instance of the `MemoryCacheProvider` class.

### Methods
* `public async Task<T?> GetAsync<T>`: Retrieves a cached item of type `T` asynchronously. Returns the cached item, or `null` if it does not exist.
* `public async Task SetAsync<T>`: Sets a cached item of type `T` asynchronously.
* `public async Task RemoveAsync`: Removes a cached item asynchronously.
* `public async Task RemoveByPatternAsync`: Removes cached items that match a specified pattern asynchronously.
* `public async Task ClearAsync`: Clears all cached items asynchronously.
* `public async Task<bool> ExistsAsync`: Checks if a cached item exists asynchronously. Returns `true` if the item exists, `false` otherwise.
* `public async Task<long> GetTtlAsync`: Retrieves the time-to-live (TTL) of a cached item asynchronously.

### Properties
* `public T? Value`: Gets the value of the cached item.
* `public DateTime ExpirationTime`: Gets the expiration time of the cached item.
* `public DateTime CreatedAt`: Gets the time when the cached item was created.
* `public int AccessCount`: Gets the number of times the cached item has been accessed.
* `public DateTime LastAccessTime`: Gets the time when the cached item was last accessed.
* `public bool IsExpired`: Gets a value indicating whether the cached item has expired.

## Usage
The following examples demonstrate how to use the `MemoryCacheProvider` class:
```csharp
// Example 1: Basic caching
var cache = new MemoryCacheProvider();
await cache.SetAsync("Hello, World!");
var cachedValue = await cache.GetAsync<string>();
Console.WriteLine(cachedValue); // Output: Hello, World!

// Example 2: Using TTL and expiration
var cache = new MemoryCacheProvider();
await cache.SetAsync("Hello, World!", ttl: 30); // 30-second TTL
var ttl = await cache.GetTtlAsync();
Console.WriteLine(ttl); // Output: 30
await Task.Delay(31000); // Wait for expiration
var isExpired = cache.IsExpired;
Console.WriteLine(isExpired); // Output: True
```

## Notes
* The `MemoryCacheProvider` class is designed for use in a single-process environment. In a multi-process or distributed environment, a more robust caching solution may be required.
* The class is not thread-safe by default. If used in a multi-threaded environment, appropriate synchronization mechanisms should be employed to ensure thread safety.
* The `RemoveByPatternAsync` method uses a pattern-matching approach to remove cached items. The exact pattern-matching logic is not specified here, but it is assumed to follow standard pattern-matching rules.
* The `GetTtlAsync` method returns the TTL of the cached item in seconds. A TTL of 0 indicates that the item does not expire.
* The `IsExpired` property is updated asynchronously when the cached item expires. However, it may not reflect the exact expiration time due to the asynchronous nature of the caching mechanism.
