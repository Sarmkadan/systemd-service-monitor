// existing content ...

## ServiceMonitorServiceExtensions

The `ServiceMonitorServiceExtensions` class provides utility methods for querying and analyzing service information. It enables you to retrieve services by state, sub-state, or name, as well as get service statuses and statistics.

### Usage Example

```csharp
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

// Assuming a list of ServiceInfo objects named 'services'
var services = await ServiceMonitorServiceExtensions.GetServicesByStateAsync(ServiceState.Running);
Console.WriteLine($"Running services: {string.Join(", ", services.Select(s => s.Id))}");

var activeServices = await ServiceMonitorServiceExtensions.GetServicesBySubStateAsync(ServiceSubState.Active);
Console.WriteLine($"Active services: {string.Join(", ", activeServices.Select(s => s.Id))}");

var serviceInfo = await ServiceMonitorServiceExtensions.GetServiceByNameWithRefreshAsync("service-name");
Console.WriteLine($"Service info for 'service-name': {serviceInfo?.Id}");

var serviceStatuses = await ServiceMonitorServiceExtensions.GetMultipleServiceStatusesAsync(new[] { "service1", "service2" });
Console.WriteLine($"Service statuses: {string.Join(", ", serviceStatuses.Select(s => $"{s.ServiceId}: {s.Status}"))}");

var isMonitored = ServiceMonitorServiceExtensions.IsServiceMonitored("service-name");
Console.WriteLine($"Is 'service-name' monitored: {isMonitored}");

var statistics = await ServiceMonitorServiceExtensions.GetStatisticsByStateAsync(ServiceState.Failed);
Console.WriteLine($"Statistics for failed services: {statistics}");

var servicesWithStatus = await ServiceMonitorServiceExtensions.GetServicesWithStatusAsync(ServiceStatusType.Warning);
Console.WriteLine($"Services with warning status: {string.Join(", ", servicesWithStatus.Select(s => s.Id))}");
```
