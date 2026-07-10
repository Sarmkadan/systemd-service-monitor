# LogsController
The `LogsController` class is designed to manage and retrieve log data related to services in the `systemd-service-monitor` project. It provides various methods to fetch logs based on different criteria, such as pagination, recent logs, errors, and priority. This controller is essential for monitoring and debugging services, allowing developers to access and analyze log data efficiently.

## API
The `LogsController` class exposes the following public members:
* `GetServiceLogs`: Retrieves paginated service logs. Returns an `ActionResult` containing a `PaginatedResponse` of `ServiceLog` objects.
* `GetRecentLogs`: Fetches recent logs. Returns an `ActionResult` containing an `ApiResponse` with a list of `ServiceLog` objects.
* `GetServiceErrors`: Retrieves paginated service error logs. Returns an `ActionResult` containing a `PaginatedResponse` of `ServiceLog` objects.
* `GetLogsByPriority`: Retrieves logs based on priority. Returns an `ActionResult` containing an `ApiResponse` with a list of `ServiceLog` objects.
* `ExportServiceLogs`: Exports service logs. Returns an `ActionResult`.

## Usage
Here are two examples of using the `LogsController` class:
```csharp
// Example 1: Retrieving paginated service logs
var logsController = new LogsController();
var result = await logsController.GetServiceLogs();
if (result.Value != null)
{
    foreach (var log in result.Value.Items)
    {
        Console.WriteLine(log);
    }
}

// Example 2: Fetching recent logs
var recentLogsResult = await logsController.GetRecentLogs();
if (recentLogsResult.Value != null)
{
    foreach (var log in recentLogsResult.Value)
    {
        Console.WriteLine(log);
    }
}
```

## Notes
When using the `LogsController` class, consider the following:
* The `GetServiceLogs`, `GetServiceErrors`, and `GetLogsByPriority` methods may throw exceptions if the underlying data storage or retrieval mechanisms fail.
* The `ExportServiceLogs` method may throw exceptions if the export process fails or if the logs cannot be retrieved.
* The `LogsController` class is designed to be thread-safe, allowing multiple concurrent requests to be handled efficiently. However, the underlying data storage and retrieval mechanisms may have their own thread-safety limitations.
* Edge cases, such as empty log datasets or invalid priority values, should be handled accordingly by the calling code to ensure robustness and reliability.
