# ServiceLogExtensions

Provides a set of static extension methods and properties for working with log levels in the `systemd-service-monitor` project. These members simplify common checks and formatting operations on `LogLevel` values, enabling concise filtering and display of log entries.

## API

### `IsErrorOrHigher`

```csharp
public static bool IsErrorOrHigher(this LogLevel level)
```

Returns `true` if the specified `level` is **Error** or **Critical** (i.e., at or above the Error severity).  
Returns `false` for all lower severities (Warning, Information, Debug, Trace).

- **Parameters**  
  `level` – The `LogLevel` value to evaluate.

- **Returns**  
  `true` if `level >= LogLevel.Error`; otherwise `false`.

- **Throws**  
  None. The method does not throw exceptions.

### `IsWarningOrHigher`

```csharp
public static bool IsWarningOrHigher(this LogLevel level)
```

Returns `true` if the specified `level` is **Warning**, **Error**, or **Critical** (i.e., at or above the Warning severity).  
Returns `false` for Information, Debug, and Trace.

- **Parameters**  
  `level` – The `LogLevel` value to evaluate.

- **Returns**  
  `true` if `level >= LogLevel.Warning`; otherwise `false`.

- **Throws**  
  None.

### `ToDetailedString`

```csharp
public static string ToDetailedString(this LogLevel level)
```

Returns a detailed, human-readable string representation of the log level, including the level name and its numeric severity value (if applicable). The exact format is implementation-defined but is intended for diagnostic or debugging output.

- **Parameters**  
  `level` – The `LogLevel` value to format.

- **Returns**  
  A non-null, non-empty string containing detailed information about the level.

- **Throws**  
  None.

### `HasLevel`

```csharp
public static bool HasLevel(this LogLevel level, LogLevel target)
```

Determines whether the log entry represented by `level` is at the exact severity specified by `target`. This is a strict equality check, not a range comparison.

- **Parameters**  
  `level` – The `LogLevel` value to test.  
  `target` – The `LogLevel` value to compare against.

- **Returns**  
  `true` if `level == target`; otherwise `false`.

- **Throws**  
  None.

### `ToSummaryString`

```csharp
public static string ToSummaryString(this LogLevel level)
```

Returns a short, concise string representation of the log level (e.g., `"ERR"`, `"WRN"`, `"INF"`). This is intended for use in compact log output or UI elements where space is limited.

- **Parameters**  
  `level` – The `LogLevel` value to format.

- **Returns**  
  A non-null, non-empty string containing the abbreviated level name.

- **Throws**  
  None.

## Usage

### Example 1: Filtering log entries by severity

```csharp
using SystemdServiceMonitor;

public void ProcessLogEntry(LogEntry entry)
{
    if (entry.Level.IsErrorOrHigher())
    {
        // Send alert for errors and criticals
        AlertService.Notify(entry);
    }
    else if (entry.Level.IsWarningOrHigher())
    {
        // Log warning details
        Logger.Warn(entry.ToDetailedString());
    }
    else
    {
        // Summarize informational messages
        Console.WriteLine(entry.Level.ToSummaryString());
    }
}
```

### Example 2: Checking for a specific level and formatting

```csharp
using SystemdServiceMonitor;

public string FormatLevel(LogLevel level)
{
    if (level.HasLevel(LogLevel.Critical))
    {
        return "CRITICAL";
    }

    // Use detailed string for debugging
    if (level.IsErrorOrHigher())
    {
        return level.ToDetailedString();
    }

    // Use summary for all other levels
    return level.ToSummaryString();
}
```

## Notes

- All members are static extension methods on the `LogLevel` type. Because `LogLevel` is a value type (enum), no null checks are performed; passing an undefined enum value will still produce a result (the behavior is defined by the underlying integer value).
- The methods are thread-safe: they operate solely on their input parameters and do not access any shared mutable state.
- `ToDetailedString` and `ToSummaryString` always return a non-null string. If the `LogLevel` value is not a known member of the enum, the output may fall back to the numeric representation.
- `HasLevel` performs an exact equality comparison; it does not consider severity ordering. Use `IsErrorOrHigher` or `IsWarningOrHigher` for range checks.
