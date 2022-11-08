# ServiceMonitorService

The `ServiceMonitorService` class provides an asynchronous interface for interacting with systemd services, enabling the retrieval of service metadata, status inspection, and lifecycle management of the monitoring process. It acts as the primary facade for querying active, failed, or specific services by name, while maintaining an internal list of monitored targets and aggregating operational statistics.

## API

### `public ServiceMonitorService`
Initializes a new instance of the `ServiceMonitorService` class. This constructor sets up the necessary internal state required to communicate with the systemd bus or underlying service manager. It does not accept parameters and does not perform asynchronous I/O upon instantiation.

### `public async Task<IEnumerable<ServiceInfo>> GetAllServicesAsync`
Retrieves a comprehensive list of all services known to the system, regardless of their current state.
*   **Parameters**: None.
*   **Return Value**: A task representing the asynchronous operation, containing an enumerable collection of `ServiceInfo` objects.
*   **Exceptions**: Throws an exception if the underlying connection to the service manager is unavailable or if the enumeration fails due to permission issues.

### `public async Task<ServiceInfo?> GetServiceByNameAsync`
Fetches detailed information for a specific service identified by its name.
*   **Parameters**: Accepts a `string` representing the service name (e.g., `nginx.service`).
*   **Return Value**: A task containing the `ServiceInfo` object if the service exists, or `null` if no service with the specified name is found.
*   **Exceptions**: Throws an exception if the query mechanism fails or the input name is invalid according to systemd naming conventions.

### `public async Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync`
Returns a filtered list containing only services currently in an active or running state.
*   **Parameters**: None.
*   **Return Value**: A task containing an enumerable collection of `ServiceInfo` objects for active services.
*   **Exceptions**: Throws an exception if the state filtering operation encounters an error during execution.

### `public async Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync`
Returns a filtered list containing only services that have entered a failed state.
*   **Parameters**: None.
*   **Return Value**: A task containing an enumerable collection of `ServiceInfo` objects for failed services.
*   **Exceptions**: Throws an exception if the state filtering operation encounters an error during execution.

### `public async Task RefreshServiceListAsync`
Forces an immediate update of the internal cache or view of the service list to reflect the current system state.
*   **Parameters**: None.
*   **Return Value**: A task representing the asynchronous refresh operation.
*   **Exceptions**: Throws an exception if the refresh operation cannot communicate with the backend or fails to update the internal state.

### `public async Task<ServiceStatus?> GetServiceStatusAsync`
Retrieves the current runtime status of a specific service.
*   **Parameters**: Accepts a `string` representing the service name.
*   **Return Value**: A task containing a `ServiceStatus` object if the service exists, or `null` if the service is not found.
*   **Exceptions**: Throws an exception if the status query fails due to system errors.

### `public async Task StartMonitoringAsync`
Initiates the background monitoring process. This begins tracking the services defined in the monitored list and updating their states periodically or via event subscription.
*   **Parameters**: None.
*   **Return Value**: A task representing the asynchronous start operation.
*   **Exceptions**: Throws an exception if monitoring is already active or if the initialization of the monitoring loop fails.

### `public async Task StopMonitoringAsync`
Halts the background monitoring process and releases associated resources.
*   **Parameters**: None.
*   **Return Value**: A task representing the asynchronous stop operation.
*   **Exceptions**: Throws an exception if the shutdown process encounters an error while cleaning up resources.

### `public IEnumerable<string> GetMonitoredServices`
Provides a synchronous enumeration of the names of services currently configured for monitoring.
*   **Parameters**: None.
*   **Return Value**: An enumerable collection of strings representing service names.
*   **Exceptions**: Generally does not throw exceptions unless the internal collection is corrupted; it reflects the current configuration state.

### `public async Task<ServiceStatistics> GetStatisticsAsync`
Aggregates and returns statistical data regarding the monitoring session, such as uptime, query counts, or error rates.
*   **Parameters**: None.
*   **Return Value**: A task containing a `ServiceStatistics` object.
*   **Exceptions**: Throws an exception if statistics cannot be calculated or retrieved.

## Usage

### Example 1: Initializing and Retrieving Failed Services
This example demonstrates instantiating the service, starting the monitoring loop, and fetching a list of failed services for alerting purposes.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;

public class HealthCheck
{
    public async Task RunCheckAsync()
    {
        var monitor = new ServiceMonitorService();
        
        // Start the background monitoring process
        await monitor.StartMonitoringAsync();

        try
        {
            // Retrieve only services in a failed state
            var failedServices = await monitor.GetFailedServicesAsync();

            if (failedServices.Any())
            {
                Console.WriteLine($"Detected {failedServices.Count()} failed services:");
                foreach (var service in failedServices)
                {
                    Console.WriteLine($"- {service.Name}: {service.LoadState}");
                }
            }
            else
            {
                Console.WriteLine("No failed services detected.");
            }
        }
        finally
        {
            // Ensure monitoring is stopped to release resources
            await monitor.StopMonitoringAsync();
        }
    }
}
```

### Example 2: Specific Service Inspection and Statistics
This example shows how to query a specific service by name, check its detailed status, and retrieve overall monitoring statistics.

```csharp
using System;
using System.Threading.Tasks;

public class ServiceInspector
{
    public async Task InspectNginxAsync()
    {
        var monitor = new ServiceMonitorService();
        const string serviceName = "nginx.service";

        // Refresh the list to ensure latest data
        await monitor.RefreshServiceListAsync();

        // Get specific service info
        var serviceInfo = await monitor.GetServiceByNameAsync(serviceName);
        
        if (serviceInfo != null)
        {
            // Get detailed runtime status
            var status = await monitor.GetServiceStatusAsync(serviceName);
            
            if (status != null)
            {
                Console.WriteLine($"Service: {serviceInfo.Name}");
                Console.WriteLine($"Active State: {status.ActiveState}");
                Console.WriteLine($"Sub State: {status.SubState}");
            }
        }
        else
        {
            Console.WriteLine($"Service '{serviceName}' not found.");
        }

        // Retrieve session statistics
        var stats = await monitor.GetStatisticsAsync();
        Console.WriteLine($"Total Queries Executed: {stats.QueryCount}");
    }
}
```

## Notes

*   **Thread Safety**: The asynchronous nature of the API suggests that methods like `GetAllServicesAsync` and `RefreshServiceListAsync` are designed to be non-blocking. However, concurrent calls to `StartMonitoringAsync` and `StopMonitoringAsync` should be serialized by the caller to prevent race conditions in the monitoring lifecycle state. The synchronous `GetMonitoredServices` property returns an enumerable that should be treated as a snapshot; modifying the underlying collection while enumerating may result in undefined behavior if the internal implementation does not use concurrent collections.
*   **Null Handling**: Methods returning single entities (`GetServiceByNameAsync`, `GetServiceStatusAsync`) explicitly return `null` when the target service does not exist, rather than throwing a "not found" exception. Callers must handle null checks appropriately.
*   **State Consistency**: Data returned by `GetActiveServicesAsync` or `GetFailedServicesAsync` represents the state at the moment of the query. In highly dynamic environments, the state of a service may change immediately after the task completes. Use `RefreshServiceListAsync` before critical queries if strict freshness is required.
*   **Resource Management**: The `StartMonitoringAsync` method likely spawns background tasks or subscribes to system events. It is critical to pair every call to `StartMonitoringAsync` with a corresponding `StopMonitoringAsync` to prevent resource leaks, particularly in long-running applications or scoped services.
