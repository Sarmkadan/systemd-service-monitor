# ErrorHandlingMiddleware

`ErrorHandlingMiddleware` is the outer error boundary for the ASP.NET Core request pipeline. It catches exceptions that escape later middleware or endpoints, logs the complete exception, and translates it into a consistent JSON `ApiResponse<object>` without exposing internal details in non-development environments.

## Registration and request flow

Register the middleware with the `UseErrorHandling` extension method:

```csharp
app.UseErrorHandling();
```

The application's `UseApplicationMiddleware` extension already calls `UseErrorHandling` before request logging and rate limiting. Keeping it early in the pipeline allows it to catch exceptions propagated by components registered after it.

For every request, `InvokeAsync` logs a debug message before calling the next request delegate. If the delegate completes normally, the response is left unchanged. If it throws, the middleware:

1. Logs the exception at error level with the request path.
2. Selects an HTTP status code and client-safe message from the exception type.
3. Creates a failed `ApiResponse<object>` using the current ASP.NET Core trace identifier.
4. Includes exception details only when the host environment is Development.
5. Writes the response as camel-case JSON with an `application/json` content type.

The middleware handles the exception and does not rethrow it.

## Exception-to-response mapping

Mappings are evaluated in the following order. The ordering matters because `ArgumentNullException` derives from `ArgumentException`.

| Exception type | HTTP status | Response message |
| --- | ---: | --- |
| `ArgumentNullException` | `400 Bad Request` | `Required parameter is missing` |
| `ArgumentException` | `400 Bad Request` | `Invalid argument provided` |
| `UnauthorizedAccessException` | `403 Forbidden` | `Access denied` |
| `OperationCanceledException` | `408 Request Timeout` | `Request was cancelled` |
| `TimeoutException` | `408 Request Timeout` | `Request timeout` |
| `InvalidOperationException` | `400 Bad Request` | `Invalid operation` |
| Any other `Exception` | `500 Internal Server Error` | `An internal error occurred` |

Project-specific exceptions such as `ServiceMonitorException` do not have a dedicated mapping in this middleware. Unless they derive from one of the listed exception types, they use the `500 Internal Server Error` fallback.

## Response body

Every translated exception produces an `ApiResponse<object>` with:

- `success` set to `false`.
- `message` set from the mapping table.
- `traceId` set to `HttpContext.TraceIdentifier`, allowing the client-visible error to be correlated with request logs.
- `timestamp` set to the current UTC time as Unix time in seconds.
- `data` left as `null`.
- `errorDetails` set to `exception.ToString()` only in Development; otherwise it remains `null`.

A production response has this general shape:

```json
{
  "data": null,
  "success": false,
  "message": "Invalid argument provided",
  "errorDetails": null,
  "timestamp": 1789142400,
  "traceId": "0HNExampleTraceId"
}
```

The middleware supplies serializer options with a camel-case naming policy. With those options, the `null` `data` and production `errorDetails` fields are included in the JSON response.

In Development, `errorDetails` contains the exception text and stack trace. Clients should treat this field as diagnostic and optional; production callers should rely on `message`, the HTTP status, and `traceId`.

## Logging

The middleware writes debug logs when entering and exiting normally. An exception is logged at error level with the full exception and request path before the client response is generated. Because the exception path is handled inside the `catch` block, the normal exit debug message is also written after the error response completes.

## Interaction with controller exception handling

The application also registers `ApiExceptionFilter` globally for MVC controllers. Exceptions handled by that filter are converted into a controller result and do not propagate to `ErrorHandlingMiddleware`. The filter has its own client messages, so a controller action error may not use the exact wording in the table above.

`ErrorHandlingMiddleware` remains the pipeline-wide fallback for exceptions that escape MVC handling or originate in other downstream middleware and endpoints.

## Operational notes

- `UnauthorizedAccessException` maps to `403 Forbidden`, not `401 Unauthorized`; the middleware does not create an authentication challenge.
- Both cancellation and timeout exceptions map to `408 Request Timeout`. The middleware does not use the non-standard `499` status for client cancellation.
- The response is written after an exception is caught. If downstream code has already started the HTTP response, changing its status or writing the JSON envelope may fail; the middleware does not contain a separate `Response.HasStarted` path.
- Only exceptions are translated. A downstream component that returns an error status without throwing retains its original response.

See [ApiResponse](ApiResponse.md) for the shared response envelope.
