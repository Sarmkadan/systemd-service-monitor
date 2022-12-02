# ServiceDetailsDtoExtensions

Provides extension methods for `ServiceDetailsDto` that expose derived state and formatted output for systemd service monitoring.

## API

### `IsActive(ServiceDetailsDto service)`
Determines whether the service is currently active (running).
**Parameters**
- `service`: The service details to inspect.

**Return value**
- `true` if the service is active; otherwise, `false`.

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---

### `IsFailed(ServiceDetailsDto service)`
Determines whether the service has entered a failed state.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- `true` if the service is in a failed state; otherwise, `false`.

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---
### `IsAutoStartEnabled(ServiceDetailsDto service)`
Determines whether the service is configured to start automatically at boot.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- `true` if the service is enabled for auto-start; otherwise, `false`.

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---
### `GetStatusDisplay(ServiceDetailsDto service)`
Returns a human-readable status string for the service.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- A localized or standardized status description (e.g., "active (running)", "failed", "inactive (dead)").

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---
### `GetHealthSummary(ServiceDetailsDto service)`
Returns a concise health summary for the service.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- A string summarizing the service's health (e.g., "Healthy: 0 restarts in 24h", "Unhealthy: 5 restarts in 1h").

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---
### `GetFormattedUptime(ServiceDetailsDto service)`
Returns the service's uptime formatted as a human-readable string.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- A string representing the uptime (e.g., "2h 30m", "5d 12h") or `"n/a"` if uptime is unavailable.

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

---
### `ShouldRestart(ServiceDetailsDto service)`
Determines whether the service should be restarted based on its failure or restart policy.
**Parameters**
- `service`: The service details to inspect.

**Return value**
- `true` if the service should be restarted; otherwise, `false`.

**Exceptions**
- Throws `ArgumentNullException` if `service` is `null`.

## Usage
