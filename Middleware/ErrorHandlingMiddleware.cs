#nullable enable

using System.Net;
using System.Text.Json;
using SystemdServiceMonitor.Responses;
using Microsoft.Extensions.Logging;

namespace SystemdServiceMonitor.Middleware;

/// <summary>
/// Global error handling middleware that catches unhandled exceptions and returns consistent error responses.
/// Prevents internal server details from leaking to clients while maintaining detailed logging.
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogDebug("Entering ErrorHandlingMiddleware for request {Path}", context.Request.Path);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception in request pipeline. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }

        logger.LogDebug("Exiting ErrorHandlingMiddleware for request {Path}", context.Request.Path);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            TraceId = context.TraceIdentifier,
            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
        };

        // Determine status code and message based on exception type
        (int statusCode, string message) = exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Required parameter is missing"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
            OperationCanceledException => (StatusCodes.Status408RequestTimeout, "Request was cancelled"),
            TimeoutException => (StatusCodes.Status408RequestTimeout, "Request timeout"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred")
        };

        context.Response.StatusCode = statusCode;
        response.Message = message;

        // Only include error details in development
        if (context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            response.ErrorDetails = exception.ToString();
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }
}

/// <summary>
/// Extension method to register the error handling middleware in the request pipeline.
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
