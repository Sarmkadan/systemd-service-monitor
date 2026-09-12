# Log stream DTOs

`Dtos/LogStreamDto.cs` defines the request settings and response item used by the real-time systemd log stream. Both types are sealed and expose init-only properties, so they are normally configured with object initializers and then treated as immutable.

## `LogStreamFilter`

`LogStreamFilter` describes which records to emit and how the stream should replay and poll for records.

### Properties

| Property | Type | Default | Meaning |
| --- | --- | --- | --- |
| `ServiceName` | `string?` | `null` | Restricts results to one systemd unit. A null, empty, or whitespace value selects logs across all services. |
| `SearchTerm` | `string?` | `null` | Case-insensitive substring matched against `ServiceLog.Message`. A null, empty, or whitespace value disables text filtering. |
| `MinLevel` | `SyslogLevel?` | `null` | Includes entries whose numeric syslog value is less than or equal to the threshold. Because lower numbers are more severe, `Error` includes `Emergency`, `Alert`, `Critical`, and `Error`. A null value disables severity filtering. |
| `BufferSize` | `int` | `50` | Number of recent matching records replayed before live polling starts. `LogStreamService` clamps the effective value to the inclusive range `0` through `500`; `0` disables replay. |
| `PollingIntervalMs` | `int` | `2000` | Delay in milliseconds between live-log polls. `LogStreamService` clamps the effective value to the inclusive range `500` through `30000`. |

The DTO itself does not perform clamping. Consequently, reading `BufferSize` or `PollingIntervalMs` from a filter returns the assigned value even when the streaming service uses a clamped value. The separate validation extensions can report out-of-range settings when callers want to reject them instead.

### Example

```csharp
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;

var filter = new LogStreamFilter
{
    ServiceName = "nginx.service",
    SearchTerm = "timeout",
    MinLevel = SyslogLevel.Warning,
    BufferSize = 100,
    PollingIntervalMs = 1000
};
```

This filter requests up to 100 recent matching entries, then polls once per second. The severity threshold accepts `Emergency` through `Warning`; it excludes `Notice`, `Info`, and `Debug`.

## `LogStreamEntry`

`LogStreamEntry` is the compact record emitted by the stream. It copies the fields needed by streaming clients from the larger `ServiceLog` model.

### Properties

| Property | Type | Default | Meaning |
| --- | --- | --- | --- |
| `IsBuffered` | `bool` | `false` | `true` for an entry from the initial historical replay; `false` for an entry found during live polling. |
| `Timestamp` | `DateTime` | `default` | Timestamp carried by the source `ServiceLog`; journald records are expected to use UTC. |
| `UnitName` | `string` | Empty string | systemd unit that produced the entry. |
| `Level` | `SyslogLevel` | `Emergency` (`0`) | Syslog severity copied from the source record. |
| `Message` | `string` | Empty string | Human-readable log message. |
| `ProcessId` | `int` | `0` | Identifier of the process that wrote the entry. |

### `FromServiceLog`

```csharp
public static LogStreamEntry FromServiceLog(ServiceLog log, bool isBuffered)
```

Creates a new stream entry by copying `Timestamp`, `UnitName`, `Level`, `Message`, and `ProcessId` from `log`, and assigns the supplied replay flag to `IsBuffered`. It does not retain a reference to the source object and does not copy fields such as `Id`, `Hostname`, `UserId`, journal sequence information, or metadata.

```csharp
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;

var log = new ServiceLog
{
    UnitName = "nginx.service",
    Level = SyslogLevel.Error,
    Message = "Upstream connection timed out",
    ProcessId = 2718,
    Timestamp = DateTime.UtcNow
};

LogStreamEntry entry = LogStreamEntry.FromServiceLog(log, isBuffered: true);
```

`FromServiceLog` expects a non-null `ServiceLog`. It has no explicit null guard, so callers should validate the argument before invoking it.

## Streaming behavior

`ILogStreamService.StreamLogsAsync` accepts a `LogStreamFilter` and returns an `IAsyncEnumerable<LogStreamEntry>`. It emits matching historical entries first in timestamp order with `IsBuffered` set to `true`, then continues polling until cancellation and marks newly observed entries as unbuffered.

```csharp
await foreach (var entry in logStreamService.StreamLogsAsync(filter, cancellationToken))
{
    var source = entry.IsBuffered ? "history" : "live";
    Console.WriteLine($"[{source}] {entry.Timestamp:O} {entry.UnitName}: {entry.Message}");
}
```

The HTTP log-streaming endpoint serializes each `LogStreamEntry` as an SSE `data` frame. Consumers should use `IsBuffered` to distinguish the initial replay from live observations rather than inferring this from timestamps.
