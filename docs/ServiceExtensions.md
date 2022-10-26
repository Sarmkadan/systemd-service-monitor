# ServiceExtensions

The `ServiceExtensions` class provides a set of static extension methods designed to simplify the configuration of services, middleware, and application components within the `systemd-service-monitor` project. These methods act as entry points for dependency injection setup and pipeline configuration, ensuring consistent application bootstrapping.

## API

### AddApplicationServices
Registers core application services into the dependency injection container.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IServiceCollection`
*   **Throws:** `ArgumentNullException` if `services` is null.

### UseApplicationMiddleware
Adds essential middleware components to the application request pipeline.
*   **Parameters:** `IApplicationBuilder app`
*   **Return Value:** `IApplicationBuilder`
*   **Throws:** `ArgumentNullException` if `app` is null.

### AddResponseCaching
Configures and adds response caching services to the dependency injection container.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IServiceCollection`
*   **Throws:** `ArgumentNullException` if `services` is null.

### AddJsonOptions
Configures JSON serialization options for the MVC framework.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IMvcBuilder`
*   **Throws:** `ArgumentNullException` if `services` is null.

### AddBackgroundServices
Registers background worker services into the dependency injection container.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IServiceCollection`
*   **Throws:** `ArgumentNullException` if `services` is null.

### AddEventBus
Configures and registers event bus infrastructure services.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IServiceCollection`
*   **Throws:** `ArgumentNullException` if `services` is null.

### AddApiDocumentation
Registers services required for generating API documentation.
*   **Parameters:** `IServiceCollection services`
*   **Return Value:** `IServiceCollection`
*   **Throws:** `ArgumentNullException` if `services` is null.

## Usage

**Configuring Services**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices()
                .AddResponseCaching()
                .AddEventBus()
                .AddApiDocumentation();
```

**Configuring Middleware**
```csharp
var app = builder.Build();

app.UseApplicationMiddleware();
```

## Notes

*   **Thread Safety:** These extension methods are intended for use during the application's startup phase. They are not designed for concurrent use after the application has initialized.
*   **Dependency Injection:** These methods require an active `IServiceCollection` for registration and must be invoked before the application container is built.
*   **Order of Operations:** The order in which services are added may impact service resolution if dependencies exist between registered services. Middleware order configured via `UseApplicationMiddleware` is critical for request handling consistency.
