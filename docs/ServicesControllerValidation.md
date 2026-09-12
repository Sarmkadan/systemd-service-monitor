# ServicesControllerValidation

`ServicesControllerValidation` provides extension methods for checking the configuration exposed by a `ServicesController`. Validation can return all detected problems, produce a boolean result, or throw when the controller is invalid.

The class is defined in `SystemdServiceMonitor.Controllers`.

## Validation rules

`Validate` applies the following rules:

| Property | Valid value | Validation message |
| --- | --- | --- |
| `MaxConcurrency` | An integer from `1` through `20`, inclusive | `MaxConcurrency must be between 1 and 20, but was {value}.` |
| `ServiceNames` | A non-null collection containing at least one entry | `ServiceNames collection cannot be null.` or `ServiceNames collection cannot be empty.` |
| Each `ServiceNames` entry | A non-null string containing at least one non-whitespace character | `ServiceNames collection contains {count} null or whitespace entries.` |

The checks for `MaxConcurrency` and `ServiceNames` are independent, so one call can report a problem for each property. The service-name checks are mutually exclusive: a null or empty collection does not also produce an invalid-entry message. When multiple problems are found, they are returned in the order shown above.

This validation does not check whether a service exists, whether a name has a particular systemd suffix or format, or whether the caller has permission to operate on the service.

## API

### `Validate(this ServicesController value)`

Returns a read-only list of human-readable validation problems. An empty list means that the controller passed every rule.

- Throws `ArgumentNullException` when `value` is null.
- Counts every null, empty, or whitespace-only item in `ServiceNames` and reports the total in one message.
- Uses invariant culture when inserting numeric values into messages.

### `IsValid(this ServicesController value)`

Returns `true` when `Validate` returns no problems; otherwise, returns `false`.

Because this method delegates to `Validate`, it throws `ArgumentNullException` when `value` is null.

### `EnsureValid(this ServicesController value)`

Returns normally when the controller is valid. If validation finds any problems, it throws an `ArgumentException` whose parameter name is `value`. The exception message joins all validation problems with a single space.

It throws `ArgumentNullException` when `value` is null.

## Usage

The methods can be called with extension-method syntax after importing the controller namespace:

```csharp
using SystemdServiceMonitor.Controllers;

IReadOnlyList<string> problems = controller.Validate();

if (problems.Count > 0)
{
    foreach (string problem in problems)
    {
        logger.LogWarning("Invalid services controller configuration: {Problem}", problem);
    }
}
```

Use `IsValid` when only a boolean result is needed:

```csharp
if (!controller.IsValid())
{
    return;
}
```

Use `EnsureValid` as a guard when invalid configuration should stop the current operation:

```csharp
controller.EnsureValid();
```

## Enumeration behavior

`ServiceNames` is typed as `IEnumerable<string>`. For a non-null collection, validation first checks whether it contains an item and then enumerates it again to collect invalid entries. Callers supplying a lazy or stateful enumerable should therefore ensure that it can be enumerated repeatedly and yields consistent results.
