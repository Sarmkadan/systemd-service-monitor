# DateTimeExtensionsValidation

Provides static validation methods for `DateTime` values, enabling consistent validation logic across the service monitor. The type offers three validation approaches: collecting error messages, boolean checks, and exception-throwing guards, each with overloads supporting optional parameter naming for richer diagnostics.

## API

### `public static IReadOnlyList<string> Validate(DateTime value)`

Validates the specified `DateTime` value and returns a list of error messages.

**Parameters**  
- `value` — The `DateTime` to validate.

**Returns**  
An `IReadOnlyList<string>` containing zero or more validation error messages. An empty list indicates the value is valid.

**Throws**  
This method does not throw.

---

### `public static IReadOnlyList<string> Validate(DateTime value, string paramName)`

Validates the specified `DateTime` value and returns a list of error messages, incorporating the parameter name into any generated messages.

**Parameters**  
- `value` — The `DateTime` to validate.  
- `paramName` — The name of the parameter being validated, used in error messages.

**Returns**  
An `IReadOnlyList<string>` containing zero or more validation error messages. An empty list indicates the value is valid.

**Throws**  
This method does not throw.

---

### `public static bool IsValid(DateTime value)`

Determines whether the specified `DateTime` value is valid.

**Parameters**  
- `value` — The `DateTime` to validate.

**Returns**  
`true` if the value passes all validation rules; otherwise, `false`.

**Throws**  
This method does not throw.

---

### `public static bool IsValid(DateTime value, string paramName)`

Determines whether the specified `DateTime` value is valid. The `paramName` is accepted for API consistency but does not affect the boolean result.

**Parameters**  
- `value` — The `DateTime` to validate.  
- `paramName` — The name of the parameter being validated (unused in result).

**Returns**  
`true` if the value passes all validation rules; otherwise, `false`.

**Throws**  
This method does not throw.

---

### `public static void EnsureValid(DateTime value)`

Validates the specified `DateTime` value and throws an exception if validation fails.

**Parameters**  
- `value` — The `DateTime` to validate.

**Throws**  
`ArgumentException` — If `value` fails validation. The exception message contains the validation error details.

---

### `public static void EnsureValid(DateTime value, string paramName)`

Validates the specified `DateTime` value and throws an exception with the parameter name if validation fails.

**Parameters**  
- `value` — The `DateTime` to validate.  
- `paramName` — The name of the parameter being validated, included in the exception.

**Throws**  
`ArgumentException` — If `value` fails validation. The exception message includes `paramName` and the validation error details.

## Usage

### Example 1: Collecting validation errors for user-facing feedback

```csharp
var scheduledStart = DateTime.Parse(userInput);

var errors = DateTimeExtensionsValidation.Validate(scheduledStart, nameof(scheduledStart));
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        logger.LogWarning("Invalid schedule time: {Error}", error);
    }
    return BadRequest(new { Errors = errors });
}

// Proceed with valid scheduledStart
```

### Example 2: Guard clause at method entry with parameter context

```csharp
public void ScheduleServiceRestart(string serviceName, DateTime restartTime)
{
    DateTimeExtensionsValidation.EnsureValid(restartTime, nameof(restartTime));

    _scheduler.Enqueue(serviceName, restartTime);
}
```

## Notes

- **Validation rules** are not specified in the signature but typically include checks for `DateTime.MinValue`, `DateTime.MaxValue`, `Kind` consistency (e.g., rejecting `Unspecified` when UTC is required), and range constraints relevant to systemd timer precision.
- **Thread safety**: All methods are pure static functions with no shared state; they are inherently thread-safe.
- **Performance**: `Validate` allocates a list only when errors exist (implementation-dependent). `IsValid` avoids allocation entirely. `EnsureValid` allocates only on the exceptional path.
- **Parameter naming**: Overloads accepting `paramName` improve diagnostic quality in logs and exception messages but do not alter validation logic.
- **Edge cases**: `DateTime.MinValue` and `DateTime.MaxValue` are commonly treated as invalid for scheduling purposes. Values with `DateTimeKind.Unspecified` may be rejected if the monitor requires explicit UTC or local time semantics.
