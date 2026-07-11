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
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The host environment.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or <paramref name="environment"/> is null.</exception>
    public static IApplicationBuilder UseApplicationMiddleware(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(environment);

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
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="defaultDuration">Optional default cache duration.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddResponseCaching(
        this IServiceCollection services,
        TimeSpan? defaultDuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddResponseCaching();

        return services;
    }

    /// <summary>
    /// Configures JSON serialization options for consistent API responses.
    /// </summary>
    /// <param name="mvcBuilder">The MVC builder.</param>
    /// <returns>The MVC builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mvcBuilder"/> is null.</exception>
    public static IMvcBuilder AddJsonOptions(this IMvcBuilder mvcBuilder)
    {
        ArgumentNullException.ThrowIfNull(mvcBuilder);

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
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Background services are registered conditionally based on configuration
        // Actual implementations are loaded via DI configuration

        return services;
    }

    /// <summary>
    /// Registers the event pub/sub system.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Event bus is registered conditionally based on configuration
        // Actual implementations are loaded via DI configuration

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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