# ServiceInfoValidation

`ServiceInfoValidation` provides extension methods for validating `ServiceInfo` instances. It can return every detected problem, provide a boolean validity check, or throw when invalid data must be rejected.

## API

### `IReadOnlyList<string> Validate(this ServiceInfo value)`

Validates `value` and returns a read-only list of human-readable problems. The list is empty when all rules pass. Validation does not stop after the first failure, so a single call can report multiple independent problems.

Throws `ArgumentNullException` when `value` is `null`.

### `bool IsValid(this ServiceInfo value)`

Returns `true` when `Validate` reports no problems; otherwise, returns `false`.

Throws `ArgumentNullException` when `value` is `null`.

### `void EnsureValid(this ServiceInfo value)`

Returns normally when `value` is valid. If validation fails, it throws an `ArgumentException` whose message contains every problem as a separate bulleted line.

Throws `ArgumentNullException` when `value` is `null`.

## Validation rules

| Property | Requirement |
| --- | --- |
| `Id` | Must not be `Guid.Empty`. |
| `LoadState` | Must not be `ServiceLoadState.Unknown`. |
| `CpuUsagePercent` | Must be between `0` and `100`, inclusive. |
| `MemoryUsageMb` | Must not be negative. |
| `UnitName` | Must not be null, empty, or whitespace and must not exceed 255 characters. |
| `Description` | Must not be null and must not exceed 1,024 characters. An empty string is allowed. |
| `UnitFilePath` | Must not be null, empty, or whitespace and must begin with `/` or `@`. |
| `State` | Must not be `ServiceState.Unknown`. |
| `SubState` | Must not be `ServiceSubState.Unknown`. |
| `MainProcessId` | Must not be negative. Zero is allowed for a service without a running main process. |
| `Result` | Must not be null and must not exceed 128 characters. An empty string is allowed. |
| `Dependencies` | Must not be null and must not contain a null, empty, or whitespace entry. An empty collection is allowed. |
| `Dependents` | Must not be null and must not contain a null, empty, or whitespace entry. An empty collection is allowed. |
| `LastStartTime` | When present, must not be in the future. |
| `LastStopTime` | When present, must not be in the future and must not precede `LastStartTime` when both values are present. |
| `UptimeSeconds` | Must be between zero and 31,536,000 seconds (365 days), inclusive. |
| `RestartCount` | Must not be negative. |
| `WorkingDirectory` | When non-null, must begin with `/` or `@`. An empty non-null string therefore fails validation. |
| `RunAsUser` | Must not be null and must not exceed 64 characters. An empty string is allowed. |
| `RunAsGroup` | Must not be null and must not exceed 64 characters. An empty string is allowed. |
| `CreatedAt` | Must not be in the future. |
| `UpdatedAt` | Must not be in the future and must not precede `CreatedAt`. |

`RestartPolicy`, `AutoStart`, and `Restart` have no validation rules in this class.

## Usage

Collect all problems when returning user-facing validation feedback:

```csharp
ServiceInfo service = GetServiceInfo();

IReadOnlyList<string> problems = service.Validate();
if (problems.Count > 0)
{
    foreach (string problem in problems)
    {
        Console.WriteLine(problem);
    }
}
```

Use `IsValid` when only the outcome matters:

```csharp
if (!service.IsValid())
{
    return BadRequest("The service information is invalid.");
}
```

Use `EnsureValid` as a guard before processing trusted model data:

```csharp
public void Store(ServiceInfo service)
{
    service.EnsureValid();
    repository.Save(service);
}
```

## Notes

- Future-time checks compare values with `DateTime.UtcNow` at validation time. Callers should provide timestamps with UTC-compatible semantics.
- `Validate` returns `List<string>.AsReadOnly()`: callers can enumerate the result but cannot modify it through the returned interface.
- `IsValid` delegates to `Validate`, and `EnsureValid` uses the same rules, so all three methods produce a consistent validity decision.
