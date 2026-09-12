# IServiceRepository Interface and ServiceRepository Implementation

## Overview

This document describes the `IServiceRepository` interface and its `ServiceRepository` implementation in the `SystemdServiceMonitor.Data.Repositories` namespace.

The repository pattern is used to abstract data access operations for `ServiceInfo` entities, providing a clean separation between the business logic and data storage concerns.

## IServiceRepository Interface

The `IServiceRepository` interface defines the contract for service unit data access operations, including CRUD operations and various query methods.

### Methods

| Method | Description |
|--------|-------------|
| `Task<ServiceInfo?> GetByIdAsync(Guid id, CancellationToken ct = default)` | Retrieves a service by its unique identifier. |
| `Task<ServiceInfo?> GetByUnitNameAsync(string unitName, CancellationToken ct = default)` | Retrieves a service by its unit name (systemd service name). |
| `Task<IEnumerable<ServiceInfo>> GetAllAsync(CancellationToken ct = default)` | Retrieves all services. |
| `Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default)` | Retrieves all services with state `Active`. |
| `Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default)` | Retrieves all services with state `Failed`. |
| `Task<IEnumerable<ServiceInfo>> GetByUserAsync(string username, CancellationToken ct = default)` | Retrieves all services running under a specific user. |
| `Task<ServiceInfo> CreateAsync(ServiceInfo service, CancellationToken ct = default)` | Creates a new service record. |
| `Task<ServiceInfo> UpdateAsync(ServiceInfo service, CancellationToken ct = default)` | Updates an existing service record. |
| `Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)` | Deletes a service by its identifier. |
| `Task<int> GetTotalCountAsync(CancellationToken ct = default)` | Gets the total number of services. |
| `Task<IEnumerable<ServiceInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)` | Retrieves a paged list of services. |
| `Task<IEnumerable<ServiceInfo>> SearchAsync(string query, CancellationToken ct = default)` | Searches services by unit name or description. |

### Thread Safety

All methods are designed to be thread-safe for concurrent access. The implementation uses a `SemaphoreSlim` to synchronize access to the underlying data store.

## ServiceRepository Implementation

The `ServiceRepository` class provides an in-memory implementation of the `IServiceRepository` interface. It stores service data in a dictionary and is intended for use in scenarios where a lightweight, non-persistent data store is acceptable (e.g., testing, prototyping, or when services are managed entirely in memory).

### Key Features

- **In-Memory Storage**: Uses a `Dictionary<Guid, ServiceInfo>` to store service records.
- **Thread Safety**: All operations are protected by a `SemaphoreSlim` to ensure safe concurrent access.
- **Automatic Timestamps**: The `CreateAsync` and `UpdateAsync` methods automatically set the `CreatedAt` and `UpdatedAt` properties.
- **Validation**: Methods validate input parameters and throw appropriate exceptions for invalid states (e.g., attempting to create a duplicate service, updating a non-existent service).

### Usage Notes

This implementation is suitable for:
- Development and testing environments
- Scenarios where service data is transient and does not require persistence
- Situations where the entire dataset can comfortably fit in memory

For production scenarios requiring persistent storage, a different implementation (e.g., using Entity Framework Core) would be more appropriate.

### Methods Implementation Details

- **GetByIdAsync**: Looks up a service by its ID in the dictionary.
- **GetByUnitNameAsync**: Searches for a service with a matching unit name.
- **GetAllAsync**: Returns all services sorted by unit name.
- **GetActiveServicesAsync/GetFailedServicesAsync**: Filters services by state and sorts by unit name.
- **GetByUserAsync**: Filters services by the `RunAsUser` property.
- **CreateAsync**: Adds a new service, setting creation and update timestamps.
- **UpdateAsync**: Updates an existing service, updating the update timestamp.
- **DeleteAsync**: Removes a service by ID.
- **GetTotalCountAsync**: Returns the count of services in the dictionary.
- **GetPagedAsync**: Returns a sorted, paged subset of services.
- **SearchAsync**: Performs a case-insensitive search on unit name and description.

## Namespace

`SystemdServiceMonitor.Data.Repositories`

## Related Files

- `Data/Repositories/IServiceRepository.cs` - Interface definition
- `Data/Repositories/ServiceRepository.cs` - Implementation
- `Models/ServiceInfo.cs` - The entity model being managed