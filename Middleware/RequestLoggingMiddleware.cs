#nullable enable

using System.Diagnostics;

namespace SystemdServiceMonitor.Middleware;

/// <summary>
/// Middleware for logging incoming requests and outgoing responses.
/// Tracks request duration, status codes, and provides detailed request/response logging.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const string RequestIdHeaderName = "X-Request-Id";
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestId = GetOrCreateRequestId(context);
        var correlationId = GetOrCreateCorrelationId(context);

        // Store for later retrieval in logging context
        context.Items["RequestId"] = requestId;
        context.Items["CorrelationId"] = correlationId;

        var request = context.Request;
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log incoming request
            logger.LogInformation(
                "Incoming {Method} {Path}{Query} | RequestId: {RequestId} | CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                request.QueryString,
                requestId,
                correlationId);

            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;

                await next(context);

                sw.Stop();

                // Log response
                logger.LogInformation(
                    "Outgoing {StatusCode} for {Method} {Path} | Duration: {DurationMs}ms | RequestId: {RequestId}",
                    context.Response.StatusCode,
                    request.Method,
                    request.Path,
                    sw.ElapsedMilliseconds,
                    requestId);

                // Copy response body to original stream
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static string GetOrCreateRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(RequestIdHeaderName, out var requestId))
        {
            return requestId.ToString();
        }

        var newRequestId = Guid.NewGuid().ToString();
        context.Response.Headers[RequestIdHeaderName] = newRequestId;
        return newRequestId;
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Response.Headers[CorrelationIdHeaderName] = newCorrelationId;
        return newCorrelationId;
    }
}

/// <summary>
/// Extension method to register request logging middleware in the request pipeline.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
