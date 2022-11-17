# IServiceCache

Provides a cache for service state snapshots with configurable size, lifetime, and eviction behavior. Implementations are expected to be thread-safe and support optional compression of cached entries.

## API

### `public int DefaultTtlSeconds`

Gets or sets the default time-to-live (in seconds) for cached service state snapshots when no explicit TTL is provided. Must be a non-negative value; setting a negative value throws an `ArgumentOutOfRangeException`.

### `public int MaxSizeMb`

Gets or sets the maximum allowed cache size in megabytes. Must be a non-negative value; setting a negative value throws an `ArgumentOutOfRangeException`. Implementations may evict entries when this limit is exceeded, depending on the configured `EvictionPolicy`.

### `public bool UseCompression`

Gets or sets whether cached service state snapshots should be compressed (e.g., using GZip) to reduce memory usage. Changing this value may trigger immediate re-compression or eviction of existing entries, depending on implementation.

### `public CacheEvictionPolicy EvictionPolicy`

Gets or sets the eviction policy that determines which entries are removed when the cache reaches `MaxSizeMb` or when entries exceed their TTL. Must be one of the defined `CacheEvictionPolicy` values; setting an invalid value throws an `ArgumentOutOfRangeException`.

## Usage
