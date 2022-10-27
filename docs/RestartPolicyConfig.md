# RestartPolicyConfig

`RestartPolicyConfig` is a data model that defines the restart behavior and constraints for a systemd service monitored by the service monitor. It encapsulates all parameters that control when and how a service should be restarted after a failure, including delays, burst limits, timeouts, and optional pre/post restart commands. This type is typically persisted in a database and associated with a specific service via the `ServiceInfoId` foreign key.

## API

### `public Guid Id`
- **Purpose**: Unique identifier for this restart policy configuration record.
- **Type**: `System.Guid`
- **Throws**: None (property getter/setter).

### `public Guid ServiceInfoId`
- **Purpose**: Foreign key referencing the associated service information record (`ServiceInfo`). Links this restart policy to a specific monitored service.
- **Type**: `System.Guid`
- **Throws**: None.

### `public RestartPolicy PolicyType`
- **Purpose**: The type of restart policy to apply (e.g., `Always`, `OnFailure`, `OnAbnormal`, etc.). Determines under which conditions the service should be restarted.
- **Type**: `RestartPolicy` (enum)
- **Throws**: None.

### `public int RestartDelaySec`
- **Purpose**: Delay in seconds before attempting a restart after a service failure.
- **Type**: `int`
- **Throws**: None.

### `public int MaxRestarts`
- **Purpose**: Maximum number of restart attempts allowed within the `RestartWindowSec` period before the service is considered failed.
- **Type**: `int`
- **Throws**: None.

### `public int RestartWindowSec`
- **Purpose**: Time window in seconds during which `MaxRestarts` restart attempts are counted. After this window, the restart counter resets.
- **Type**: `int`
- **Throws**: None.

### `public int StartLimitIntervalSec`
- **Purpose**: Time interval in seconds used for the start rate limiting. If the service is started more than `StartLimitBurst` times within this interval, further starts are denied.
- **Type**: `int`
- **Throws**: None.

### `public int StartLimitBurst`
- **Purpose**: Maximum number of start attempts allowed within the `StartLimitIntervalSec` interval before rate limiting kicks in.
- **Type**: `int`
- **Throws**: None.

### `public int TimeoutStartSec`
- **Purpose**: Maximum time in seconds allowed for the service to reach a running state after a start command. If exceeded, the start is considered failed.
- **Type**: `int`
- **Throws**: None.

### `public int TimeoutStopSec`
- **Purpose**: Maximum time in seconds allowed for the service to stop gracefully after a stop command. If exceeded, the service may be forcefully terminated.
- **Type**: `int`
- **Throws**: None.

### `public RestartStrategy RestartStrategy`
- **Purpose**: Defines the strategy for restarting the service (e.g., `Immediate`, `Delayed`, `ExponentialBackoff`). Controls the timing and pattern of restart attempts.
- **Type**: `RestartStrategy` (enum)
- **Throws**: None.

### `public bool IsEnabled`
- **Purpose**: Indicates whether this restart policy is currently active. When `false`, the service monitor will not apply the policy.
- **Type**: `bool`
- **Throws**: None.

### `public string? PreRestartCommand`
- **Purpose**: An optional shell command to execute before each restart attempt. Can be used for cleanup, logging, or preparation tasks. `null` if no command is defined.
- **Type**: `string?`
- **Throws**: None.

### `public string? PostRestartCommand`
- **Purpose**: An optional shell command to execute after each successful restart. `null` if no command is defined.
- **Type**: `string?`
- **Throws**: None.

### `public bool NotifyOnRestart`
- **Purpose**: If `true`, the service monitor will send a notification (e.g., via configured alerting channels) each time a restart occurs.
- **Type**: `bool`
- **Throws**: None.

### `public bool TrackRestartHistory`
- **Purpose**: If `true`, the service monitor will record detailed history of restart events for this service, enabling later analysis.
- **Type**: `bool`
- **Throws**: None.

### `public DateTime CreatedAt`
- **Purpose**: Timestamp indicating when this restart policy configuration was first created.
- **Type**: `System.DateTime`
- **Throws**: None.

### `public DateTime UpdatedAt`
- **Purpose**: Timestamp indicating the last time this restart policy configuration was modified.
- **Type**: `System.DateTime`
- **Throws**: None.

### `public override string ToString()`
- **Purpose**: Returns a human-readable string representation of the restart policy configuration, typically including the `Id`, `ServiceInfoId`, and `PolicyType`.
- **Parameters**: None.
- **Return value**: `string` – a formatted summary of the instance.
- **Throws**: None.

## Usage

### Example 1: Creating and configuring a restart policy for a web service

```csharp
var policy = new RestartPolicyConfig
{
    Id = Guid.NewGuid(),
    ServiceInfoId = serviceInfoId,
    PolicyType = RestartPolicy.OnFailure,
    RestartDelaySec = 5,
    MaxRestarts = 3,
    RestartWindowSec = 60,
    StartLimitIntervalSec = 120,
    StartLimitBurst = 5,
    TimeoutStartSec = 30,
    TimeoutStopSec = 10,
    RestartStrategy = RestartStrategy.Delayed,
    IsEnabled = true,
    PreRestartCommand = "echo 'Restarting service...' >> /var/log/restarts.log",
    PostRestartCommand = null,
    NotifyOnRestart = true,
    TrackRestartHistory = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

Console.WriteLine(policy.ToString());
// Output example: "RestartPolicyConfig { Id = 3f4a... , ServiceInfoId = b1c2... , PolicyType = OnFailure }"
```

### Example 2: Updating an existing policy and disabling it temporarily

```csharp
// Assume 'existingPolicy' was loaded from a data store
existingPolicy.IsEnabled = false;
existingPolicy.NotifyOnRestart = false;
existingPolicy.UpdatedAt = DateTime.UtcNow;

// Save changes to the repository
await repository.UpdateAsync(existingPolicy);

// The service monitor will now ignore this policy until re-enabled.
```

## Notes

- **Nullable commands**: `PreRestartCommand` and `PostRestartCommand` are nullable. When set to `null`, no command is executed before or after restarts. An empty string is treated as a valid command (though it may result in a no-op depending on the execution environment). Always check for `null` before invoking a command.
- **Integer fields**: All integer fields (`RestartDelaySec`, `MaxRestarts`, etc.) are expected to be non-negative. Negative values may cause undefined behavior in the service monitor logic. Validation should be performed before persisting.
- **Thread safety**: `RestartPolicyConfig` is a mutable data object. It is not thread-safe for concurrent reads and writes. If instances are shared across threads (e.g., in a cache or background processing pipeline), external synchronization (e.g., locking or immutable snapshots) is required.
- **Timestamps**: `CreatedAt` and `UpdatedAt` are intended to be set by the application layer. The service monitor does not automatically update them; the caller is responsible for maintaining consistency.
- **Enum values**: The `RestartPolicy` and `RestartStrategy` enums are defined elsewhere in the project. Ensure that the values used are valid according to the service monitor’s supported set. Unrecognized values may be ignored or cause exceptions at runtime.
