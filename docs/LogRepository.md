# LogRepository

`LogRepository` provides persistence and query operations for `ServiceLog` entities within the `systemd-service-monitor` project. It abstracts the underlying data store, offering asynchronous methods to retrieve logs by various criteria (identifier, unit name, service ID, severity level, process ID, recency, or free-text search), create single or batch entries, delete individual or time-based expired records, and obtain aggregate counts.

## API

### GetByIdAsync
```csharp
public async Task<ServiceLog?> GetByIdAsync(int id)
```
Retrieves a single `ServiceLog` by its unique integer identifier.  
**Parameters:** `id` – the primary key of the log entry.  
**Returns:** the matching `ServiceLog` instance, or `null` if no record with that `id` exists.  
**Throws:** `InvalidOperationException` or data-store-specific exceptions when the underlying connection or query fails.

### GetByUnitNameAsync
```csharp
public async Task<IEnumerable<ServiceLog>> GetByUnitNameAsync(string unitName)
```
Returns all log entries associated with a given systemd unit name (e.g., `"nginx.service"`).  
**Parameters:** `unitName` – case-sensitive unit name string; must not be `null`.  
**Returns:** an enumerable collection of `ServiceLog` records; empty if none match.  
**Throws:** `ArgumentNullException` when `unitName` is `null`; data-store exceptions on infrastructure failures.

### GetByServiceIdAsync
```csharp
public async Task<IEnumerable<ServiceLog>> GetByServiceIdAsync(int serviceId)
```
Returns all log entries linked to a specific service by its internal identifier.  
**Parameters:** `serviceId` – the foreign-key identifier of the monitored service.  
**Returns:** matching `ServiceLog` entries; empty collection when no logs exist for that service.  
**Throws:** data-store exceptions when the query cannot be executed.

### GetByLevelAsync
```csharp
public async Task<IEnumerable<ServiceLog>> GetByLevelAsync(string level)
```
Returns log entries filtered by severity level (e.g., `"error"`, `"warning"`, `"info"`).  
**Parameters:** `level` – case-insensitive level string; must not be `null`.  
**Returns:** collection of `ServiceLog` records with the specified level; empty if none match.  
**Throws:** `ArgumentNullException` when `level` is `null`; data-store exceptions on query failure.

### GetRecentAsync
```csharp
public async Task<IEnumerable<ServiceLog>> GetRecentAsync(int count)
```
Returns the most recent log entries up to the specified count, ordered by timestamp descending.  
**Parameters:** `count` – maximum number of records to return; must be non-negative.  
**Returns:** up to `count` `ServiceLog` records; fewer if the store contains less than `count` entries.  
**Throws:** `ArgumentOutOfRangeException` when `count` is negative; data-store exceptions on query failure.

### GetByProcessIdAsync
```csharp
public async Task<IEnumerable<ServiceLog>> GetByProcessIdAsync(int processId)
```
Returns log entries originating from a specific OS process identifier.  
**Parameters:** `processId` – the PID recorded with the log entry.  
**Returns:** matching `ServiceLog` records; empty collection when no logs reference that PID.  
**Throws:** data-store exceptions when the query cannot be executed.

### CreateAsync
```csharp
public async Task<ServiceLog> CreateAsync(ServiceLog log)
```
Persists a new `ServiceLog` entry and returns the entity with any store-generated values (such as the assigned primary key) populated.  
**Parameters:** `log` – a fully populated `ServiceLog` instance except for store-generated fields; must not be `null`.  
**Returns:** the created `ServiceLog` with its `Id` and possibly timestamps updated.  
**Throws:** `ArgumentNullException` when `log` is `null`; `InvalidOperationException` or data-store exceptions on persistence failure.

### CreateBatchAsync
```csharp
public async Task<int> CreateBatchAsync(IEnumerable<ServiceLog> logs)
```
Persists multiple `ServiceLog` entries in a single operation.  
**Parameters:** `logs` – a collection of `ServiceLog` instances to insert; must not be `null`.  
**Returns:** the number of records successfully inserted.  
**Throws:** `ArgumentNullException` when `logs` is `null`; data-store exceptions on batch persistence failure (partial success behaviour depends on the underlying store’s transactional guarantees).

### DeleteAsync
```csharp
public async Task<bool> DeleteAsync(int id)
```
Removes a single log entry by its unique identifier.  
**Parameters:** `id` – the primary key of the log to delete.  
**Returns:** `true` if a record was found and deleted; `false` if no record with that `id` exists.  
**Throws:** data-store exceptions when the delete operation cannot be completed.

### DeleteOlderThanAsync
```csharp
public async Task<int> DeleteOlderThanAsync(DateTime cutoff)
```
Removes all log entries whose timestamp is strictly older than the specified cutoff.  
**Parameters:** `cutoff` – the exclusive threshold `DateTime`; entries with a timestamp before this value are deleted.  
**Returns:** the number of records deleted.  
**Throws:** data-store exceptions when the bulk delete operation fails.

### GetCountAsync
```csharp
public async Task<int> GetCountAsync()
```
Returns the total number of `ServiceLog` entries currently stored.  
**Returns:** the integer count of all log records.  
**Throws:** data-store exceptions when the aggregate query fails.

### SearchAsync
```csharp
public async Task<IEnumerable<ServiceLog>> SearchAsync(string query)
```
Performs a free-text search across log message content and possibly other textual fields.  
**Parameters:** `query` – the search string; must not be `null`.  
**Returns:** an enumerable collection of `ServiceLog` records whose text matches the query; empty if no matches are found.  
**Throws:** `ArgumentNullException` when `query` is `null`; data-store exceptions on search execution failure.

## Usage

### Example 1: Retrieving recent errors for a specific unit
```csharp
var repository = new LogRepository(connectionFactory);

// Fetch the last 20 error-level logs for nginx.service
IEnumerable<ServiceLog> nginxLogs = await repository.GetByUnitNameAsync("nginx.service");
IEnumerable<ServiceLog> recentErrors = nginxLogs
    .Where(log => log.Level.Equals("error", StringComparison.OrdinalIgnoreCase))
    .OrderByDescending(log => log.Timestamp)
    .Take(20);

foreach (var entry in recentErrors)
{
    Console.WriteLine($"[{entry.Timestamp:O}] {entry.Message}");
}
```

### Example 2: Bulk insertion and periodic cleanup
```csharp
var repository = new LogRepository(connectionFactory);

// Batch-insert a collection of parsed journal entries
var newEntries = parsedJournalEntries.Select(e => new ServiceLog
{
    ServiceId = e.ServiceId,
    UnitName = e.UnitName,
    Level = e.Level,
    Message = e.Message,
    ProcessId = e.ProcessId,
    Timestamp = e.Timestamp
}).ToList();

int inserted = await repository.CreateBatchAsync(newEntries);
Console.WriteLine($"Inserted {inserted} log entries.");

// Remove logs older than 30 days
var cutoff = DateTime.UtcNow.AddDays(-30);
int removed = await repository.DeleteOlderThanAsync(cutoff);
Console.WriteLine($"Purged {removed} expired entries.");

int remaining = await repository.GetCountAsync();
Console.WriteLine($"Remaining total: {remaining}");
```

## Notes

- **Null handling:** Methods accepting reference-type parameters (`unitName`, `level`, `log`, `logs`, `query`) throw `ArgumentNullException` when passed `null`. Callers must guard or validate inputs before invocation.
- **Empty results:** Query methods (`GetByUnitNameAsync`, `GetByServiceIdAsync`, `GetByLevelAsync`, `GetByProcessIdAsync`, `SearchAsync`) return empty enumerables, never `null`, when no records match. `GetByIdAsync` returns `null` for a missing identifier.
- **Batch insertion atomicity:** `CreateBatchAsync` returns the count of inserted rows. The underlying store may or may not provide full atomicity; partial failure scenarios depend on the data-store implementation and transaction configuration. Callers should not assume rollback on error unless explicitly configured externally.
- **Time-based deletion:** `DeleteOlderThanAsync` uses an exclusive cutoff (`timestamp < cutoff`). Entries exactly at the cutoff are retained. The `DateTime` kind (UTC vs. local) must match the stored timestamp kind to avoid unintended retention or deletion.
- **Thread safety:** `LogRepository` itself does not guarantee thread safety from its signatures. Concurrent calls to mutation methods (`CreateAsync`, `CreateBatchAsync`, `DeleteAsync`, `DeleteOlderThanAsync`) should be externally synchronised if the underlying connection or session is not thread-safe. Read-only methods may be invoked concurrently if the data store supports it.
- **Resource management:** The repository likely holds or obtains disposable resources (connections, sessions). Callers should manage the repository’s lifecycle appropriately, disposing it when no longer needed, or ensuring the injected dependencies handle cleanup.
