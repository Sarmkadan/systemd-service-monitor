#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Extensions;

/// <summary>Extension methods for wiring up real-time log streaming.</summary>
public static class LogStreamingExtensions
{
    private static readonly JsonSerializerOptions SseJsonOptions = new()
    {
        PropertyNamingPolicy         = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition       = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Registers <see cref="ILogStreamService"/> with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddLogStreaming(this IServiceCollection services)
    {
        services.AddScoped<ILogStreamService, LogStreamService>();
        return services;
    }

    /// <summary>
    /// Maps the <c>GET /api/stream/logs</c> Server-Sent Events endpoint.
    /// </summary>
    /// <remarks>
    /// Clients connect with <c>Accept: text/event-stream</c> (or an <c>EventSource</c> in the browser).
    /// Each SSE <c>data</c> frame carries a JSON-serialised <see cref="LogStreamEntry"/>.
    /// Historical entries are sent first, then live entries arrive as they are produced.
    ///
    /// Query parameters:
    /// <list type="bullet">
    ///   <item><term>serviceName</term><description>Restrict to one unit (optional).</description></item>
    ///   <item><term>searchTerm</term><description>Case-insensitive substring filter (optional).</description></item>
    ///   <item><term>minLevel</term><description>Numeric syslog threshold; 0 = Emergency … 7 = Debug (optional).</description></item>
    ///   <item><term>bufferSize</term><description>Historical entries to replay (default 50, max 500).</description></item>
    ///   <item><term>pollingIntervalMs</term><description>Live-tail poll cadence in ms (default 2000, range 500–30 000).</description></item>
    /// </list>
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> to register the route on.</param>
    /// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
    public static WebApplication MapLogStreamEndpoints(this WebApplication app)
    {
        app.MapGet("/api/stream/logs", async (
            HttpContext            context,
            ILogStreamService      streamService,
            [FromQuery] string?    serviceName,
            [FromQuery] string?    searchTerm,
            [FromQuery] int?       minLevel,
            [FromQuery] int        bufferSize        = 50,
            [FromQuery] int        pollingIntervalMs = 2000,
            CancellationToken      ct                = default) =>
        {
            var response = context.Response;
            response.Headers.Append("Content-Type",      "text/event-stream");
            response.Headers.Append("Cache-Control",     "no-cache");
            response.Headers.Append("X-Accel-Buffering", "no");

            var filter = new LogStreamFilter
            {
                ServiceName       = serviceName,
                SearchTerm        = searchTerm,
                MinLevel          = minLevel.HasValue ? (SyslogLevel)minLevel.Value : null,
                BufferSize        = bufferSize,
                PollingIntervalMs = pollingIntervalMs,
            };

            await foreach (var entry in streamService.StreamLogsAsync(filter, ct))
            {
                var json = JsonSerializer.Serialize(entry, SseJsonOptions);
                await response.WriteAsync($"data: {json}\n\n", ct);
                await response.Body.FlushAsync(ct);
            }
        })
        .WithName("StreamLogs")
        .WithTags("Logs")
        .WithSummary("Stream real-time log entries via Server-Sent Events with optional filtering and full-text search")
        .Produces<LogStreamEntry>(200, "text/event-stream");

        return app;
    }
}
