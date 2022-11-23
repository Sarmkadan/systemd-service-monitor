# ServiceDetailsDto
The `ServiceDetailsDto` type is a data transfer object designed to hold detailed information about a systemd service. It provides a comprehensive overview of the service's current state, configuration, and runtime metrics, making it a valuable resource for service monitoring and management applications.

## API
The `ServiceDetailsDto` type exposes the following public members:
* `Id`: A unique identifier for the service, represented as a `Guid`.
* `UnitName`: The name of the systemd unit, represented as a `string`.
* `Description`: A brief description of the service, represented as a `string`.
* `State`: The current state of the service (e.g., "running", "stopped", etc.), represented as a `string`.
* `SubState`: A more detailed description of the service's state, represented as a `string`.
* `MainProcessId`: The process ID of the service's main process, represented as an `int`.
* `Result`: The result of the service's last operation, represented as a `string`.
* `RestartPolicy`: The service's restart policy, represented as a `string`.
* `AutoStart`: A boolean indicating whether the service is configured to start automatically.
* `Restart`: A boolean indicating whether the service is configured to restart on failure.
* `Dependencies`: A list of services that this service depends on, represented as a `List<string>`.
* `Dependents`: A list of services that depend on this service, represented as a `List<string>`.
* `LastStartTime`: The time at which the service was last started, represented as a `DateTime?`.
* `LastStopTime`: The time at which the service was last stopped, represented as a `DateTime?`.
* `UptimeSeconds`: The amount of time the service has been running, represented as a `long`.
* `RestartCount`: The number of times the service has been restarted, represented as an `int`.
* `RunAsUser`: The user under which the service runs, represented as a `string`.
* `RunAsGroup`: The group under which the service runs, represented as a `string`.
* `StatusSummary`: A brief summary of the service's status, represented as a `string?`.
* `HealthStatus`: The health status of the service, represented as a `string?`.

## Usage
The following examples demonstrate how to use the `ServiceDetailsDto` type:
```csharp
// Example 1: Creating a new ServiceDetailsDto instance
var serviceDetails = new ServiceDetailsDto
{
    Id = Guid.NewGuid(),
    UnitName = "example.service",
    Description = "An example service",
    State = "running",
    // ...
};

// Example 2: Using ServiceDetailsDto to monitor a service
var serviceMonitor = new ServiceMonitor();
var serviceDetails = serviceMonitor.GetServiceDetails("example.service");
Console.WriteLine($"Service {serviceDetails.UnitName} is {serviceDetails.State}");
```

## Notes
When working with the `ServiceDetailsDto` type, consider the following edge cases and thread-safety remarks:
* The `LastStartTime` and `LastStopTime` properties may be null if the service has not been started or stopped, respectively.
* The `UptimeSeconds` property may not be accurate if the service has been restarted multiple times.
* The `RestartCount` property may not be accurate if the service has been restarted multiple times and the restart count has been reset.
* The `StatusSummary` and `HealthStatus` properties may be null if the service does not provide this information.
* The `ServiceDetailsDto` type is designed to be thread-safe, but it is still important to ensure that instances are properly synchronized when accessed from multiple threads.
