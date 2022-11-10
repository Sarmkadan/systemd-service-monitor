# IResourceMonitorService

`IResourceMonitorService` defines a contract for monitoring system resources consumed by a systemd unit. Implementations track CPU, memory, threads, file descriptors, network, and disk I/O, and raise alerts when configured thresholds are exceeded.

## API

### `UnitName`
- **Purpose**: Identifies the systemd unit being monitored (e.g., `"nginx.service"`).
- **Type**: `string`
- **Return value**: The unit name as configured during service initialization.

### `CpuUsagePercent`
- **Purpose**: Current CPU usage percentage of the monitored unit.
- **Type**: `decimal`
- **Return value**: A value between `0.0` and `100.0`, representing the percentage of total CPU capacity used by the unit.

### `MemoryUsageMb`
- **Purpose**: Current memory usage in megabytes.
- **Type**: `long`
- **Return value**: The amount of memory consumed by the unit, in MB.

### `ThreadCount`
- **Purpose**: Number of active threads associated with the unit.
- **Type**: `int`
- **Return value**: The current thread count; may be zero if no threads are active.

### `FileDescriptorCount`
- **Purpose**: Number of open file descriptors held by the unit.
- **Type**: `int`
- **Return value**: The current count of open file descriptors; may exceed system limits if not managed.

### `NetworkBytesIn`
- **Purpose**: Total bytes received by the unit since monitoring began.
- **Type**: `long`
- **Return value**: Cumulative count of incoming network traffic in bytes.

### `NetworkBytesOut`
- **Purpose**: Total bytes transmitted by the unit since monitoring began.
- **Type**: `long`
- **Return value**: Cumulative count of outgoing network traffic in bytes.

### `DiskBytesRead`
- **Purpose**: Total bytes read from disk by the unit since monitoring began.
- **Type**: `long`
- **Return value**: Cumulative count of disk read operations in bytes.

### `DiskBytesWritten`
- **Purpose**: Total bytes written to disk by the unit since monitoring began.
- **Type**: `long`
- **Return value**: Cumulative count of disk write operations in bytes.

### `MeasuredAt`
- **Purpose**: Timestamp of when the current resource measurements were captured.
- **Type**: `DateTime`
- **Return value**: The exact time of measurement; typically in UTC.

### `Id`
- **Purpose**: Unique identifier for the monitoring session or alert instance.
- **Type**: `Guid`
- **Return value**: A new `Guid` generated per measurement or alert.

### `AlertType`
- **Purpose**: Classification of the alert condition (e.g., CPU overload, memory pressure).
- **Type**: `ResourceAlertType`
- **Return value**: One of the defined alert types in the `ResourceAlertType` enum.

### `Message`
- **Purpose**: Human-readable description of the alert condition.
- **Type**: `string`
- **Return value**: A localized or formatted message explaining the alert (e.g., `"CPU usage exceeded 90%"`).

### `CurrentValue`
- **Purpose**: The measured value that triggered the alert.
- **Type**: `decimal`
- **Return value**: The actual value (e.g., `92.5`) that exceeded the threshold.

### `Threshold`
- **Purpose**: The configured limit that triggered the alert.
- **Type**: `decimal`
- **Return value**: The threshold value (e.g., `90.0`) against which `CurrentValue` was compared.

### `AlertTime`
- **Purpose**: Timestamp when the alert was raised.
- **Type**: `DateTime`
- **Return value**: The exact time the alert condition was detected.

## Usage
