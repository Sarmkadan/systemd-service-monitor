// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Serilog.Core;
using Serilog.Events;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Enriches Serilog log events with additional contextual information.
/// Automatically adds correlation IDs, request information, and other useful context to all logs.
/// </summary>
public class LogContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public LogContextEnricher(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_httpContextAccessor?.HttpContext == null)
            return;

        var context = _httpContextAccessor.HttpContext;

        // Add request ID
        if (context.Items.TryGetValue("RequestId", out var requestId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestId", requestId));
        }

        // Add correlation ID
        if (context.Items.TryGetValue("CorrelationId", out var correlationId))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        }

        // Add user information
        if (context.User?.Identity?.Name != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("User", context.User.Identity.Name));
        }

        // Add HTTP request information
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Method", context.Request.Method));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Path", context.Request.Path.Value));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("QueryString", context.Request.QueryString.Value));

        // Add client IP
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", clientIp));

        // Add response status code if available
        if (context.Response.StatusCode > 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("StatusCode", context.Response.StatusCode));
        }
    }
}

/// <summary>
/// Serilog enricher extensions for fluent configuration.
/// </summary>
public static class LogEnricherExtensions
{
    /// <summary>
    /// Adds the LogContextEnricher to the logger configuration.
    /// </summary>
    public static Serilog.LoggerConfiguration WithContextEnricher(
        this Serilog.LoggerConfiguration config,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        return config.Enrich.With(new LogContextEnricher(httpContextAccessor));
    }

    /// <summary>
    /// Adds contextual properties to the logging scope.
    /// </summary>
    public static IDisposable PushContext(string key, object value)
    {
        return Serilog.Context.LogContext.PushProperty(key, value);
    }

    /// <summary>
    /// Pushes a correlation ID to the logging context.
    /// </summary>
    public static IDisposable PushCorrelationId(string correlationId)
    {
        return Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId);
    }

    /// <summary>
    /// Pushes a request ID to the logging context.
    /// </summary>
    public static IDisposable PushRequestId(string requestId)
    {
        return Serilog.Context.LogContext.PushProperty("RequestId", requestId);
    }
}

/// <summary>
/// Helper class for managing structured logging context.
/// </summary>
public class StructuredLogContext : IDisposable
{
    private readonly Dictionary<string, IDisposable> _properties = new();

    public void AddProperty(string key, object value)
    {
        if (_properties.ContainsKey(key))
        {
            _properties[key].Dispose();
        }

        _properties[key] = Serilog.Context.LogContext.PushProperty(key, value);
    }

    public void RemoveProperty(string key)
    {
        if (_properties.TryGetValue(key, out var prop))
        {
            prop.Dispose();
            _properties.Remove(key);
        }
    }

    public void Dispose()
    {
        foreach (var prop in _properties.Values)
        {
            prop?.Dispose();
        }

        _properties.Clear();
    }
}
