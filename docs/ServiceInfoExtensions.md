# ServiceInfoExtensions

Provides extension methods for querying and formatting the state of a systemd service represented by a `ServiceInfo` instance. These methods encapsulate common status checks and presentation logic used by monitoring UIs and automation scripts.

## API

### IsActive
```csharp
public static bool IsActive(this ServiceInfo service)
```
Returns `true` when the service's `ActiveState` property equals `"active"`; otherwise `false`.

**Parameters**  
- `service` — The service instance to evaluate. Must not be `null`.

**Returns**  
`true` if the service is currently active; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

---

### IsFailed
```csharp
public static bool IsFailed(this ServiceInfo service)
```
Returns `true` when the service's `SubState` property equals `"failed"`; otherwise `false`.

**Parameters**  
- `service` — The service instance to evaluate. Must not be `null`.

**Returns**  
`true` if the service is in a failed state; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

---

### IsEnabled
```csharp
public static bool IsEnabled(this ServiceInfo service)
```
Returns `true` when the service's `UnitFileState` property equals `"enabled"` or `"static"`; otherwise `false`.

**Parameters**  
- `service` — The service instance to evaluate. Must not be `null`.

**Returns**  
`true` if the service is enabled or static; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

---

### GetFormattedUptime
```csharp
public static string GetFormattedUptime(this ServiceInfo service)
```
Formats the service's uptime as a human-readable string (e.g., `"2d 3h 15m"`). Returns `"N/A"` if the service is not active or `ActiveEnterTimestamp` is not set.

**Parameters**  
- `service` — The service instance to evaluate. Must not be `null`.

**Returns**  
A formatted uptime string, or `"N/A"` when uptime cannot be determined.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

---

### CanRestart
```csharp
public static bool CanRestart(this ServiceInfo service)
```
Returns `true` when the service is in a state that permits a restart operation (i.e., `ActiveState` is `"active"`, `"inactive"`, or `"failed"`). Returns `false` for transient states such as `"activating"` or `"deactivating"`.

**Parameters**  
- `service` — The service instance to evaluate. Must not be `null`.

**Returns**  
`true` if a restart request would be accepted; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

---

### GetStatusSummary
```csharp
public static string GetStatusSummary(this ServiceInfo service)
```
Produces a concise one-line summary combining the service name, active state, sub-state, and enabled status (e.g., `"nginx.service: active (running), enabled"`).

**Parameters**  
- `service` — The service instance to summarize. Must not be `null`.

**Returns**  
A formatted summary string.

**Exceptions**  
- `ArgumentNullException` — Thrown if `service` is `null`.

## Usage

```csharp
using SystemdServiceMonitor;

var services = ServiceEnumerator.GetAllServices();

foreach (var svc in services.Where(s => s.IsActive()))
{
    Console.WriteLine(svc.GetStatusSummary());
    Console.WriteLine($"  Uptime: {svc.GetFormattedUptime()}");
    if (svc.CanRestart())
    {
        await ServiceController.RestartAsync(svc.Name);
    }
}
```

```csharp
using SystemdServiceMonitor;

var failedServices = ServiceEnumerator.GetAllServices()
    .Where(s => s.IsFailed())
    .Select(s => new
    {
        Name = s.Name,
        Summary = s.GetStatusSummary(),
        Uptime = s.GetFormattedUptime()
    });

foreach (var item in failedServices)
{
    _logger.LogWarning("Service {Name} is failed: {Summary} (uptime: {Uptime})",
        item.Name, item.Summary, item.Uptime);
}
```

## Notes

- All methods throw `ArgumentNullException` if the `service` parameter is `null`. Callers should guard against null references when the source collection may contain nulls.
- `GetFormattedUptime` relies on `ActiveEnterTimestamp` being populated by the underlying systemd D-Bus interface. If the timestamp is missing or the service is not active, the method returns `"N/A"` rather than throwing.
- `CanRestart` reflects systemd's own restart eligibility rules. It does not verify policykit authorization or whether the calling user has permission to restart the unit.
- These extension methods are pure functions with no internal mutable state. They are thread-safe provided the `ServiceInfo` instance itself is not mutated concurrently. The `ServiceInfo` type is typically treated as immutable after construction.
