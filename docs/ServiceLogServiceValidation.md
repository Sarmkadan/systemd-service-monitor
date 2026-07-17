# ServiceLogServiceValidation

Provides static validation helpers for service log configuration within the systemd-service-monitor library. The type contains a set of parameter‑less members that report validation results, indicate overall validity, or enforce validity by throwing when the configuration is not acceptable.

## API

### Validate (IReadOnlyList<string>)
- **Purpose:** Returns a collection of validation error messages for the current service log configuration.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` containing error messages; an empty list indicates the configuration is valid.  
- **Exceptions:** None documented.

### Validate (IReadOnlyList<string>) – overload
- **Purpose:** Alternate overload of `Validate` that provides the same validation result.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` of validation messages; empty when valid.  
- **Exceptions:** None documented.

### Validate (IReadOnlyList<string>) – overload
- **Purpose:** Another overload of `Validate` with identical semantics.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` of validation messages; empty when valid.  
- **Exceptions:** None documented.

### Validate (IReadOnlyList<string>) – overload
- **Purpose:** Additional overload of `Validate`.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` of validation messages; empty when valid.  
- **Exceptions:** None documented.

### ValidateSearch (IReadOnlyList<string>)
- **Purpose:** Returns validation messages related to a search term used with the service log.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` containing search‑specific validation errors; empty if the search term is acceptable.  
- **Exceptions:** None documented.

### Validate (IReadOnlyList<string>) – overload
- **Purpose:** Yet another overload of `Validate`.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` of validation messages; empty when valid.  
- **Exceptions:** None documented.

### Validate (IReadOnlyList<string>) – overload
- **Purpose:** Further overload of `Validate`.  
- **Parameters:** None.  
- **Return Value:** An `IReadOnlyList<string>` of validation messages; empty when valid.  
- **Exceptions:** None documented.

### IsValid (bool)
- **Purpose:** Indicates whether the service log configuration passes validation.  
- **Parameters:** None.  
- **Return Value:** `true` if the configuration is valid; otherwise `false`.  
- **Exceptions:** None documented.

### IsValid (bool) – overload
- **Purpose:** Alternate overload providing the same validity check.  
- **Parameters:** None.  
- **Return Value:** `true` when valid, `false` otherwise.  
- **Exceptions:** None documented.

### IsValid (bool) – overload
- **Purpose:** Additional overload of `IsValid`.  
- **Parameters:** None.  
- **Return Value:** `true` for a valid configuration, `false` otherwise.  
- **Exceptions:** None documented.

### EnsureValid (void)
- **Purpose:** Confirms that the service log configuration is valid; throws an exception if it is not.  
- **Parameters:** None.  
- **Return Value:** None.  
- **Exceptions:** Throws an `InvalidOperationException` (or a derived type) when validation fails, containing the validation error messages.

### EnsureValid (void) – overload
- **Purpose:** Alternate overload that enforces validity in the same manner.  
- **Parameters:** None.  
- **Return Value:** None.  
- **Exceptions:** Throws when the configuration is invalid.

### EnsureValid (void) – overload
- **Purpose:** Further overload that validates and throws on failure.  
- **Parameters:** None.  
- **Return Value:** None.  
- **Exceptions:** Throws an exception describing validation problems.

## Usage

```csharp
// Example 1: Retrieve validation messages and act on them.
IReadOnlyList<string> errors = ServiceLogServiceValidation.Validate();
if (errors.Count > 0)
{
    foreach (var err in errors)
    {
        Console.WriteLine($"Validation error: {err}");
    }
}
else
{
    Console.WriteLine("Service log configuration is valid.");
}
```

```csharp
// Example 2: Ensure the configuration is valid, handling the exception if not.
try
{
    ServiceLogServiceValidation.EnsureValid();
    // Proceed with operations that require a valid configuration.
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Configuration invalid: {ex.Message}");
    // Optionally fallback to a safe default or abort initialization.
}
```

## Notes

- All members are **static** and contain no mutable state; therefore they are **thread‑safe** and can be invoked concurrently from multiple threads without additional synchronization.
- The various overloads of `Validate`, `IsValid`, and `EnsureValid` share the same signature in the published metadata; they exist to support different calling contexts (e.g., interface implementations) but behave identically.
- `ValidateSearch` is intended for validating search‑specific input; its error list is independent of the general service log validation performed by the `Validate` overloads.
- Calling `EnsureValid` when the configuration is invalid will result in an exception; the exception’s message typically aggregates the strings returned by `Validate`.
- If the configuration is valid, `Validate` and `ValidateSearch` return empty lists, `IsValid` returns `true`, and `EnsureValid` completes without throwing.
