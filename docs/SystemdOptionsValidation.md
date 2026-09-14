# SystemdOptionsValidation

Provides validation helpers for `SystemdOptions` instances.

## Validate

Validates a `SystemdOptions` and returns a list of human-readable problems.

```csharp
public static IReadOnlyList<string> Validate(this SystemdOptions value)
```

### Parameters

- `value`: The options to validate.

### Returns

A read-only list of validation errors; empty if the options are valid.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.

### Remarks

The validation checks the following:

1. **MetricCollectionIntervalMs**:
   - Must be positive (> 0)
   - Cannot exceed 1 hour (3600000ms)

2. **LogRetentionDays**:
   - Cannot be negative (< 0)
   - Cannot exceed 10 years (3650 days)

3. **MaxLogEntriesPerRequest**:
   - Must be positive (> 0)
   - Cannot exceed 100000 entries

4. **OperationTimeoutMs**:
   - Must be positive (> 0)
   - Cannot exceed 5 minutes (300000ms)

5. **ConnectionRetryCount**:
   - Cannot be negative (< 0)
   - Cannot exceed 20

6. **ConnectionRetryDelayMs**:
   - Must be positive (> 0)
   - Cannot exceed 1 minute (60000ms)

Note that boolean properties (`EnableMonitoring`, `EnableRemoteOperations`, `EnableHealthChecks`) require no specific validation.

## IsValid

Determines whether a `SystemdOptions` instance is valid.

```csharp
public static bool IsValid(this SystemdOptions value)
```

### Parameters

- `value`: The options to check.

### Returns

`true` if the options are valid; otherwise, `false`.

### Remarks

This method is a convenience wrapper that calls `Validate` and checks if the returned list is empty.

## EnsureValid

Ensures that a `SystemdOptions` instance is valid, throwing an `ArgumentException` with a detailed message listing all validation problems if it is not.

```csharp
public static void EnsureValid(this SystemdOptions value)
```

### Parameters

- `value`: The options to validate.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.
- `ArgumentException`: Thrown when the options have validation errors. The message includes a list of all validation problems.

### Remarks

This method calls `Validate` and throws an `ArgumentException` if any errors are found, providing a consolidated error message.