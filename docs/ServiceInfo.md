# ServiceInfo

`ServiceInfo` is a data-transfer object that represents the runtime state and configuration of a systemd service unit. It aggregates monitoring metrics (CPU, memory), service lifecycle information (state, uptime, restart behavior), and dependency relationships with other units. This type is intended for use in monitoring tools, dashboards, or service management utilities that need to inspect or track systemd services programmatically.

## API

### `Id`
A unique identifier for the service instance. This value is stable for the lifetime of the process and can be used to correlate monitoring data across restarts.

### `LoadState`
Indicates whether the unit file was successfully loaded by systemd. Possible values include `Loaded`, `NotFound`, `Error`, and `Masked`. This property reflects the state of the unit definition, not its runtime status.

### `CpuUsagePercent`
The current CPU usage of the service as a percentage of total system CPU capacity. This value is sampled over a short interval and may fluctuate. Returns `0.0` if the service is not consuming CPU or if the metric is unavailable.

### `MemoryUsageMb`
The current resident set size (RSS) of the service's main process, expressed in megabytes. This value includes memory shared with other processes. Returns `0` if the service has no active process or if the metric cannot be retrieved.

### `UnitName`
The full name of the systemd unit, including the suffix (e.g., `nginx.service`). This is the identifier used by systemd commands like `systemctl`.

### `Description`
A human-readable description of the service, typically sourced from the unit file's `Description=` directive. May be `null` if the unit file does not define a description.

### `UnitFilePath`
The absolute filesystem path to the unit file on disk. This path may point to a file in `/etc/systemd/system/`, `/usr/lib/systemd/system/`, or a runtime directory. Returns `null` if the unit file cannot be located.

### `State`
The high-level state of the service as reported by systemd. Possible values include `Running`, `Stopped`, `Starting`, `Stopping`, and `Reloading`. This property reflects the service's current operational phase.

### `SubState`
A more granular state that qualifies `State`. For example, a service in `State = Running` might have `SubState = running` or `SubState = auto-restart`. This property provides additional context about the service's behavior.

### `MainProcessId`
The process ID (PID) of the service's main process. Returns `0` if the service is not active or if the PID cannot be determined. This value may change across restarts or reloads.

### `Result`
The outcome of the service's most recent activation attempt. Possible values include `Success`, `Failure`, `Timeout`, and `Dependency`. This property is reset on each start attempt and is useful for diagnosing startup issues.

### `RestartPolicy`
The configured restart behavior for the service, as defined in the unit file. Possible values include `No`, `OnFailure`, `Always`, and `OnAbnormal`. This policy determines whether systemd will automatically restart the service after a crash or exit.

### `AutoStart`
Indicates whether the service is enabled to start at boot. This is equivalent to the `systemctl is-enabled` command. Returns `false` if the service is disabled, static, or masked.

### `Restart`
Indicates whether the service is configured to restart automatically after a clean exit. This is distinct from `RestartPolicy` and reflects the `Restart=` directive in the unit file. Returns `false` if the service is not configured to restart.

### `Dependencies`
A list of unit names that this service depends on for activation. These dependencies are defined in the unit file's `After=` and `Requires=` directives. The list may be empty if the service has no dependencies.

### `Dependents`
A list of unit names that depend on this service for activation. These dependents are defined in other unit files' `After=` or `Requires=` directives. The list may be empty if no other units depend on this service.

### `LastStartTime`
The timestamp of the most recent successful start of the service. Returns `null` if the service has never been started or if the timestamp cannot be retrieved. This value is sourced from systemd's journal or runtime state.

### `LastStopTime`
The timestamp of the most recent stop of the service. Returns `null` if the service has never been stopped or if the timestamp cannot be retrieved. This value is useful for calculating uptime or diagnosing unexpected stops.

### `UptimeSeconds`
The total time, in seconds, that the service has been running continuously. This value is calculated from `LastStartTime` and the current time, and resets to `0` after each stop. Returns `0` if the service is not running.

### `RestartCount`
The number of times the service has been restarted by systemd since the last full boot. This count includes restarts triggered by `RestartPolicy` or manual interventions. Returns `0` if the service has never been restarted.

## Usage

### Example 1: Monitoring Service Health
