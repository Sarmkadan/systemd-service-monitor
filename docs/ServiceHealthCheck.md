# ServiceHealthCheck

`ServiceHealthCheck` is a data model representing a configurable health check for a systemd service. It tracks service health status over time, including thresholds for healthy/unhealthy states, timeout and interval configuration, and historical check results.

## API

### `Id`
- **Type**: `Guid`
- **Purpose**: Unique identifier for the health check instance.
- **Notes**: Immutable after construction.

### `ServiceInfoId`
- **Type**: `Guid`
- **Purpose**: Identifier linking the health check to a specific service in the systemd service monitor.
- **Notes**: Must correspond to an existing `ServiceInfo` record.

### `Name`
- **Type**: `string`
- **Purpose**: Human-readable name for the health check.
- **Notes**: Used for display and logging purposes.

### `CheckType`
- **Type**: `HealthCheckType`
- **Purpose**: Specifies the type of health check to perform (e.g., HTTP endpoint, TCP port, systemd service status).
- **Notes**: Influences how `Endpoint` is interpreted.

### `Description`
- **Type**: `string`
- **Purpose**: Detailed description of the health check's purpose and expected behavior.
- **Notes**: Optional; may be empty.

### `Endpoint`
- **Type**: `string`
- **Purpose**: The target endpoint for the health check (e.g., URL for HTTP checks, port for TCP checks).
- **Notes**: Format depends on `CheckType`.

### `HttpMethod`
- **Type**: `string?`
- **Purpose**: HTTP method to use for HTTP-based health checks (e.g., `GET`, `HEAD`).
- **Notes**: Required if `CheckType` is HTTP-based; otherwise ignored.

### `ExpectedHttpStatus`
- **Type**: `int?`
- **Purpose**: Expected HTTP status code for successful health checks.
- **Notes**: Commonly `200` or `204`; ignored for non-HTTP checks.

### `TimeoutSeconds`
- **Type**: `int`
- **Purpose**: Maximum duration (in seconds) allowed for a single health check to complete.
- **Notes**: Must be a positive integer.

### `IntervalSeconds`
- **Type**: `int`
- **Purpose**: Time (in seconds) between consecutive health checks.
- **Notes**: Must be a positive integer.

### `UnhealthyThreshold`
- **Type**: `int`
- **Purpose**: Number of consecutive failed checks required to mark the service as unhealthy.
- **Notes**: Must be a non-negative integer.

### `HealthyThreshold`
- **Type**: `int`
- **Purpose**: Number of consecutive successful checks required to mark the service as healthy.
- **Notes**: Must be a non-negative integer.

### `IsEnabled`
- **Type**: `bool`
- **Purpose**: Indicates whether the health check is actively monitored.
- **Notes**: Disabling pauses check execution but retains historical data.

### `CurrentStatus`
- **Type**: `HealthStatus`
- **Purpose**: Current health status of the service (e.g., `Healthy`, `Unhealthy`, `Unknown`).
- **Notes**: Updated after each check execution.

### `LastCheckMessage`
- **Type**: `string`
- **Purpose**: Result message from the most recent health check.
- **Notes**: May include error details or success confirmation.

### `LastCheckResponseMs`
- **Type**: `long`
- **Purpose**: Response time (in milliseconds) of the last health check.
- **Notes**: `0` if the check did not complete or timed out.

### `LastCheckTime`
- **Type**: `DateTime?`
- **Purpose**: Timestamp of the last health check execution.
- **Notes**: `null` if no checks have been performed.

### `ConsecutiveFailures`
- **Type**: `int`
- **Purpose**: Count of consecutive failed health checks.
- **Notes**: Resets to `0` on success.

### `ConsecutiveSuccesses`
- **Type**: `int`
- **Purpose**: Count of consecutive successful health checks.
- **Notes**: Resets to `0` on failure.

### `TotalChecks`
- **Type**: `long`
- **Purpose**: Total number of health checks performed.
- **Notes**: Includes both successful and failed attempts.

## Usage

### Example 1: Creating and Registering a Health Check
