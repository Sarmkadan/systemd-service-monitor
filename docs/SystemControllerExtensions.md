# SystemControllerExtensions

`SystemControllerExtensions` is a static class that provides helper methods for exposing system‑health and resource information through ASP.NET Core controller actions. The methods return `ActionResult<ApiResponse<object>>`, allowing callers to leverage standard MVC result handling while encapsulating the actual payload in a uniform `ApiResponse` wrapper. The accompanying `ResourceThresholds` record defines a simple, immutable configuration structure used by the health‑evaluation logic.

## API

### `public static ActionResult<ApiResponse<object>> GetSimpleHealthStatus()`
- **Purpose**: Returns a high‑level health indicator (e.g., “healthy” or “degraded”) for the monitored system.
- **Parameters**: None.
- **Return Value**: An `ActionResult` wrapping an `ApiResponse<object>` whose `Data` property contains the health status payload; the HTTP status code reflects the outcome (typically 200 for success).
- **Throws**: May propagate exceptions from underlying service queries (e.g., inability to access service manager) which are caught by the ASP.NET Core pipeline and transformed into a 500 response.

### `public static ActionResult<ApiResponse<object>> GetCompactSystemInfo()`
- **Purpose**: Provides a concise snapshot of system information such as OS version, hostname, and uptime.
- **Parameters**: None.
- **Return Value**: An `ActionResult` wrapping an `ApiResponse<object>` with the compact info object in `Data`; returns 200 on success.
- **Throws**: Similar to `GetSimpleHealthStatus`, any failure to retrieve system details results in an exception that surfaces as a 500 error.

### `public static ActionResult<ApiResponse<object>> GetResourceHealthSummary()`
- **Purpose**: Summarizes the health of key resources (CPU, memory, disk) based on configured thresholds.
- **Parameters**: None.
- **Return Value**: An `ActionResult` wrapping an `ApiResponse<object>` where `Data` holds a summary object; returns 200 when the summary can be generated.
- **Throws**: If resource metrics cannot be read or threshold evaluation fails, an exception may be thrown leading to a 500 response.

### `public static ActionResult<ApiResponse<object>> GetCriticalServiceCounts()`
- **Purpose**: Returns counts of services that are in critical states (e.g., failed, inactive) as defined by the monitoring policy.
- **Parameters**: None.
- **Return Value**: An `ActionResult` wrapping an `ApiResponse<object>` with the count information in `Data`; returns 200 on success.
- **Throws**: Errors accessing service state propagate as exceptions, yielding a 500 response.

### `public record ResourceThresholds`
- **Purpose**: Immutable record used to configure threshold values for resource health checks (e.g., CPU usage limits, memory pressure levels).
- **Parameters**: The record’s constructor parameters are compiler‑generated based on its declared fields; they define the threshold values.
- **Return Value**: Not applicable; the record is a data type.
- **Throws**: Instantiation throws only if arguments violate any explicit validation logic defined in the record (none inferred from the signature alone).

## Usage

```csharp
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Extensions; // adjust to actual namespace

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetSimpleHealth()
    {
        // Calls the extension method and returns the ActionResult directly.
        return SystemControllerExtensions.GetSimpleHealthStatus();
    }

    [HttpGet("info")]
    public IActionResult GetCompactInfo()
    {
        var result = SystemControllerExtensions.GetCompactSystemInfo();
        // Optionally inspect result.Result for custom handling.
        return result;
    }
}
```

```csharp
// Example of using the ResourceThresholds record to configure monitoring.
var thresholds = new ResourceThresholds(
    cpuWarningPercent: 75.0,
    cpuCriticalPercent: 90.0,
    memoryWarningMb: 2000,
    memoryCriticalMb: 1000
);

// The thresholds could be passed to a health‑evaluation service elsewhere in the application.
```

## Notes

- The extension methods are **stateless**; they rely only on injected services or static state that is assumed to be thread‑safe. Consequently, calling them concurrently from multiple requests does not introduce race conditions.
- If any of the underlying monitoring components are not initialized (e.g., the service manager connection has not been established), the methods will throw, resulting in a 500 Internal Server Error response from the ASP.NET Core framework.
- The `ResourceThresholds` record is immutable and value‑based; instances can be safely shared across threads without synchronization.
- Return types are deliberately generic (`ApiResponse<object>`) to allow the caller to deserialize the `Data` payload into a strongly‑typed model appropriate to the endpoint. Consumers should perform null checks on `Data` before use.
