# AlertRuleValidation

Provides validation helpers for `AlertRule` instances.

## Validate

Validates an `AlertRule` and returns a list of human-readable problems.

```csharp
public static IReadOnlyList<string> Validate(this AlertRule value)
```

### Parameters

- `value`: The rule to validate.

### Returns

A read-only list of validation errors; empty if the rule is valid.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.

### Remarks

The validation checks the following:

1. **Required properties**:
   - `Name` must not be null, empty, or whitespace.
   - `ServicePattern` must not be null, empty, or whitespace and cannot contain whitespace except for the wildcard character '*'.
   - `Condition` must not be the default value.

2. **Numeric properties based on condition type**:
   - For conditions `CpuThresholdExceeded`, `MemoryThresholdExceeded`, `RestartCountExceeded`, and `UptimeBelowMinimum`, the `Threshold` must be greater than 0.

3. **Severity**:
   - Must be a valid `AlertSeverity` value.

4. **Cooldown minutes**:
   - Cannot be negative.
   - Cannot exceed 1440 (24 hours).

5. **Consecutive evaluations required**:
   - Must be at least 1.
   - Cannot exceed 100.

6. **Tags collection**:
   - Cannot be null.
   - Cannot contain null, empty, or whitespace entries.

7. **Timestamps**:
   - `CreatedAt` cannot be default `DateTime`.
   - `UpdatedAt` cannot be default `DateTime`.

## IsValid

Determines whether an `AlertRule` is valid.

```csharp
public static bool IsValid(this AlertRule value)
```

### Parameters

- `value`: The rule to check.

### Returns

`true` if the rule is valid; otherwise, `false`.

### Remarks

This method is a convenience wrapper that calls `Validate` and checks if the returned list is empty.

## EnsureValid

Ensures that an `AlertRule` is valid, throwing an `ArgumentException` with a detailed message listing all validation problems if it is not.

```csharp
public static void EnsureValid(this AlertRule value)
```

### Parameters

- `value`: The rule to validate.

### Exceptions

- `ArgumentNullException`: Thrown when `value` is `null`.
- `ArgumentException`: Thrown when the rule has validation errors. The message includes a list of all validation problems.

### Remarks

This method calls `Validate` and throws an `ArgumentException` if any errors are found, providing a consolidated error message.