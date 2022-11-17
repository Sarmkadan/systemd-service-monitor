# ServiceHealthChecker

A utility class that provides health-checking capabilities for systemd services, exposing static methods to query service status, calculate reliability metrics, and generate actionable recommendations.

## API

### `public static ServiceHealthStatus GetHealthStatus()`

Determines the overall health status of the systemd service being monitored. The status is derived from multiple factors including service state, recent failures, and uptime patterns.

- **Return value**: A `ServiceHealthStatus` enum value indicating the current health condition (`Healthy`, `Warning`, `Critical`, or `Degraded`).
- **Throws**: `InvalidOperationException` if the underlying systemd interface is unavailable or the service is not found.

---

### `public static string GetHealthSummary()`

Generates a human-readable summary of the service's current health, combining status, uptime, and recent issues into a concise diagnostic message.

- **Return value**: A string containing a brief health assessment (e.g., "Service is healthy (uptime: 5d 3h). No recent failures detected.").
- **Throws**: `InvalidOperationException` if the service metadata cannot be retrieved.

---

### `public static bool IsProblematic()`

Checks whether the service is in a problematic state, defined as any non-healthy status (`Warning`, `Critical`, or `Degraded`) or recent failure activity.

- **Return value**: `true` if the service is problematic; otherwise, `false`.
- **Throws**: `InvalidOperationException` if the service state cannot be queried.

---

### `public static List<string> GetRecommendedActions()`

Returns a prioritized list of recommended actions to remediate detected issues, such as restarting the service, checking logs, or adjusting resource limits.

- **Return value**: A list of strings, each describing a specific action (e.g., ["Restart service", "Check journalctl logs for errors"]). The list is empty if no actions are recommended.
- **Throws**: `InvalidOperationException` if the health assessment cannot be performed.

---
### `public static string FormatUptime()`

Formats the service's current uptime into a human-readable string (e.g., "2d 14h 30m").

- **Return value**: A string representing the formatted uptime. Returns `"N/A"` if uptime cannot be determined.
- **Throws**: Never.

---
### `public static double CalculateReliability(int days = 7)`

Calculates the service's reliability over the specified trailing period, expressed as a percentage (0.0 to 100.0).

- **Parameters**:
  - `days` (optional, default: 7): The number of trailing days to consider in the calculation.
- **Return value**: A `double` between 0.0 and 100.0 representing the reliability score. Returns `0.0` if insufficient data is available.
- **Throws**: `ArgumentOutOfRangeException` if `days` is less than 1 or greater than 30.

## Usage
