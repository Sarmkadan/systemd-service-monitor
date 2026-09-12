# ILogRepository Interface and LogRepository Implementation

## Overview

This document describes the `ILogRepository` interface and its `LogRepository` implementation in the `SystemdServiceMonitor.Data.Repositories` namespace.

The repository pattern is used to abstract data access operations for `ServiceLog` entities, providing a clean separation between the business logic and data storage concerns.

## ILogRepository Interface

The `ILogRepository` interface defines the contract for service log data access operations, including CRUD operations and various query methods.

### Methods

| Method | Description |
|--------|-------------|
| `Task<ServiceLog?> GetByIdAsync(Guid id, CancellationToken ct = default)` | Retrieves a service log by its unique identifier. |
| `Task<IEnumerable<ServiceLog>> GetByUnitNameAsync(string unitName, int limit = 100, CancellationToken ct = default)` | Retrieves service logs for a specific unit name, ordered by timestamp descending, with an optional limit. |
| `Task<IEnumerable<ServiceLog>> GetByServiceIdAsync(Guid serviceId, int limit = 100, CancellationToken ct = default)` | Retrieves service logs for a specific service ID, ordered by timestamp descending, with an optional limit. |
| `Task<IEnumerable<ServiceLog>> GetByLevelAsync(SyslogLevel level, CancellationToken ct = default)` | Retrieves all service logs for a specific syslog level, ordered by timestamp descending. |
| `Task<IEnumerable<ServiceLog>> GetRecentAsync(TimeSpan timeRange, CancellationToken ct = default)` | Retrieves service logs from the last specified time range, ordered by timestamp descending. |
| `Task<IEnumerable<ServiceLog>> GetByProcessIdAsync(int processId, CancellationToken ct = default)` | Retrieves all service logs for a specific process ID, ordered by timestamp descending. |
| `Task<ServiceLog> CreateAsync(ServiceLog log, CancellationToken ct = default)` | Creates a new service log record. |
| `Task<int> CreateBatchAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default)` | Creates multiple service log records in a batch and returns the count of created logs. |
| `Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)` | Deletes a service log by its identifier. |
| `Task<int> DeleteOlderThanAsync(DateTime before, CancellationToken ct = default)` | Deletes all service logs older than the specified date and returns the count of deleted logs. |
| `Task<int> GetCountAsync(CancellationToken ct = default)` | Gets the total number of service logs. |
| `Task<IEnumerable<ServiceLog>> SearchAsync(string searchTerm, CancellationToken ct = default)` | Searches service logs by message or unit name (case-insensitive) and returns results ordered by timestamp descending. |

### Thread Safety

All methods are designed to be thread-safe for concurrent access. The implementation uses a `SemaphoreSlim` to synchronize access to the underlying data store.

## LogRepository Implementation

The `LogRepository` class provides an in-memory implementation of the `ILogRepository` interface. It stores service log data in a dictionary and is intended for use in scenarios where a lightweight, non-persistent data store is acceptable (e.g., testing, prototyping, or when logs are managed entirely in memory).

### Key Features

- **In-Memory Storage**: Uses a `Dictionary<Guid, ServiceLog>` to store service log records.
- **Thread Safety**: All operations are protected by a `SemaphoreSlim` to ensure safe concurrent access.
- **Automatic Ordering**: Query methods return logs ordered by timestamp descending (most recent first) unless otherwise specified.
- **Batch Operations**: Supports creating multiple logs in a single batch operation.
- **Flexible Querying**: Supports filtering by various properties (unit name, service ID, level, process ID, time range) and text search.

### Usage Notes

This implementation is suitable for:
- Development and testing environments
- Scenarios where log data is transient and does not require persistence
- Situations where the entire dataset can comfortably fit in memory

For production scenarios requiring persistent storage, a different implementation (e.g., using Entity Framework Core) would be more appropriate.

### Methods Implementation Details

- **GetByIdAsync**: Looks up a service log by its ID in the dictionary.
- **GetByUnitNameAsync**: Filters logs by unit name, orders by timestamp descending, and applies limit.
- **GetByServiceIdAsync**: Filters logs by service ID, orders by timestamp descending, and applies limit.
- **GetByLevelAsync**: Filters logs by syslog level and orders by timestamp descending.
- **GetRecentAsync**: Filters logs by timestamp within the specified time range and orders by timestamp descending.
- **GetByProcessIdAsync**: Filters logs by process ID and orders by timestamp descending.
- **CreateAsync**: Adds a new service log to the dictionary.
- **CreateBatchAsync**: Adds multiple service logs to the dictionary in a loop and returns the count.
- **DeleteAsync**: Removes a service log by ID from the dictionary.
- **DeleteOlderThanAsync**: Identifies logs older than the specified date, removes them, and returns the count.
- **GetCountAsync**: Returns the count of logs in the dictionary.
- **SearchAsync**: Performs a case-insensitive search on message and unit name fields, orders by timestamp descending.

## Namespace

`SystemdServiceMonitor.Data.Repositories`

## Related Files

- `Data/Repositories/ILogRepository.cs` - Interface definition
- `Data/Repositories/LogRepository.cs` - Implementation
- `Models/ServiceLog.cs` - The entity model being managed