# ApiExceptionFilter

Global exception filter that catches exceptions thrown by actions and returns consistent error responses.

## Purpose

Provides centralized exception handling for MVC actions, converting unhandled exceptions into standardized JSON error responses with appropriate HTTP status codes.

## Behavior

The filter implements `IAsyncExceptionFilter` and processes exceptions as follows:

1. **Logging**: Logs the exception with action context using `ILogger<ApiExceptionFilter>`
2. **Response Creation**: Creates an `ApiResponse<object>` with:
   - `Success = false`
   - `TraceId` from `HttpContext.TraceIdentifier`
   - `Timestamp` as Unix seconds
3. **Exception Mapping**: Maps exception types to HTTP status codes and user-friendly messages:
   - `ArgumentNullException` → 400 Bad Request ("Required argument is missing")
   - `ArgumentException` → 400 Bad Request ("Invalid argument provided")
   - `UnauthorizedAccessException` → 403 Forbidden ("Access denied - insufficient permissions")
   - `OperationCanceledException` → 408 Request Timeout ("Request was cancelled")
   - `TimeoutException` → 408 Request Timeout ("Request timeout - operation took too long")
   - `InvalidOperationException` → 500 Internal Server Error ("Invalid operation")
   - All other exceptions → 500 Internal Server Error ("An error occurred processing your request")
4. **Development Details**: In development environments, includes full exception details in `response.ErrorDetails`
5. **Result**: Sets `context.Result` to a `JsonResult` containing the response with the appropriate status code
6. **Completion**: Marks exception as handled (`context.ExceptionHandled = true`)

## Usage

Register as a global filter in `Startup.cs` or `Program.cs`:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});
```

## Notes

- Designed for action-level exceptions; middleware may still handle pipeline-level exceptions
- Does not swallow exceptions - marks them as handled after generating response
- Follows the project's standard `ApiResponse<T>` format for consistency
- Error details are only included in development to avoid leaking sensitive information