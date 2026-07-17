# OutputFormatterValidation

`OutputFormatterValidation` is a static helper class that validates data structures used by the output formatting subsystem of **systemd-service-monitor**.  
It provides a set of overloads that return a list of validation error messages, a convenience method that reports whether the data is valid, and a method that throws an exception when validation fails.  All methods are pure and thread‑safe.

## API

| Method | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `public static IReadOnlyList<string> Validate(IEnumerable<ServiceInfo>? services)` | Validates a collection of `ServiceInfo` objects. | `services` – the collection to validate; may be `null` or empty. | A read‑only list of error messages; empty if the collection is valid. | None |
| `public static IReadOnlyList<string> Validate(ServiceInfo? service)` | Validates a single `ServiceInfo` instance. | `service` – the instance to validate; may be `null`. | A read‑only list of error messages; empty if the instance is valid. | None |
| `public static IReadOnlyList<string> Validate(SystemResource? metrics)` | Validates a `SystemResource` instance. | `metrics` – the instance to validate; may be `null`. | A read‑only list of error messages; empty if the instance is valid. | None |
| `public static IReadOnlyList<string> Validate<T>(T value)` | Generic validation that dispatches to the appropriate overload based on the runtime type of `value`. | `value` – the object to validate; may be `null`. | A read‑only list of error messages; empty if the object is valid. | None |
| `public static IReadOnlyList<string> ValidateTable(IEnumerable<ServiceInfo>? services)` | Validates a collection of `ServiceInfo` objects for table rendering. | `services` – the collection to validate; may be `null` or empty. | A read‑only list of error messages; empty if the collection is valid for table output. | None |
| `public static IReadOnlyList<string> Validate()` | Validates the default output configuration (no parameters). | None | A read‑only list of error messages; empty if the default configuration is valid. | None |
| `public static bool IsValid(IEnumerable<ServiceInfo>? services)` | Returns `true` if the collection of `ServiceInfo` objects is valid. | `services` – the collection to validate. | `true` if no validation errors; otherwise `false`. | None |
| `public static void EnsureValid(IEnumerable<ServiceInfo>? services)` | Validates the collection and throws `ValidationException` if any errors are found. | `services` – the collection to validate. | None | `ValidationException` containing the list of errors. |

## Usage

