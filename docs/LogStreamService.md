# `LogStreamService`

`LogStreamService` implements `ILogStreamService` as an asynchronous, polling-based stream of systemd journal records. Each subscription optionally replays a historical buffer and then tails the log repository until cancellation.

The interface and implementation are defined together in `Services/LogStreamService.cs`. Stream configuration and emitted records are represented by `LogStreamFilter` and `LogStreamEntry` in `Dtos/LogStreamDto.cs`.

## Registration and dependencies

`AddLogStreaming()` registers `ILogStreamService` as a scoped service implemented by `LogStreamService`. Each instance requires:

- `IServiceLogService` to query stored service logs.
- `ILogger<LogStreamService>` to record stream lifecycle and query failures.

The constructor throws `ArgumentNullException` when either dependency is null.

## Streaming contract

### `StreamLogsAsync(LogStreamFilter filter, CancellationToken ct = default)`

Returns an `IAsyncEnumerable<LogStreamEntry>`. Work begins when a caller enumerates the sequence, not when the method is called. The enumeration has two phases:

1. If the effective buffer size is greater than zero, recent matching records are replayed with `IsBuffered` set to `true`.
2. The service polls for newer records and emits them with `IsBuffered` set to `false` until the cancellation token is cancelled.

Entries within each fetched batch are emitted in ascending timestamp order. `LogStreamEntry.FromServiceLog` copies the timestamp, unit name, severity, message, and process ID from the repository model.

`filter` is expected to be non-null. The method does not call `LogStreamFilter.Validate()` or `EnsureValid()`; passing null results in a `NullReferenceException` when enumeration starts.

## Effective settings

The service clamps numeric filter settings before use:

| Setting | Effective range | Default from `LogStreamFilter` |
| --- | --- | --- |
| `BufferSize` | 0 through 500 | 50 |
| `PollingIntervalMs` | 500 through 30,000 milliseconds | 2,000 milliseconds |

A buffer size of zero disables historical replay. Values outside either range do not cause this service to throw; they are replaced with the nearest bound. Validation performed by an API layer may reject the same values before they reach the service.

## Historical replay

The initial query depends on `ServiceName`:

- A nonblank service name calls `GetServiceLogsAsync(serviceName, bufferSize, ct)`.
- A null, empty, or whitespace service name calls `SearchLogsAsync(searchTerm ?? string.Empty, bufferSize, ct)` to query across services.

The returned records are then filtered locally by `MinLevel` and `SearchTerm`. Consequently, the service-specific query may return fewer than `bufferSize` entries after text or severity filtering. The cross-service query already receives the search term, but the same case-insensitive message filter is applied again locally.

Cancellation during the initial query ends the sequence normally. Any other initial-query exception is logged as an error and also ends the sequence without rethrowing it.

## Live polling

The live cursor starts at `DateTime.UtcNow` immediately before historical replay. Historical entries do not move it, so replayed records are not treated as live records.

After each polling delay, the service captures a UTC window end and fetches a batch:

- For one service, it calls `GetLogsInTimeRangeAsync(serviceName, cursor, windowEnd, ct)`.
- Across all services, it calls `SearchLogsAsync(searchTerm ?? string.Empty, 500, ct)` and retains records whose timestamps are strictly later than the cursor and no later than the window end.

The local severity and text filters are applied to both paths. Syslog levels use lower numbers for greater severity, so a `MinLevel` includes entries whose numeric level is less than or equal to the configured value. For example, `Warning` includes Emergency through Warning and excludes Notice, Info, and Debug. Text matching is a case-insensitive ordinal substring match against non-null messages.

After emitting a batch, the cursor advances to the newest emitted timestamp and then to the window end when that is later. Advancing on an empty or fully filtered batch prevents the next poll from repeatedly querying the same stale window.

Cancellation during a delay or repository query ends enumeration normally. Other live-query exceptions are logged as warnings; the cursor is left unchanged and the service retries after the next polling interval. Because the retry window starts from the previous cursor, records are not deliberately skipped after a transient query failure.

## Consumption and disposal

Callers should pass a cancellation token and consume the sequence with `await foreach`. Stopping enumeration also disposes the asynchronous iterator. The final "stream closed" information log is written when the polling loop exits normally through cancellation; it is not reached when the initial replay returns early after cancellation or an error.

```csharp
var filter = new LogStreamFilter
{
    ServiceName = "nginx.service",
    SearchTerm = "error",
    MinLevel = SyslogLevel.Warning,
    BufferSize = 25,
    PollingIntervalMs = 1_000
};

await foreach (var entry in logStreamService.StreamLogsAsync(filter, cancellationToken))
{
    Console.WriteLine($"{entry.Timestamp:O} [{entry.Level}] {entry.Message}");
}
```

The mapped `GET /api/stream/logs` endpoint uses this stream to send each entry as a Server-Sent Events `data` frame. That transport is provided by `MapLogStreamEndpoints()` rather than by `LogStreamService` itself.
