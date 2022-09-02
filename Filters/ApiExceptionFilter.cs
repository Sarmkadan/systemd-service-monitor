// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc.Filters;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Filters;

/// <summary>
/// Global exception filter that catches exceptions thrown by actions and returns consistent error responses.
/// Provides better error handling than middleware for action-level exceptions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception in {ActionName}",
            context.ActionDescriptor.DisplayName);

        var response = new ApiResponse<object>
        {
            Success = false,
            TraceId = context.HttpContext.TraceIdentifier,
            Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()
        };

        // Map exception types to status codes and messages
        context.HttpContext.Response.StatusCode = context.Exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            OperationCanceledException => StatusCodes.Status408RequestTimeout,
            TimeoutException => StatusCodes.Status408RequestTimeout,
            _ => StatusCodes.Status500InternalServerError
        };

        response.Message = context.Exception switch
        {
            ArgumentNullException => "Required argument is missing",
            ArgumentException => "Invalid argument provided",
            UnauthorizedAccessException => "Access denied - insufficient permissions",
            OperationCanceledException => "Request was cancelled",
            TimeoutException => "Request timeout - operation took too long",
            InvalidOperationException => "Invalid operation",
            _ => "An error occurred processing your request"
        };

        // Include error details in development only
        if (context.HttpContext.RequestServices
            .GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            response.ErrorDetails = context.Exception.ToString();
        }

        context.Result = new Microsoft.AspNetCore.Mvc.JsonResult(response)
        {
            StatusCode = context.HttpContext.Response.StatusCode
        };

        context.ExceptionHandled = true;

        await Task.CompletedTask;
    }
}
