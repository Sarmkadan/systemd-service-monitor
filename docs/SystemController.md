# SystemController

Provides HTTP endpoints for monitoring and querying the state of a systemd-based Linux system. It exposes endpoints to retrieve system health, resource usage, service status, version information, and diagnostics.

## API

### `HealthCheck`
Performs a basic health check of the monitoring service itself. Returns a simple success indicator if the service is responsive.

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<object>>>`
  - Success: HTTP 200 with `ApiResponse<object>` containing a success indicator.
  - Failure: HTTP 500 with `ApiResponse<object>` containing error details.
- **Throws**: May throw if the monitoring service is unresponsive or misconfigured.

---

### `GetSystemInfo`
Retrieves basic system information such as OS name, kernel version, and system uptime.

- **Parameters**: None
- **Return value**: `ActionResult<ApiResponse<object>>`
  - Success: HTTP 200 with `ApiResponse<object>` containing system metadata.
  - Failure: HTTP 500 with `ApiResponse<object>` containing error details.
- **Throws**: May throw if system information cannot be retrieved (e.g., permissions, missing tools).

---

### `GetSystemResources`
Asynchronously retrieves real-time system resource usage metrics (CPU, memory, disk, network).

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<SystemResource>>>`
  - Success: HTTP 200 with `ApiResponse<SystemResource>` containing resource metrics.
  - Failure: HTTP 500 with `ApiResponse<SystemResource>` containing error details.
- **Throws**: May throw if resource monitoring tools fail or are unavailable.

---

### `GetSystemSummary`
Asynchronously retrieves a high-level summary of system status, including health indicators and key metrics.

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<object>>>`
  - Success: HTTP 200 with `ApiResponse<object>` containing summary data.
  - Failure: HTTP 500 with `ApiResponse<object>` containing error details.
- **Throws**: May throw if summary generation fails due to missing dependencies or permissions.

---
### `GetFailedServices`
Asynchronously retrieves a list of systemd services that are in a failed state.

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<List<ServiceInfo>>>>`
  - Success: HTTP 200 with `ApiResponse<List<ServiceInfo>>` containing failed services.
  - Failure: HTTP 500 with `ApiResponse<List<ServiceInfo>>` containing error details.
- **Throws**: May throw if service status cannot be queried (e.g., `systemctl` access denied).

---
### `GetProblematicServices`
Asynchronously retrieves a list of systemd services that are in a degraded or problematic state (e.g., activating, deactivating, or in a maintenance state).

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<List<ServiceInfo>>>>`
  - Success: HTTP 200 with `ApiResponse<List<ServiceInfo>>` containing problematic services.
  - Failure: HTTP 500 with `ApiResponse<List<ServiceInfo>>` containing error details.
- **Throws**: May throw if service status cannot be queried (e.g., `systemctl` access denied).

---
### `GetVersion`
Retrieves the version of the monitoring service.

- **Parameters**: None
- **Return value**: `ActionResult<ApiResponse<object>>`
  - Success: HTTP 200 with `ApiResponse<object>` containing version information.
  - Failure: HTTP 500 with `ApiResponse<object>` containing error details.
- **Throws**: May throw if version metadata is unavailable.

---
### `GetDiagnostics`
Asynchronously retrieves diagnostic information about the system and the monitoring service, including logs and configuration checks.

- **Parameters**: None
- **Return value**: `Task<ActionResult<ApiResponse<object>>>`
  - Success: HTTP 200 with `ApiResponse<object>` containing diagnostic data.
  - Failure: HTTP 500 with `ApiResponse<object>` containing error details.
- **Throws**: May throw if diagnostics collection fails (e.g., log access denied).

## Usage

### Example 1: Basic Health Check
