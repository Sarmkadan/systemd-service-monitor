# IMetricRepository Interface and MetricRepository Implementation

## Overview

This document describes the `IMetricRepository` interface and its `MetricRepository` implementation in the `SystemdServiceMonitor.Data.Repositories` namespace.

The repository pattern is used to abstract data access operations for `ServiceMetric` entities, providing a clean separation between the business logic and data storage concerns.

## IMetricRepository Interface

The `IMetricRepository` interface defines the contract for service metric data access operations, including CRUD operations and time-series queries.

### Methods

| Method | Description |
|--------|-------------|
| `Task<ServiceMetric?> GetByIdAsync(Guid id, CancellationToken ct = default)` | Retrieves a metric by its unique identifier. |
| `Task<IEnumerable<ServiceMetric>> GetByServiceIdAsync(Guid serviceId, CancellationToken ct = default)` | Retrieves all metrics for a specific service. |
| `Task<IEnumerable<ServiceMetric>> GetByMetricTypeAsync(MetricType type, CancellationToken ct = default)` | Retrieves all metrics of a specific type across all services. |
| `Task<IEnumerable<ServiceMetric>> GetTimeSeriesAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default)` | Retrieves a time series of metrics for a service and metric type within a time range. |
| `Task<IEnumerable<ServiceMetric>> GetRecentAsync(int hours, int limit = 1000, CancellationToken ct = default)` | Retrieves the most recent metrics across all services within a specified number of hours. |
| `Task<ServiceMetric> CreateAsync(ServiceMetric metric, CancellationToken ct = default)` | Creates a new metric record. |
| `Task<int> CreateBatchAsync(IEnumerable<ServiceMetric> metrics, CancellationToken ct = default)` | Creates multiple metric records in a batch. |
| `Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)` | Deletes a metric by its identifier. |
| `Task<int> DeleteOlderThanAsync(DateTime before, CancellationToken ct = default)` | Deletes all metrics older than a specified date. |
| `Task<ServiceMetric?> GetLatestAsync(Guid serviceId, MetricType type, CancellationToken ct = default)` | Retrieves the most recent metric for a service and metric type. |
| `Task<decimal> GetAverageAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default)` | Calculates the average value of metrics for a service and metric type within a time range. |

### Thread Safety

All methods are designed to be thread-safe for concurrent access. The implementation uses a `SemaphoreSlim` to synchronize access to the underlying data store.

## MetricRepository Implementation

The `MetricRepository` class provides an in-memory implementation of the `IMetricRepository` interface. It stores metric data in a dictionary and is intended for use in scenarios where a lightweight, non-persistent data store is acceptable (e.g., testing, prototyping, or when metrics are managed entirely in memory).

### Key Features

- **In-Memory Storage**: Uses a `Dictionary<Guid, ServiceMetric>` to store metric records.
- **Thread Safety**: All operations are protected by a `SemaphoreSlim` to ensure safe concurrent access.
- **Efficient Queries**: Methods are optimized for common query patterns (by service, by type, time-series, etc.).
- **Automatic Cleanup**: Includes methods for deleting old metrics to prevent unbounded memory growth.

### Usage Notes

This implementation is suitable for:
- Development and testing environments
- Scenarios where metric data is transient and does not require persistence
- Situations where the entire dataset can comfortably fit in memory
- High-frequency metric collection where low-latency access is important

For production scenarios requiring persistent storage or handling very large volumes of metrics, a different implementation (e.g., using a time-series database) would be more appropriate.

### Methods Implementation Details

- **GetByIdAsync**: Looks up a metric by its ID in the dictionary.
- **GetByServiceIdAsync**: Filters metrics by service ID and returns them sorted by timestamp (newest first).
- **GetByMetricTypeAsync**: Filters metrics by metric type and returns them sorted by timestamp (newest first).
- **GetTimeSeriesAsync**: Filters metrics by service ID, metric type, and time range, then returns them sorted by timestamp (oldest first for charting).
- **GetRecentAsync**: Filters metrics by timestamp (within the last N hours), sorts by timestamp (newest first), and applies a limit.
- **CreateAsync**: Adds a new metric to the dictionary.
- **CreateBatchAsync**: Adds multiple metrics to the dictionary in a loop.
- **DeleteAsync**: Removes a metric by ID from the dictionary.
- **DeleteOlderThanAsync**: Identifies and removes all metrics older than a specified date.
- **GetLatestAsync**: Retrieves the most recent metric for a service and metric type.
- **GetAverageAsync**: Calculates the average value of metrics for a service and metric type within a time range.

## Namespace

`SystemdServiceMonitor.Data.Repositories`

## Related Files

- `Data/Repositories/IMetricRepository.cs` - Interface definition
- `Data/Repositories/MetricRepository.cs` - Implementation
- `Models/ServiceMetric.cs` - The entity model being managed