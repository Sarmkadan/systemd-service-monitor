# LogStreamFilter

`LogStreamFilter` represents a single log entry retrieved from a systemd service journal, optionally filtered by service name, syslog severity, or a textual search term. It is produced by the `FromServiceLog` static factory method and carries both the original journal fields and the filtering criteria that were applied when the entry was captured.

## API

### `public string? ServiceName`

The name of the systemd service unit whose logs are being monitored. When non‑null, only entries originating from this service are returned. A `null` value indicates that no service‑name filter is applied.

### `public string? SearchTerm`

An optional substring to match against the log message. When set, only entries whose `Message` contains this term (case‑insensitive) are included. `null` means no text filter is active.

### `public SyslogLevel? MinLevel`

The minimum syslog severity level required for an entry to be returned. Entries with a severity numerically lower than this value are discarded. `null` disables level filtering, allowing all severities.

### `public int BufferSize`

The maximum number of log entries the internal buffer can hold before older entries are evicted. Must be a positive integer; typical values range from 100 to 10 000 depending on memory constraints and desired history depth.

### `public int PollingIntervalMs`

The interval in milliseconds between successive polls of the systemd journal. Lower values reduce latency but increase CPU usage. Must be greater than zero.

### `public bool IsBuffered`

Indicates whether the log stream is operating in buffered mode. When `true`, entries are accumulated in memory up to `BufferSize` and can be re‑enumerated. When `false`, entries are yielded as they arrive and are not retained after enumeration.

### `public DateTime Timestamp`

The wall‑clock time at which this log entry was recorded by systemd, expressed in UTC.

### `public string UnitName`

The systemd unit name associated with this specific log entry. This may differ from `ServiceName` when the filter is broad (e.g., `ServiceName` is `null`) or when the entry originates from a related scope or slice unit.

### `public SyslogLevel Level`

The syslog severity level of this individual log entry, such as `Info`, `Warning`, or `Error`.

### `public string Message`

The full text body of the log entry as emitted by the service process.

### `public int ProcessId`

The operating‑system process identifier (PID) of the process that wrote this log entry. May be `0` if the journal could not determine the originating PID.

### `public static LogStreamEntry FromServiceLog`

Static factory method that creates a `LogStreamEntry` (the base type of `LogStreamFilter`) from a raw journal record. It accepts a journal cursor or native record handle and populates all standard fields. This method is the primary entry point for constructing instances from systemd journal data.

**Parameters**  
A native journal record reference (opaque handle or cursor) obtained from the systemd journal API.

**Return value**  
A fully populated `LogStreamEntry` instance, which can be further filtered or cast to `LogStreamFilter` when filtering metadata is attached.

**Exceptions**  
Throws `ArgumentNullException` if the record handle is null. Throws `InvalidOperationException` if the journal cursor is positioned at an invalid location or the underlying journal file has been closed.

## Usage

### Example 1: Buffered monitoring with severity filter

```csharp
var filter = new LogStreamFilter
{
    ServiceName = "nginx.service",
    MinLevel = SyslogLevel.Warning,
    BufferSize = 500,
    PollingIntervalMs = 2000,
    IsBuffered = true
};

await foreach (var entry in journalReader.EnumerateEntries(filter))
{
    Console.WriteLine($"[{entry.Timestamp:O}] {entry.Level}: {entry.Message}");
    if (entry.Level <= SyslogLevel.Error)
        AlertingService.SendAlert(entry.UnitName, entry.Message);
}
```

### Example 2: Unbuffered, real‑time search across all services

```csharp
var filter = new LogStreamFilter
{
    SearchTerm = "timeout",
    IsBuffered = false,
    PollingIntervalMs = 500
};

await foreach (var entry in journalReader.EnumerateEntries(filter))
{
    Console.WriteLine($"{entry.UnitName} (PID {entry.ProcessId}): {entry.Message}");
    // Entries are not retained; only immediate reaction is possible.
    if (entry.Message.Contains("critical timeout", StringComparison.OrdinalIgnoreCase))
        await IncidentLogger.LogAsync(entry);
}
```

## Notes

- When `IsBuffered` is `false`, the `BufferSize` property has no effect; entries are streamed directly and cannot be replayed.
- `ServiceName` and `UnitName` may diverge: `ServiceName` is the filter criterion, while `UnitName` reflects the actual unit that emitted the entry. A `null` `ServiceName` means all units are accepted, so `UnitName` can be any valid systemd unit.
- The `MinLevel` filter operates on the numeric syslog priority. `SyslogLevel.Debug` is the lowest priority; setting `MinLevel` to `SyslogLevel.Error` excludes `Warning`, `Info`, and `Debug` entries.
- `FromServiceLog` is a static factory and does not participate in the filtering logic; it merely constructs a raw entry. Filtering is applied later by the journal enumeration infrastructure.
- This type is not thread‑safe. Instances should be used within a single enumeration context. Concurrent enumeration with the same filter instance leads to undefined behaviour, including skipped or duplicated entries.
- `PollingIntervalMs` must be tuned carefully: values below 100 ms can cause excessive journal seeks and CPU load, while values above 10 000 ms may miss short‑lived log bursts.
