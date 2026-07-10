# SystemResource
The `SystemResource` type represents a snapshot of system resource utilization, providing a comprehensive overview of the current state of system resources such as memory, CPU, disk, and network usage. This type is designed to be used in the context of system monitoring and alerting, allowing developers to easily access and utilize system resource data.

## API
The `SystemResource` type exposes the following public members:
* `Id`: A unique identifier for the system resource snapshot, represented as a `Guid`.
* `TotalMemoryMb`: The total amount of memory available in the system, in megabytes.
* `AvailableMemoryMb`: The amount of memory currently available for use, in megabytes.
* `UsedMemoryMb`: The amount of memory currently in use, in megabytes.
* `CachedMemoryMb`: The amount of memory currently being used for caching, in megabytes.
* `CpuCoreCount`: The number of CPU cores available in the system.
* `CpuLoad1Min`, `CpuLoad5Min`, `CpuLoad15Min`: The average CPU load over the last 1, 5, and 15 minutes, respectively.
* `CpuUsagePercent`: The current CPU usage as a percentage.
* `TotalDiskGb`: The total amount of disk space available in the system, in gigabytes.
* `UsedDiskGb`: The amount of disk space currently in use, in gigabytes.
* `AvailableDiskGb`: The amount of disk space currently available for use, in gigabytes.
* `DiskIopsPerSecond`: The current disk I/O operations per second.
* `NetworkBytesIn` and `NetworkBytesOut`: The total number of bytes received and sent over the network, respectively.
* `RunningProcesses`: The number of processes currently running in the system.
* `SystemUptimeSeconds`: The amount of time the system has been running, in seconds.
* `LoadAveragePercent`: The average system load as a percentage.
* `MemoryUsagePercent`: The current memory usage as a percentage.

## Usage
Here are two examples of using the `SystemResource` type:
```csharp
// Example 1: Basic system resource usage
SystemResource systemResource = GetSystemResource();
Console.WriteLine($"CPU Usage: {systemResource.CpuUsagePercent}%");
Console.WriteLine($"Memory Usage: {systemResource.MemoryUsagePercent}%");
Console.WriteLine($"Disk Space: {systemResource.UsedDiskGb} GB used out of {systemResource.TotalDiskGb} GB");

// Example 2: Advanced system resource monitoring
SystemResource systemResource2 = GetSystemResource();
if (systemResource2.CpuUsagePercent > 80)
{
    Console.WriteLine("High CPU usage detected!");
}
if (systemResource2.MemoryUsagePercent > 90)
{
    Console.WriteLine("High memory usage detected!");
}
```

## Notes
When using the `SystemResource` type, note that the values returned are snapshots of the system resource utilization at the time of retrieval. These values may change rapidly, and it is recommended to retrieve the latest values as needed. Additionally, the `SystemResource` type is designed to be thread-safe, allowing it to be safely accessed and used from multiple threads. However, it is still important to follow standard threading best practices when using this type in a multi-threaded environment. Edge cases such as extremely high or low system resource utilization may result in unexpected behavior, and it is recommended to implement appropriate error handling and logging mechanisms to handle such scenarios.
