# LogContextEnricher

LogContextEnricher is a Serilog enricher that allows ambient properties to be added to log events through a disposable scope. It provides static helpers to push context, correlation IDs, and request IDs, as well as an instance‑based API for custom property management.

## API

### `public LogContextEnricher()`
Creates a new enricher instance with an empty property collection. The instance is ready to be supplied to Serilog’s configuration pipeline. No parameters. Does not throw under normal circumstances.

### `public void Enrich(Serilog.Events.LogEvent logEvent, Serilog.ILogEventPropertyFactory propertyFactory)`
Adds all currently stored properties to the supplied `logEvent`.  
- **Parameters**  
  - `logEvent`: The log event being enriched; must not be `null`.  
  - `propertyFactory`: Factory used to create property objects; must not be `null`.  
- **Return value**: None (void).  
- **Throws**: `ArgumentNullException` if either parameter is `null`. The method does not modify the enricher’s internal state.

### `public static Serilog.LoggerConfiguration WithContextEnricher(this Serilog.LoggerConfiguration loggerConfiguration)`
Extension method that registers the enricher with Serilog.  
- **Parameters**  
  - `loggerConfiguration`: The configuration to extend; must not be `null`.  
- **Return value**: The same `LoggerConfiguration` instance, allowing fluent chaining.  
- **Throws**: `ArgumentNullException` if `loggerConfiguration` is `null`.

### `public static IDisposable PushContext(string key, object value)`
Creates a scoped context property that is automatically removed when the returned `IDisposable` is disposed.  
- **Parameters**  
  - `key`: Property name; must not be `null` or empty.  
  - `value`: Property value; may be `null`.  
- **Return value**: An `IDisposable` token; disposing it removes the property.  
- **Throws**: `ArgumentException` if `key` is `null` or empty.

### `public static IDisposable PushCorrelationId(string correlationId)`
Convenience overload that pushes a property named `"CorrelationId"` with the supplied value.  
- **Parameters**  
  - `correlationId`: The correlation identifier; must not be `null` or empty.  
- **Return value**: An `IDisposable` token that removes the `"CorrelationId"` property when disposed.  
- **Throws**: `ArgumentException` if `correlationId` is `null` or empty.

### `public static IDisposable PushRequestId(string requestId)`
Convenience overload that pushes a property named `"RequestId"` with the supplied value.  
- **Parameters**  
  - `requestId`: The request identifier; must not be `null` or empty.  
- **Return value**: An `IDisposable` token that removes the `"RequestId"` property when disposed.  
- **Throws**: `ArgumentException` if `requestId` is `null` or empty.

### `public void AddProperty(string key, object value)`
Adds or updates a property in the enricher’s internal collection.  
- **Parameters**  
  - `key`: Property name; must not be `null` or empty.  
  - `value`: Property value; may be `null`.  
- **Return value**: None (void).  
- **Throws**: `ArgumentException` if `key` is `null` or empty.

### `public void RemoveProperty(string key)`
Removes a property from the enricher’s internal collection if it exists.  
- **Parameters**  
  - `key`: Property name to remove; must not be `null` or empty.  
- **Return value**: None (void).  
- **Throws**: `ArgumentException` if `key` is `null` or empty. No exception is thrown if the key is not present.

### `public void Dispose()`
Clears all stored properties and releases any internal resources. After disposal, further calls to `AddProperty`, `RemoveProperty`, or `Enrich` will have no effect or may throw `ObjectDisposedException` depending on implementation.  
- **Parameters**: None.  
- **Return value**: None (void).  
- **Throws**: May throw `ObjectDisposedException` if the enricher is used after disposal.

## Usage

### Example 1: Configuring a logger with scoped enrichment
```csharp
using Serilog;
using System;

Log.Logger = new LoggerConfiguration()
    .WithContextEnricher()
    .WriteTo.Console()
    .CreateLogger();

using (LogContextEnricher.PushCorrelationId("abc-123"))
using (LogContextEnricher.PushRequestId("req-456"))
{
    Log.Information("Processing request"); // Contains CorrelationId and RequestId
}

// After the using blocks the properties are removed automatically.
Log.Information("Request finished"); // No CorrelationId/RequestId
```

### Example 2: Manual property management
```csharp
using Serilog;
using System;

var enricher = new LogContextEnricher();
enricher.AddProperty("Environment", "Production");
enricher.AddProperty("Version", "1.2.3");

Log.Logger = new LoggerConfiguration()
    .Enrich.With(enricher) // Assuming an overload that accepts ILogEventEnricher
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Application started"); // Logs with Environment and Version

enricher.RemoveProperty("Version");
Log.Information("Version property removed"); // Only Environment remains

enricher.Dispose(); // Clean up
```

## Notes
- The static `Push*` methods are thread‑safe; each call returns an independent `IDisposable` token that only affects the ambient state for the thread that created it, assuming the underlying implementation uses `AsyncLocal` or similar.  
- Instance members (`AddProperty`, `RemoveProperty`, `Enrich`, `Dispose`) are **not** thread‑safe; concurrent access from multiple threads without external synchronization may lead to undefined behavior.  
- Disposing an enricher more than once is safe; subsequent calls to `Dispose` have no effect.  
- After calling `Dispose`, the enricher should not be used for further enrichment; attempting to do so may result in `ObjectDisposedException`.  
- The enricher does not capture exceptions thrown by property value `ToString()` implementations; such exceptions propagate through Serilog’s pipeline as usual.  
- When using the static push methods, the returned disposable must be disposed (e.g., via `using`) to avoid leaking properties into subsequent log events.
