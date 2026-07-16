## ApiResponse

The `ApiResponse` class provides a standardized way to return data and errors from API endpoints. It wraps the actual data being returned, along with additional metadata such as success status, human-readable messages, and error details.

### Usage Example

```csharp
var successResponse = new ApiResponse<string>
{
    Data = "Hello, World!",
    Success = true,
    Message = "Operation completed successfully.",
    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
    TraceId = Guid.NewGuid().ToString()
};

var errorResponse = new ApiResponse<string>
{
    Success = false,
    Message = "An error occurred.",
    ErrorDetails = "Invalid input data.",
    Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
    TraceId = Guid.NewGuid().ToString()
};

Console.WriteLine($"Success: {successResponse.Success}, Data: {successResponse.Data}");
Console.WriteLine($"Success: {errorResponse.Success}, Error: {errorResponse.ErrorDetails}");
```

## RateLimitingMiddleware

The `RateLimitingMiddleware` implements a token‑bucket algorithm to limit the number of requests per IP address. It consumes a token on each request and returns a 429 status code when the bucket is empty. The middleware can be added to the ASP.NET Core pipeline using the provided extension methods.

### Usage Example

```csharp
using SystemdServiceMonitor.Middleware;
using Microsoft.AspNetCore.Builder;

// In your ASP.NET Core application startup:
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configure rate limiting: 200 requests per minute, refill every 60 seconds
app.UseRateLimiting(new RateLimitOptions
{
    RequestsPerMinute = 200,
    RefillIntervalSeconds = 60
});

// Alternatively, use the default configuration (300 requests/minute)
// app.UseRateLimiting();

// Example of creating and inspecting a TokenBucket manually
var bucket = new TokenBucket(100, 60); // 100 tokens, refill every 60 seconds
bool consumed = bucket.TryConsumeToken(); // true if a token was available
int remaining = bucket.RemainingTokens;   // current token count
int capacity = bucket.RequestsPerMinute;  // bucket capacity
int refill = bucket.RefillIntervalSeconds; // refill interval

app.MapGet("/", () => "Hello, world!");
app.Run();
```

The middleware automatically tracks requests per IP and enforces the configured limits, ensuring that clients cannot exceed the specified request rate.

## IServiceControlService

The `IServiceControlService` interface provides a standardized way to control systemd services. It allows you to start, stop, restart, reload, enable, and disable services, as well as perform bulk restarts and get the status of recent operations.

### Usage Example

```csharp
var serviceControlService = new ServiceControlService(); // implementation of IServiceControlService
var result = await serviceControlService.StartServiceAsync("my-service", CancellationToken.None);
Console.WriteLine($"Service started: {result.Success}, Message: {result.Message}");
```

In this example, we create an instance of the `ServiceControlService` class, which implements the `IServiceControlService` interface. We then call the `StartServiceAsync` method to start the "my-service" service, and print the result to the console.

### Members

The `IServiceControlService` interface has the following members:

* `Results`: a list of `OperationResult` objects, representing the results of recent operations
* `UnitName`: the name of the service being controlled
* `Operation`: the operation being performed (e.g. "start", "stop", etc.)
* `Success`: a boolean indicating whether the operation was successful
* `Message`: a human-readable message describing the result of the operation
* `ExitCode`: the exit code of the service (if applicable)
* `OperationTime`: the timestamp of the operation
* `DurationMs`: the duration of the operation in milliseconds

Note that this is not an exhaustive list of all possible members, but rather a selection of the most relevant ones for demonstration purposes.