# SystemResourceValidation

Provides validation helpers for `SystemResource` instances.

## Overview

The `SystemResourceValidation` static class contains extension methods for validating `SystemResource` objects.
It checks various properties for correctness, consistency, and valid ranges.

## Methods

### Validate(SystemResource? value)

Validates a `SystemResource` instance and returns a list of human-readable validation problems.

#### Parameters
- `value`: The system resource to validate.

#### Returns
A read-only list of validation error messages. Empty if valid.

#### Exceptions
- `ArgumentNullException`: Thrown if `value` is null.

#### Validation Rules
The method checks the following:
- **Id**: Must not be empty (Guid.Empty).
- **Memory Metrics**:
  - All memory values (TotalMemoryMb, AvailableMemoryMb, UsedMemoryMb, CachedMemoryMb) must be non-negative.
  - AvailableMemoryMb cannot exceed TotalMemoryMb.
  - UsedMemoryMb cannot exceed TotalMemoryMb.
  - CachedMemoryMb cannot exceed TotalMemoryMb.
  - MemoryUsagePercent must be between 0 and 100 inclusive.
  - If MemoryUsagePercent is 0 and TotalMemoryMb > 0, it validates that UsedMemoryMb / TotalMemoryMb * 100 does not exceed 100%.
- **CPU Metrics**:
  - CpuCoreCount must be greater than zero.
  - CpuLoad1Min, CpuLoad5Min, CpuLoad15Min must be non-negative.
  - CpuUsagePercent must be between 0 and 100 inclusive.
  - LoadAveragePercent must be between 0 and 100 inclusive.
- **Disk Metrics**:
  - All disk values (TotalDiskGb, UsedDiskGb, AvailableDiskGb, DiskIopsPerSecond) must be non-negative.
  - AvailableDiskGb cannot exceed TotalDiskGb.
  - UsedDiskGb cannot exceed TotalDiskGb.
  - DiskUsagePercent must be between 0 and 100 inclusive.
  - If DiskUsagePercent is 0 and TotalDiskGb > 0, it validates that UsedDiskGb / TotalDiskGb * 100 does not exceed 100%.
- **Network Metrics**:
  - NetworkBytesIn and NetworkBytesOut must be non-negative.
- **Process Count**:
  - RunningProcesses must be non-negative.
- **Uptime**:
  - SystemUptimeSeconds must be non-negative.
- **RecordedAt**:
  - Must not be the default DateTime.
  - Must not be more than 5 minutes in the future.

### IsValid(SystemResource? value)

Determines whether a `SystemResource` instance is valid.

#### Parameters
- `value`: The system resource to check.

#### Returns
True if valid; otherwise, false.

#### Exceptions
- `ArgumentNullException`: Thrown if `value` is null.

### EnsureValid(SystemResource? value)

Ensures that a `SystemResource` instance is valid, throwing an `ArgumentException` if not.

#### Parameters
- `value`: The system resource to validate.

#### Exceptions
- `ArgumentNullException`: Thrown if `value` is null.
- `ArgumentException`: Thrown if `value` is invalid with a detailed message containing all validation problems.

## Usage Example

```csharp
var resource = new SystemResource 
{
    Id = Guid.NewGuid(),
    TotalMemoryMb = 8192,
    AvailableMemoryMb = 4096,
    UsedMemoryMb = 4096,
    CachedMemoryMb = 1024,
    MemoryUsagePercent = 50,
    // ... other properties
};

var errors = resource.Validate();
if (errors.Count > 0)
{
    // Handle validation errors
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}

if (!resource.IsValid())
{
    // Resource is invalid
}

resource.EnsureValid(); // Throws ArgumentException if invalid
```