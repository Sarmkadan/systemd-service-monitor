# ServiceLogService

Centralizes access to systemd journal logs for managed services. Provides methods to query, store, clear, and analyze service logs with support for time ranges, log levels, and free-text search.

## API

### `public ServiceLogService`

Constructor. Initializes a new instance of the `ServiceLogService` class that connects to the local systemd journal and prepares storage for service logs.

### `public async Task<IEnumerable<ServiceLog>> GetServiceLogsAsync()`

Retrieves all stored service logs from the local database. This method does not interact with the systemd journal.

- **Returns**: An asynchronous enumerable of `ServiceLog` objects representing all stored logs.
- **Throws**: `InvalidOperationException` if the database connection is not available.

### `public async Task<IEnumerable<ServiceLog>> GetLogsInTimeRangeAsync(DateTimeOffset start, DateTimeOffset end)`

Retrieves stored service logs that fall within the specified time range.

- **Parameters**:
  - `start` – The inclusive start of the time range.
  - `end` – The exclusive end of the time range.
- **Returns**: An asynchronous enumerable of `ServiceLog` objects within the range.
- **Throws**: `ArgumentOutOfRangeException` if `start` is later than `end`; `InvalidOperationException` if the database connection is not available.

### `public async Task<IEnumerable<ServiceLog>> GetLogsByLevelAsync(LogLevel level)`

Retrieves stored service logs filtered by the specified log level.

- **Parameters**:
  - `level` – The `LogLevel` to filter by (e.g., `LogLevel.Error`).
- **Returns**: An asynchronous enumerable of `ServiceLog` objects matching the level.
- **Throws**: `InvalidOperationException` if the database connection is not available.

### `public async Task<IEnumerable<ServiceLog>> SearchLogsAsync(string query)`

Performs a free-text search across stored service logs.

- **Parameters**:
  - `query` – A search string to match against log message content.
- **Returns**: An asynchronous enumerable of `ServiceLog` objects whose messages contain the query string (case-insensitive).
- **Throws**: `ArgumentException` if `query` is null or whitespace; `InvalidOperationException` if the database connection is not available.

### `public async Task<IEnumerable<ServiceLog>> FetchLatestFromJournalAsync(int limit)`

Fetches the most recent service logs directly from the systemd journal.

- **Parameters**:
  - `limit` – Maximum number of entries to retrieve.
- **Returns**: An asynchronous enumerable of `ServiceLog` objects representing the latest journal entries.
- **Throws**: `ArgumentOutOfRangeException` if `limit` is less than 1.

### `public async Task<IEnumerable<ServiceLog>> FetchFromJournalByPriorityAsync(int limit)`

Fetches service logs from the systemd journal prioritized by severity.

- **Parameters**:
  - `limit` – Maximum number of entries to retrieve.
- **Returns**: An asynchronous enumerable of `ServiceLog` objects ordered by descending priority (errors first).
- **Throws**: `ArgumentOutOfRangeException` if `limit` is less than 1.

### `public async Task<IEnumerable<ServiceLog>> GetRecentLogsAsync(int count)`

Retrieves the most recently stored service logs from the local database.

- **Parameters**:
  - `count` – Number of recent logs to return.
- **Returns**: An asynchronous enumerable of the `count` most recent `ServiceLog` objects.
- **Throws**: `ArgumentOutOfRangeException` if `count` is less than 1; `InvalidOperationException` if the database connection is not available.

### `public async Task<ServiceLog> StoreLogAsync(ServiceLog log)`

Stores a single service log entry in the local database.

- **Parameters**:
  - `log` – The `ServiceLog` instance to store.
- **Returns**: The stored `ServiceLog` with any database-generated fields populated.
- **Throws**: `ArgumentNullException` if `log` is null; `InvalidOperationException` if the database connection is not available or the log already exists.

### `public async Task<int> StoreLogsAsync(IEnumerable<ServiceLog> logs)`

Stores multiple service log entries in the local database in a single transaction.

- **Parameters**:
  - `logs` – An enumerable of `ServiceLog` objects to store.
- **Returns**: The number of logs successfully stored.
- **Throws**: `ArgumentNullException` if `logs` is null; `InvalidOperationException` if the database connection is not available.

### `public async Task<int> ClearOldLogsAsync(TimeSpan retention)`

Removes stored service logs older than the specified retention period.

- **Parameters**:
  - `retention` – The age threshold; logs older than this are removed.
- **Returns**: The number of logs deleted.
- **Throws**: `ArgumentOutOfRangeException` if `retention` is negative or zero; `InvalidOperationException` if the database connection is not available.

### `public async Task<LogStatistics> GetLogStatisticsAsync()`

Computes summary statistics for stored service logs.

- **Returns**: A `LogStatistics` object containing counts by level, total entries, and date range.
- **Throws**: `InvalidOperationException` if the database connection is not available.

## Usage
