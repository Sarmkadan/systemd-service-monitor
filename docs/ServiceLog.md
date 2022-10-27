# ServiceLog
The `ServiceLog` type represents a log entry for a systemd service, encapsulating key information about the service's activity, including the log message, severity level, and associated metadata. It provides a structured way to access and process log data, facilitating monitoring, debugging, and analysis of systemd services.

## API
* `public Guid Id`: A unique identifier for the log entry.
* `public Guid ServiceInfoId`: The identifier of the associated `ServiceInfo` object.
* `public string UnitName`: The name of the systemd unit (e.g., service, socket, timer) that generated the log entry.
* `public SyslogLevel Level`: The severity level of the log message (e.g., Debug, Info, Warning, Error, Critical).
* `public string Message`: The log message itself.
* `public int ProcessId`: The process ID of the process that generated the log entry.
* `public int UserId`: The user ID of the user that generated the log entry.
* `public string Hostname`: The hostname of the system that generated the log entry.
* `public string CodeFile`: The file name of the code that generated the log entry.
* `public int CodeLine`: The line number in the code file where the log entry was generated.
* `public string CodeFunction`: The name of the function that generated the log entry.
* `public string? ErrNo`: An optional error number associated with the log entry.
* `public string? MessageId`: An optional message ID associated with the log entry.
* `public ulong Sequence`: A sequence number for the log entry.
* `public string BootId`: The boot ID of the system that generated the log entry.
* `public DateTime Timestamp`: The timestamp when the log entry was generated.
* `public Dictionary<string, string> Metadata`: Additional metadata associated with the log entry.
* `public override string ToString()`: Returns a string representation of the log entry.

## Usage
```csharp
// Example 1: Creating a new ServiceLog instance
var serviceLog = new ServiceLog
{
    Id = Guid.NewGuid(),
    ServiceInfoId = Guid.NewGuid(),
    UnitName = "my-service",
    Level = SyslogLevel.Info,
    Message = "Service started successfully",
    ProcessId = 1234,
    UserId = 5678,
    Hostname = "my-host",
    CodeFile = "MyService.cs",
    CodeLine = 42,
    CodeFunction = "StartService",
    Timestamp = DateTime.UtcNow
};

Console.WriteLine(serviceLog.ToString());

// Example 2: Processing a list of ServiceLog entries
var logEntries = new List<ServiceLog>
{
    new ServiceLog { Level = SyslogLevel.Error, Message = "Error occurred" },
    new ServiceLog { Level = SyslogLevel.Info, Message = "Service started" },
    new ServiceLog { Level = SyslogLevel.Debug, Message = "Debug information" }
};

foreach (var logEntry in logEntries)
{
    Console.WriteLine($"Level: {logEntry.Level}, Message: {logEntry.Message}");
}
```

## Notes
When working with `ServiceLog` instances, note that the `Metadata` dictionary can be null if no metadata is associated with the log entry. Additionally, the `ErrNo` and `MessageId` properties are optional and may be null if not provided. The `ToString()` method returns a string representation of the log entry, which can be useful for logging or debugging purposes. The `ServiceLog` type is not thread-safe, so care should be taken when accessing or modifying instances from multiple threads.
