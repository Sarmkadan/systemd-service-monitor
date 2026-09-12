# RequestLoggingMiddleware

`RequestLoggingMiddleware` records one information-level log when an HTTP request arrives and another after downstream processing completes. It also establishes request and correlation identifiers, makes them available through `HttpContext.Items`, and measures the time spent in the remainder of the request pipeline.

## Registration

Add the middleware to an ASP.NET Core pipeline with the `UseRequestLogging` extension method:

```csharp
app.UseRequestLogging();
```

The application's `UseApplicationMiddleware` extension registers request logging after error handling and before rate limiting. With that ordering, `ErrorHandlingMiddleware` can handle exceptions propagated by request logging or later components, while rate-limited requests are included in request logs.

`RequestLoggingMiddleware` receives its next `RequestDelegate` and `ILogger<RequestLoggingMiddleware>` through dependency injection. Its public `InvokeAsync(HttpContext context)` method is called once per request.

## Request and correlation identifiers

At the start of a request, the middleware reads these headers:

| Header | `HttpContext.Items` key | Purpose |
| --- | --- | --- |
| `X-Request-Id` | `RequestId` | Identifies one HTTP request. |
| `X-Correlation-Id` | `CorrelationId` | Associates related operations or requests. |

If an incoming header exists, its complete value is converted to a string and used unchanged. The middleware does not validate its format, reject an empty value, or replace multiple values with a new identifier.

If a header is absent, the middleware creates a GUID with `Guid.NewGuid().ToString()` and adds that value to the corresponding response header. A client-supplied identifier is not copied to the response header by this implementation.

Both resolved values are stored in `HttpContext.Items` before the next delegate runs. Downstream code can retrieve them as follows:

```csharp
var requestId = context.Items["RequestId"] as string;
var correlationId = context.Items["CorrelationId"] as string;
```

`LogContextEnricher`, when configured with access to the current `HttpContext`, also reads these item keys and can attach the values to other log events produced during the request.

## Logging

The incoming request is logged at information level before downstream middleware runs. The message template is:

```text
Incoming {Method} {Path}{Query} | RequestId: {RequestId} | CorrelationId: {CorrelationId}
```

It captures the HTTP method, request path, full query string, request identifier, and correlation identifier. Query values are logged as received, so callers should avoid placing secrets in URLs and logging configuration should account for potentially sensitive query data.

After the next delegate completes successfully, the middleware stops its stopwatch and writes another information-level event:

```text
Outgoing {StatusCode} for {Method} {Path} | Duration: {DurationMs}ms | RequestId: {RequestId}
```

The outgoing event contains the final status code at that point in the pipeline and elapsed whole milliseconds. It includes the request identifier but not the correlation identifier or query string.

If downstream processing throws, the outgoing event is not written and the exception is not caught by this middleware. It propagates after the original response stream has been restored. With the standard application ordering, the earlier `ErrorHandlingMiddleware` can then log and translate the exception.

## Response buffering

Before invoking the next delegate, the middleware replaces `HttpResponse.Body` with a new in-memory stream. On successful completion it attempts to copy that stream to the original response body, and a `finally` block restores the original stream reference whether processing succeeds or throws.

The current implementation does not reset the memory stream's position before `CopyToAsync`. After a downstream component writes a response body, the position is normally at the end, so the copy begins there and may write no body bytes to the original stream. This is an implementation characteristic callers should be aware of; the middleware does not otherwise inspect, log, or modify response content.

Because buffering uses an unbounded `MemoryStream`, the entire downstream response can be held in memory for the duration of the request. This can increase memory usage for large or streaming responses, and streaming data is not forwarded incrementally.

## Request flow summary

For a normally completed request, `InvokeAsync` performs these operations in order:

1. Starts a stopwatch.
2. Resolves or generates the request and correlation identifiers.
3. Stores both identifiers in `HttpContext.Items`.
4. Logs the incoming request.
5. Replaces the response body with an in-memory buffer and invokes the next delegate.
6. Stops the stopwatch and logs the outgoing status and duration.
7. Attempts to copy the buffered response to the original response stream.
8. Restores the original response stream.

The middleware does not change response status codes, response content types, request bodies, or downstream routing behavior.

See [ErrorHandlingMiddleware](ErrorHandlingMiddleware.md) for the outer exception boundary and [LogContextEnricher](LogContextEnricher.md) for contextual log enrichment.
