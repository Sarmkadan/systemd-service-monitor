# ValidateModelFilter

Action filter that validates model state and returns a consistent error response for validation failures. Prevents execution of invalid requests and provides clear validation error messages.

## Overview

The `ValidateModelFilter` is an asynchronous action filter that implements `IAsyncActionFilter` to validate the model state before executing controller actions. If validation fails, it returns a standardized error response without executing the action.

## Implementation Details

### Class: `ValidateModelFilter`

```csharp
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
```

#### Key Features:
- **Dependency Injection**: Receives `ILogger<ValidateModelFilter>` via constructor
- **Early Validation**: Checks `context.ModelState.IsValid` before proceeding
- **Error Collection**: Extracts validation errors into a dictionary format
- **Standardized Response**: Returns `ApiResponse<object>` with validation failure details
- **Logging**: Logs validation failures with action name for debugging
- **Pipeline Control**: Returns early to prevent action execution on validation failure

### Class: `AutoValidateModelFilterFactory`

```csharp
public class AutoValidateModelFilterFactory : IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ValidateModelFilter>>();
        return new ValidateModelFilter(logger);
    }

    public bool IsReusable => true;
}
```

#### Purpose:
- Enables automatic registration of the validation filter on all controllers
- Implements `IFilterFactory` for dependency injection support
- Returns reusable filter instances

## Integration into MVC Pipeline

### Registration

The filter is typically registered globally in `Program.cs` or `Startup.cs`:

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AutoValidateModelFilterFactory());
});
```

### Execution Flow

1. **Request Received**: ASP.NET Core receives HTTP request
2. **Model Binding**: Framework binds request data to action parameters
3. **Filter Execution**: `ValidateModelFilter.OnActionExecutionAsync` executes
4. **Validation Check**: 
   - If model state is valid: Calls `await next()` to proceed to action
   - If model state is invalid: Returns `BadRequestObjectResult` with formatted errors
5. **Response**: Client receives standardized error response

### Error Response Format

When validation fails, the filter returns HTTP 400 with JSON body:

```json
{
  "success": false,
  "message": "Model validation failed",
  "traceId": "0HMXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "timestamp": 1726300800,
  "errorDetails": "{\"FieldName\":[\"Error message 1\",\"Error message 2\"],\"AnotherField\":[\"Error message\"]}"
}
```

## Usage Benefits

### Consistency
- All validation errors follow the same response format
- Eliminates need for manual validation checks in each action
- Standardizes error handling across the API

### Performance
- Prevents unnecessary action execution when validation fails
- Early termination saves processing cycles
- Reduces load on downstream services

### Developer Experience
- Clear separation of concerns (validation vs business logic)
- Automatic validation reduces boilerplate code
- Centralized logging of validation issues

### API Contract
- Clients can rely on consistent error format
- TraceId enables correlation with logs
- Timestamp provides request timing information

## Related Components

- **ApiResponse<T>**: Standardized response wrapper used for both success and error responses
- **ILogger<T>**: Provides structured logging capabilities
- **ModelStateDictionary**: ASP.NET Core's built-in model validation state tracking

## Example Controller Usage

Once registered globally, no additional attributes are needed:

```csharp
[ApiController]
[Route("[controller]")]
public class AlertsController : ControllerBase
{
    // No [ValidateModel] attribute needed - handled automatically
    [HttpPost]
    public IActionResult CreateAlert([FromBody] CreateAlertDto dto)
    {
        // Action only executes if model state is valid
        // Validation errors are handled by ValidateModelFilter
        return Ok(_alertService.Create(dto));
    }
}
```

## Testing Considerations

### Unit Testing
- Test filter in isolation with mock `ActionExecutingContext`
- Verify valid models proceed to `next()` delegate
- Verify invalid models return `BadRequestObjectResult`
- Confirm error response contains expected validation details

### Integration Testing
- Test actual API endpoints with invalid payloads
- Validate HTTP 400 response status
- Confirm response body matches expected error format
- Ensure valid requests proceed normally

## Dependencies

- `Microsoft.AspNetCore.Mvc`
- `Microsoft.AspNetCore.Mvc.Filters`
- `SystemdServiceMonitor.Responses` (for ApiResponse<T>)
- `Microsoft.Extensions.Logging.Abstractions` (for ILogger)

## Thread Safety

The filter is thread-safe as:
- It contains no mutable state after construction
- Dependencies (`ILogger`) are thread-safe
- Each request gets its own filter instance via DI
- The `AutoValidateModelFilterFactory.IsReusable = true` indicates safe reuse

## Error Handling

The filter handles:
- Null model state values gracefully
- Empty error collections
- JSON serialization of error details
- Proper HTTP status code setting (400 Bad Request)

## Performance Characteristics

- Minimal overhead for valid requests (just model state check)
- Efficient error extraction using LINQ
- Single JSON serialization for error details
- Early exit prevents action execution costs on invalid requests