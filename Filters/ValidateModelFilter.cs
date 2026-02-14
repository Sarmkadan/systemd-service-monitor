#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Filters;

/// <summary>
/// Action filter that validates model state and returns a consistent error response for validation failures.
/// Prevents execution of invalid requests and provides clear validation error messages.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class ValidateModelFilter : IAsyncActionFilter
{
    private readonly ILogger<ValidateModelFilter> _logger;

    public ValidateModelFilter(ILogger<ValidateModelFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed for {ActionName}",
                context.ActionDescriptor.DisplayName);

            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>());

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Model validation failed",
                TraceId = context.HttpContext.TraceIdentifier,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                ErrorDetails = System.Text.Json.JsonSerializer.Serialize(errors)
            };

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}

/// <summary>
/// Validation filter factory for auto-registering the validation filter on all controllers.
/// </summary>
public class AutoValidateModelFilterFactory : IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ValidateModelFilter>>();
        return new ValidateModelFilter(logger);
    }

    public bool IsReusable => true;
}
