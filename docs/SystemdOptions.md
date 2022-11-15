# SystemdOptions

`SystemdOptions` is the central configuration class for the `systemd-service-monitor` library. It holds all tunable parameters that govern monitoring behavior, remote operation capabilities, logging, database connectivity, and general service metadata. An instance of this class is typically bound from application settings (e.g., `appsettings.json`) and injected into the monitor’s startup pipeline.

## API

### EnableMonitoring
`public bool EnableMonitoring`

Controls whether the background monitoring loop is active. When `false`, metric collection, health checks, and log pruning are suspended, but the service host itself remains running.

### MetricCollectionIntervalMs
`public int MetricCollectionIntervalMs`

The interval, in milliseconds, at which the monitor polls systemd units for performance counters and status changes. Must be a positive integer; values below 100 ms may cause excessive CPU usage on systems with many units.

### LogRetentionDays
`public int LogRetentionDays`

Number of days that collected log entries are retained in the persistent store before being purged by the maintenance job. A value of zero disables automatic pruning entirely.

### MaxLogEntriesPerRequest
`public int MaxLogEntriesPerRequest`

Upper limit on the number of log entries returned by a single query to the log API endpoint. Used to prevent unintentional large payloads and to enforce pagination boundaries.

### EnableRemoteOperations
`public bool EnableRemoteOperations`

Toggles the ability to issue start, stop, restart, and reload commands to systemd units through the monitor’s remote interface. When `false`, all mutating endpoints return `403 Forbidden`.

### OperationTimeoutMs
`public int OperationTimeoutMs`

Timeout in milliseconds applied to each individual systemd operation (start, stop, etc.). If the operation does not complete within this window, the call is aborted and a timeout error is raised.

### ConnectionRetryCount
`public int ConnectionRetryCount`

Maximum number of consecutive attempts to establish a lost connection to the backing database or message broker before the circuit is opened and an alert is raised.

### ConnectionRetryDelayMs
`public int ConnectionRetryDelayMs`

Delay in milliseconds between connection retry attempts. Used in conjunction with `ConnectionRetryCount` to implement a linear back-off strategy.

### EnableHealthChecks
`public bool EnableHealthChecks`

When `true`, the monitor exposes health-check endpoints and periodically evaluates the reachability of systemd’s D-Bus interface, database connectivity, and internal queue lengths.

### ConnectionString
`public string ConnectionString`

The database or storage connection string used for persisting metrics, logs, and configuration state. The format must be compatible with the provider specified in `Provider`.

### Provider
`public string Provider`

Identifies the storage provider. Expected values are invariant strings such as `"PostgreSQL"`, `"SqlServer"`, `"MySQL"`, or `"InMemory"`. Changing this value at runtime without a restart may leave orphaned connections.

### EnableLogging
`public bool EnableLogging`

Master switch for the library’s internal diagnostic logging. When `false`, all log output from the monitor is suppressed regardless of the configured logging level.

### CommandTimeoutSeconds
`public int CommandTimeoutSeconds`

Timeout in seconds for database commands executed by the monitor. This applies to both read and write operations. A value of zero indicates no timeout.

### MaxConnectionPoolSize
`public int MaxConnectionPoolSize`

Maximum number of connections the monitor may hold open simultaneously in its internal connection pool. Exceeding this limit causes requests to be queued until a connection is released.

### QueryCacheExpirationMinutes
`public int QueryCacheExpirationMinutes`

Lifetime, in minutes, of cached query results for repeated read operations (e.g., unit lists, status summaries). A value of zero disables caching entirely.

### Enabled
`public bool Enabled`

Top-level toggle that determines whether the entire `systemd-service-monitor` extension is loaded into the host application. When `false`, no services are registered and no resources are consumed.

### Title
`public string Title`

Human-readable title displayed in administrative dashboards, logs, and documentation headers. Purely cosmetic; has no behavioral effect.

### Version
`public string Version`

Semantic version string of the monitor configuration schema. Used by migration routines to detect and apply configuration upgrades. Should match the assembly version of the library.

### Description
`public string Description`

Free-text description of the monitor instance’s purpose, typically shown in UI footers and API metadata endpoints. Optional and informational only.

## Usage

### Example 1: Binding from appsettings.json

```csharp
// appsettings.json fragment:
// "SystemdMonitor": {
//     "EnableMonitoring": true,
//     "MetricCollectionIntervalMs": 5000,
//     "LogRetentionDays": 30,
//     "ConnectionString": "Host=localhost;Database=monitor;Username=app;Password=secret",
//     "Provider": "PostgreSQL",
//     "EnableRemoteOperations": false
// }

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SystemdOptions>(
    builder.Configuration.GetSection("SystemdMonitor"));
builder.Services.AddSystemdMonitor();
```

### Example 2: Programmatic configuration for an integration test

```csharp
var options = new SystemdOptions
{
    Enabled = true,
    EnableMonitoring = true,
    MetricCollectionIntervalMs = 1000,
    LogRetentionDays = 7,
    MaxLogEntriesPerRequest = 50,
    EnableRemoteOperations = true,
    OperationTimeoutMs = 30000,
    ConnectionRetryCount = 3,
    ConnectionRetryDelayMs = 2000,
    EnableHealthChecks = true,
    ConnectionString = "DataSource=:memory:",
    Provider = "InMemory",
    EnableLogging = false,
    CommandTimeoutSeconds = 10,
    MaxConnectionPoolSize = 5,
    QueryCacheExpirationMinutes = 1,
    Title = "Test Instance",
    Version = "1.0.0",
    Description = "Ephemeral monitor for integration tests"
};

var monitor = new SystemdMonitor(options);
await monitor.StartAsync(CancellationToken.None);
```

## Notes

- All integer fields representing intervals or timeouts are expected to be non-negative. Negative values are coerced to zero internally but should be avoided in configuration to prevent unintended behavior (e.g., infinite retry loops or zero-interval polling).
- `ConnectionString` and `Provider` are interdependent. Specifying a connection string that does not match the provider’s expected format will cause a `ProviderConfigurationException` at initialization.
- `EnableMonitoring` and `Enabled` are independent; setting `Enabled = false` prevents the entire subsystem from loading, whereas `EnableMonitoring = false` loads the subsystem but keeps the monitoring loop dormant.
- This class is not thread-safe for mutation. All properties should be set during startup before the monitor is started. Changing values at runtime may lead to race conditions in the monitoring loop, connection pool, or cache invalidation timer.
- `MaxLogEntriesPerRequest` works in tandem with the API layer’s pagination; setting it to an excessively high value may cause query timeouts if `CommandTimeoutSeconds` is not adjusted accordingly.
- When `EnableRemoteOperations` is enabled, ensure that `OperationTimeoutMs` is shorter than any upstream HTTP request timeout to avoid orphaned systemd jobs.
