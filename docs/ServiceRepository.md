# ServiceRepository
The `ServiceRepository` class provides a set of asynchronous methods for managing and querying services in the `systemd-service-monitor` project. It allows for retrieving services by ID, unit name, or user, as well as creating, updating, and deleting services. Additionally, it provides methods for retrieving active and failed services, searching services, and getting the total count of services.

## API
* `GetByIdAsync`: Retrieves a service by its ID. Returns a `ServiceInfo` object if found, otherwise `null`. Throws if the ID is invalid or an error occurs during retrieval.
* `GetByUnitNameAsync`: Retrieves a service by its unit name. Returns a `ServiceInfo` object if found, otherwise `null`. Throws if the unit name is invalid or an error occurs during retrieval.
* `GetAllAsync`: Retrieves all services. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.
* `GetActiveServicesAsync`: Retrieves all active services. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.
* `GetFailedServicesAsync`: Retrieves all failed services. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.
* `GetByUserAsync`: Retrieves services by user. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.
* `CreateAsync`: Creates a new service. Returns a `ServiceInfo` object representing the created service. Throws if the creation fails or an error occurs.
* `UpdateAsync`: Updates an existing service. Returns a `ServiceInfo` object representing the updated service. Throws if the update fails or an error occurs.
* `DeleteAsync`: Deletes a service by its ID. Returns `true` if the deletion is successful, otherwise `false`. Throws if the ID is invalid or an error occurs during deletion.
* `GetTotalCountAsync`: Retrieves the total count of services. Returns an `int` representing the total count. Throws if an error occurs during retrieval.
* `GetPagedAsync`: Retrieves services with pagination. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.
* `SearchAsync`: Searches services based on a query. Returns an `IEnumerable` of `ServiceInfo` objects. Throws if an error occurs during retrieval.

## Usage
```csharp
// Example 1: Retrieving a service by ID
var serviceRepository = new ServiceRepository();
var serviceInfo = await serviceRepository.GetByIdAsync(1);
if (serviceInfo != null)
{
    Console.WriteLine($"Service ID: {serviceInfo.Id}, Unit Name: {serviceInfo.UnitName}");
}

// Example 2: Creating a new service
var newServiceInfo = new ServiceInfo { UnitName = "example.service", Description = "Example service" };
var createdServiceInfo = await serviceRepository.CreateAsync(newServiceInfo);
Console.WriteLine($"Created Service ID: {createdServiceInfo.Id}, Unit Name: {createdServiceInfo.UnitName}");
```

## Notes
The `ServiceRepository` class is designed to be thread-safe, allowing for concurrent access and modification of services. However, it is essential to note that the `DeleteAsync` method may throw if the service is being accessed or modified by another thread while deletion is attempted. Additionally, the `GetPagedAsync` and `SearchAsync` methods may return incomplete or outdated results if the underlying data changes rapidly. It is recommended to use these methods with caution and consider implementing additional caching or synchronization mechanisms if necessary.
