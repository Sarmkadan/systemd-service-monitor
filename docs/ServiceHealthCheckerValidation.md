# ServiceHealthCheckerValidation

Provides validation helpers for `ServiceHealthChecker` to ensure service health data is valid.

## Validate

Validates a `ServiceInfo` instance and returns a list of human-readable problems.

```csharp
public static IReadOnlyList<string> Validate(this ServiceInfo value)
```

### Parameters

- `value`: The service information to validate.

### Returns

A read-only list of validation problems; empty if the service is valid.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.

### Remarks

The validation checks the following:

1. **Required string fields**:
   - `UnitName` must not be null, empty, or whitespace.
   - `Description` must not be null, empty, or whitespace.
   - `UnitFilePath` must not be null, empty, or whitespace.
   - `RunAsUser` must not be null, empty, or whitespace.
   - `RunAsGroup` must not be null, empty, or whitespace.
   - `WorkingDirectory` must not be null, empty, or whitespace.

2. **Enum values**:
   - `State` must not be `ServiceState.Unknown`.
   - `LoadState` must not be `ServiceLoadState.Unknown`.
   - `SubState` must not be `ServiceSubState.Unknown`.

3. **Numeric ranges**:
   - `RestartCount` must not be negative.
   - `UptimeSeconds` must not be negative.
   - `CpuUsagePercent` must be between 0 and 100.
   - `MemoryUsageMb` must not be negative.
   - `MainProcessId` must not be negative.

4. **Dates**:
   - `CreatedAt` must not be default `DateTime`.
   - `UpdatedAt` must not be default `DateTime`.
   - `CreatedAt` must not be after `UpdatedAt`.
   - `CreatedAt` must be within reasonable bounds (1 year before/after current date).
   - `UpdatedAt` must be within reasonable bounds (1 year before/after current date).

5. **Lists**:
   - `Dependencies` collection must not be null.
   - `Dependents` collection must not be null.

## IsValid

Determines whether the specified `ServiceInfo` instance is valid.

```csharp
public static bool IsValid(this ServiceInfo value)
```

### Parameters

- `value`: The service information to check.

### Returns

`true` if the service is valid; otherwise, `false`.

### Remarks

This method is a convenience wrapper that calls `Validate` and checks if the returned list is empty.

## EnsureValid

Ensures that the specified `ServiceInfo` instance is valid, throwing an exception if it is not.

```csharp
public static void EnsureValid(this ServiceInfo value)
```

### Parameters

- `value`: The service information to validate.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.
- `ArgumentException`: Thrown when `value` is not valid, containing a list of validation problems.

### Remarks

This method calls `Validate` and throws an `ArgumentException` if any errors are found, providing a consolidated error message.