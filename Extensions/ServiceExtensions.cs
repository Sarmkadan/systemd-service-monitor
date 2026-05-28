#nullable enable

using Microsoft.OpenApi.Models;
using SystemdServiceMonitor.Caching;
using SystemdServiceMonitor.Integration;
using SystemdServiceMonitor.Middleware;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for dependency injection container registration.
/// Provides fluent API for registering application services, middleware, and configuration.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers core application services into the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register caching services
        services.AddSingleton<IServiceCache, MemoryCacheProvider>();
        services.AddSingleton<DBusConnectionManager>();

        // Add health checks
        services.AddHealthChecks();

        services.AddScoped<IServiceDependencyGraphService, ServiceDependencyGraphService>();

        return services;
    }

    /// <summary>
    /// Registers all middleware components and configures the request pipeline.
    /// </summary>
    public static IApplicationBuilder UseApplicationMiddleware(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        // Error handling should be first in pipeline
        app.UseErrorHandling();

        // Logging and request tracking
        app.UseRequestLogging();

        // Rate limiting
        app.UseRateLimiting(options =>
        {
            options.RequestsPerMinute = 300;
            options.RefillIntervalSeconds = 60;
        });

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        return app;
    }

    /// <summary>
    /// Registers API response caching with configurable policies.
    /// </summary>
    public static IServiceCollection AddResponseCaching(
        this IServiceCollection services,
        TimeSpan? defaultDuration = null)
    {
        services.AddResponseCaching();

        return services;
    }

    /// <summary>
    /// Configures JSON serialization options for consistent API responses.
    /// </summary>
    public static IMvcBuilder AddJsonOptions(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        return mvcBuilder;
    }

    /// <summary>
    /// Registers background services and scheduled tasks.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // Register hosted services for background tasks
        // services.AddHostedService<ServiceStatusUpdateWorker>();
        // services.AddHostedService<LogCollectionWorker>();
        // services.AddHostedService<MetricsCollectionWorker>();

        return services;
    }

    /// <summary>
    /// Registers the event pub/sub system.
    /// </summary>
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        // Register event publisher and observer registry
        // services.AddSingleton<IServiceEventPublisher, ServiceEventPublisher>();
        // services.AddSingleton<EventObserverRegistry>();

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation.
    /// </summary>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "systemd-service-monitor API",
                Version = "v1",
                Description = "REST API for monitoring and controlling systemd services via D-Bus"
            });

            // Include XML documentation if available
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
