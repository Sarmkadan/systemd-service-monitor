# ServiceMonitorServiceExtensions

`ServiceMonitorServiceExtensions` provides a set of asynchronous extension methods for querying and aggregating systemd service state from the `systemd-service-monitor` infrastructure. It acts as a convenience layer over the underlying service monitor, allowing callers to retrieve service information filtered by high-level state, substate, or specific service name, to check monitoring eligibility, and to compute statistical summaries of service states without interacting directly with the monitor’s internal data structures.

## API

### GetServicesByStateAsync

```csharp
public static async Task<IEnumerable<ServiceInfo>> GetServicesByStateAsync(this IServiceMonitor monitor, string state)
```

Returns all services whose primary systemd state matches the given `state` string (e.g. `"active"`, `"inactive"`). The comparison is case-sensitive and expects the canonical systemd state names. If `monitor` is `null`, an `ArgumentNullException` is thrown. If `state` is `null` or empty, an `ArgumentException` is thrown.

### GetServicesBySubStateAsync

```csharp
public static async Task<IEnumerable<ServiceInfo>> GetServicesBySubStateAsync(this IServiceMonitor monitor, string subState)
```

Returns all services whose systemd substate matches the given `subState` string (e.g. `"running"`, `"dead"`). The comparison is case-sensitive and expects canonical systemd substate names. Throws `ArgumentNullException` when `monitor` is `null`, and `ArgumentException` when `subState` is `null` or empty.

### GetServiceByNameWithRefreshAsync

```csharp
public static async Task<ServiceInfo?> GetServiceByNameWithRefreshAsync(this IServiceMonitor monitor, string serviceName)
```

Looks up a single service by its unit name, triggers an on-demand refresh of that service’s state from systemd, and returns the resulting `ServiceInfo`. Returns `null` if no service with the given name is tracked by the monitor. Throws `ArgumentNullException` when `monitor` is `null`, and `ArgumentException` when `serviceName` is `null` or empty.

### GetMultipleServiceStatusesAsync

```csharp
public static async Task<IEnumerable<ServiceStatus>> GetMultipleServiceStatusesAsync(this IServiceMonitor monitor, IEnumerable<string> serviceNames)
```

Accepts a collection of service unit names and returns a corresponding collection of `ServiceStatus` objects, each containing the name and current status of the requested service. If a requested name is not tracked, its entry will indicate an unknown or absent status rather than being omitted. Throws `ArgumentNullException` when `monitor` or `serviceNames` is `null`. Individual `null` entries within `serviceNames` are silently skipped.

### IsServiceMonitored

```csharp
public static bool IsServiceMonitored(this IServiceMonitor monitor, string serviceName)
```

Determines synchronously whether a given service unit name is currently within the monitor’s tracking scope. Returns `true` if the service is known to the monitor, `false` otherwise. Throws `ArgumentNullException` when `monitor` is `null`, and `ArgumentException` when `serviceName` is `null` or empty.

### GetStatisticsByStateAsync

```csharp
public static async Task<ServiceStatistics> GetStatisticsByStateAsync(this IServiceMonitor monitor)
```

Aggregates all currently tracked services and returns a `ServiceStatistics` object containing counts grouped by primary systemd state. The result reflects the monitor’s last-known state snapshot and does not force a global refresh. Throws `ArgumentNullException` when `monitor` is `null`.

### GetServicesWithStatusAsync

```csharp
public static async Task<IEnumerable<ServiceInfo>> GetServicesWithStatusAsync(this IServiceMonitor monitor, ServiceStatusFilter filter)
```

Returns services that satisfy the criteria defined by a `ServiceStatusFilter` object, which may combine conditions on state, substate, and other properties. The exact filtering logic is delegated to the filter implementation. Throws `ArgumentNullException` when `monitor` or `filter` is `null`.

## Usage

### Example 1: Retrieving and displaying active services

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;

public async Task DisplayActiveServices(IServiceMonitor monitor)
{
    IEnumerable<ServiceInfo> activeServices = await monitor.GetServicesByStateAsync("active");

    foreach (var service in activeServices)
    {
        Console.WriteLine($"{service.UnitName}: {service.SubState}");
    }
}
```

### Example 2: Checking a specific service and computing statistics

```csharp
using System;
using System.Threading.Tasks;

public async Task AnalyzeService(IServiceMonitor monitor, string unitName)
{
    if (!monitor.IsServiceMonitored(unitName))
    {
        Console.WriteLine($"Service '{unitName}' is not monitored.");
        return;
    }

    ServiceInfo? info = await monitor.GetServiceByNameWithRefreshAsync(unitName);
    if (info is not null)
    {
        Console.WriteLine($"{info.UnitName} is {info.State}/{info.SubState}");
    }

    ServiceStatistics stats = await monitor.GetStatisticsByStateAsync();
    Console.WriteLine($"Active: {stats.ActiveCount}, Inactive: {stats.InactiveCount}, Failed: {stats.FailedCount}");
}
```

## Notes

- All async methods rely on the underlying `IServiceMonitor` implementation for thread safety. Callers should assume that these extension methods themselves are not thread-safe unless the supplied monitor explicitly guarantees concurrent access.
- `GetServiceByNameWithRefreshAsync` forces a refresh for a single service, which may involve a D-Bus call to systemd. Calling it in a tight loop can introduce latency and should be avoided; prefer batch methods like `GetMultipleServiceStatusesAsync` for multiple lookups.
- `GetStatisticsByStateAsync` operates on the monitor’s current in-memory snapshot. If services have changed state since the last refresh cycle, the returned statistics may be stale. Combine with a refresh mechanism if up-to-the-moment accuracy is required.
- `IsServiceMonitored` is synchronous and returns immediately; it does not validate whether the service actually exists in systemd, only whether the monitor has been configured to track it.
- Methods that accept `IEnumerable<string>` parameters, such as `GetMultipleServiceStatusesAsync`, will enumerate the collection once. Passing an unmaterialized query that performs expensive per-element computation may cause unexpected delays.
- All methods validate their arguments on entry and will throw before any asynchronous work begins when preconditions are violated.
