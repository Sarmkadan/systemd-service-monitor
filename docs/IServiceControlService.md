# IServiceControlService
The `IServiceControlService` type is designed to provide information about the outcome of a service control operation, such as starting, stopping, or restarting a service. It encapsulates details about the operation, including whether it was successful, the exit code, and any messages generated during the operation.

## API
* `IReadOnlyList<OperationResult> Results`: Gets a list of results from the service control operation. This list may contain multiple results if the operation involved multiple steps or affected multiple services.
* `string UnitName`: Gets the name of the service unit that was the target of the operation.
* `string Operation`: Gets the type of operation that was performed, such as "start", "stop", or "restart".
* `bool Success`: Gets a boolean indicating whether the operation was successful.
* `string Message`: Gets a message that describes the outcome of the operation. This may be an error message if the operation failed.
* `int ExitCode`: Gets the exit code of the operation. A non-zero exit code typically indicates that an error occurred.
* `DateTime OperationTime`: Gets the time at which the operation was performed.
* `long DurationMs`: Gets the duration of the operation in milliseconds.

## Usage
The following example demonstrates how to use the `IServiceControlService` to start a service and check the outcome of the operation:
```csharp
var serviceControlService = new ServiceControlService();
var operationResult = serviceControlService.StartService("myService");
if (operationResult.Success)
{
    Console.WriteLine($"Service {operationResult.UnitName} started successfully.");
}
else
{
    Console.WriteLine($"Failed to start service {operationResult.UnitName}: {operationResult.Message}");
}
```
This example shows how to use the `IServiceControlService` to restart a service and log the outcome:
```csharp
var serviceControlService = new ServiceControlService();
var operationResult = serviceControlService.RestartService("myService");
Console.WriteLine($"Service {operationResult.UnitName} restart operation completed with exit code {operationResult.ExitCode}.");
```

## Notes
The `IServiceControlService` type is designed to be thread-safe, allowing it to be safely accessed and used from multiple threads. However, the `IReadOnlyList<OperationResult>` returned by the `Results` property should not be modified, as it is a read-only collection. Additionally, the `Success` property should be checked before attempting to access other properties, as some properties (such as `Message` and `ExitCode`) may not be meaningful if the operation was not successful. Edge cases, such as a service being in an unknown state or an operation timing out, may result in an `OperationResult` with a non-zero exit code and an error message.
