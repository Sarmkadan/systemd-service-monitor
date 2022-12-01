# ServiceStatusUpdateWorkerExtensions

Provides extension methods for configuring and inspecting `ServiceWorkerOptions` used by the `ServiceStatusUpdateWorker`. The class contains only static members and does not maintain any state.

## API

### ConfigureServiceStatusUpdateWorker
**Purpose**  
Registers the `ServiceStatusUpdateWorker` and its options with the DI container.

**Parameters**  
- `services`: The `IServiceCollection` to add the worker to.  
- `options`: A `ServiceWorkerOptions` instance containing configuration values.

**Return value**  
The same `IServiceCollection` instance, allowing fluent chaining.

**Exceptions**  
- `ArgumentNullException` if `services` or `options` is `null`.

### GetUpdateIntervalMs
**Purpose**  
Retrieves the update interval, in milliseconds, from the supplied options.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to read from.

**Return value**  
An `int` representing the interval in milliseconds.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### GetErrorBackoffMs
**Purpose**  
Retrieves the error back‑off interval, in milliseconds, from the supplied options.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to read from.

**Return value**  
An `int` representing the back‑off interval in milliseconds.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### GetCacheTtl
**Purpose**  
Retrieves the cache time‑to‑live as a `TimeSpan` from the supplied options.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to read from.

**Return value**  
A `TimeSpan` indicating how long cached data is considered valid.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### GetBatchSize
**Purpose**  
Retrieves the maximum number of items processed in a single batch.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to read from.

**Return value**  
An `int` indicating the batch size.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### IsVerboseLoggingEnabled
**Purpose**  
Determines whether verbose logging is enabled for the worker.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to read from.

**Return value**  
`true` if verbose logging is enabled; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### CloneOptions
**Purpose**  
Creates a deep copy of the supplied `ServiceWorkerOptions`.

**Parameters**  
- `options`: The `ServiceWorkerOptions` to clone.

**Return value**  
A new `ServiceWorkerOptions` instance with the same property values as `options`.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.

### WithUpdateInterval
**Purpose**  
Returns a new `ServiceWorkerOptions` instance with the update interval modified.

**Parameters**  
- `options`: The original `ServiceWorkerOptions`.  
- `intervalMs`: The new update interval in milliseconds; must be greater than zero.

**Return value**  
A new `ServiceWorkerOptions` object reflecting the updated interval.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.  
- `ArgumentOutOfRangeException` if `intervalMs` is less than or equal to zero.

### WithErrorBackoff
**Purpose**  
Returns a new `ServiceWorkerOptions` instance with the error back‑off interval modified.

**Parameters**  
- `options`: The original `ServiceWorkerOptions`.  
- `backoffMs`: The new error back‑off interval in milliseconds; must be greater than zero.

**Return value**  
A new `ServiceWorkerOptions` object reflecting the updated back‑off.

**Exceptions**  
- `ArgumentNullException` if `options` is `null`.  
- `ArgumentOutOfRangeException` if `backoffMs` is less than or equal to zero.

### LogConfiguration
**Purpose**  
Writes the current configuration of a `ServiceWorkerOptions` instance to the supplied logger at the `Information` level.

**Parameters**  
- `logger`: The `ILogger` used for output.  
- `options`: The `ServiceWorkerOptions` to log.

**Return value**  
None.

**Exceptions**  
- `ArgumentNullException` if `logger` or `options` is `null`.

## Usage

### Example 1: Registering the worker in an ASP.NET Core application
```csharp
using Microsoft.Extensions.DependencyInjection;
using System.ServiceMonitor; // namespace containing the extensions

var builder = WebApplication.CreateBuilder(args);

var workerOptions = new ServiceWorkerOptions
{
    UpdateIntervalMs = 5000,
    ErrorBackoffMs = 15000,
    CacheTtl = TimeSpan.FromMinutes(1),
    BatchSize = 100,
    VerboseLogging = true
};

builder.Services.ConfigureServiceStatusUpdateWorker(workerOptions);

var app = builder.Build();
app.Run();
```

### Example 2: Inspecting and modifying options at runtime
```csharp
using System.ServiceMonitor;
using Microsoft.Extensions.Logging;

// Assume 'options' is obtained from configuration or DI
ServiceWorkerOptions options = GetOptionsFromSomewhere();

// Read current settings
int interval = ServiceStatusUpdateWorkerExtensions.GetUpdateIntervalMs(options);
bool verbose = ServiceStatusUpdateWorkerExtensions.IsVerboseLoggingEnabled(options);

// Create a modified copy with a new update interval
ServiceWorkerOptions updatedOpts = ServiceStatusUpdateWorkerExtensions.WithUpdateInterval(options, 10000);

// Log the final configuration
ILogger logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
ServiceStatusUpdateWorkerExtensions.LogConfiguration(logger, updatedOpts);
```

## Notes
- All extension methods are stateless and thread‑safe; they only read from or produce new instances of `ServiceWorkerOptions` without modifying shared state.
- Methods that return a new `ServiceWorkerOptions` (`CloneOptions`, `WithUpdateInterval`, `WithErrorBackoff`) leave the original instance unchanged, supporting immutable‑style usage patterns.
- Passing `null` for any reference‑type argument results in an `ArgumentNullException`; callers should validate inputs before invoking these members.
- The `LogConfiguration` method does not alter the logger’s state; it merely emits a log entry and can be called concurrently from multiple threads.
