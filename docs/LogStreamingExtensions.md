# LogStreamingExtensions

Extension methods for wiring up real-time log streaming in ASP.NET Core applications.

## AddLogStreaming

Registers `ILogStreamService` with the dependency injection container.

```csharp
public static IServiceCollection AddLogStreaming(this IServiceCollection services)
```

### Parameters

- `services`: The `IServiceCollection` to configure.

### Returns

The same `IServiceCollection` instance for chaining.

### Exceptions

- `ArgumentNullException`: Thrown when `services` is `null`.

## MapLogStreamEndpoints

Maps the `GET /api/stream/logs` Server-Sent Events endpoint for real-time log streaming.

```csharp
public static WebApplication MapLogStreamEndpoints(this WebApplication app)
```

### Parameters

- `app`: The `WebApplication` to register the route on.

### Returns

The same `WebApplication` instance for chaining.

### Exceptions

- `ArgumentNullException`: Thrown when `app` is `null`.

### Remarks

Clients connect with `Accept: text/event-stream` (or an `EventSource` in the browser).
Each SSE `data` frame carries a JSON-serialised `LogStreamEntry`.
Historical entries are sent first, then live entries arrive as they are produced.

#### Query parameters

- `serviceName`: Restrict to one unit (optional).
- `searchTerm`: Case-insensitive substring filter (optional).
- `minLevel`: Numeric syslog threshold; 0 = Emergency … 7 = Debug (optional).
- `bufferSize`: Historical entries to replay (default 50, max 500).
- `pollingIntervalMs`: Live-tail poll cadence in ms (default 2000, range 500–30 000).

### Example

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogStreaming();
var app = builder.Build();
app.MapLogStreamEndpoints();
app.Run();
```