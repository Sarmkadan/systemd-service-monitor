#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Serilog;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Extensions;
using SystemdServiceMonitor.Filters;
using SystemdServiceMonitor.Middleware;
using SystemdServiceMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging with Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/systemd-monitor-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add configuration
var systemdOptions = builder.Configuration.GetSection("Systemd").Get<SystemdOptions>() ?? new();
var dbOptions = builder.Configuration.GetSection("Database").Get<DatabaseOptions>() ?? new();

builder.Services.AddSingleton(systemdOptions);
builder.Services.AddSingleton(dbOptions);

// Register core services
builder.Services.AddScoped<ISystemdConnectionService, SystemdConnectionService>();
builder.Services.AddScoped<IServiceMonitorService, ServiceMonitorService>();
builder.Services.AddScoped<IServiceLogService, ServiceLogService>();
builder.Services.AddScoped<IResourceMonitorService, ResourceMonitorService>();
builder.Services.AddScoped<IServiceControlService, ServiceControlService>();

// Register repositories
builder.Services.AddSingleton<IServiceRepository, ServiceRepository>();
builder.Services.AddSingleton<ILogRepository, LogRepository>();
builder.Services.AddSingleton<IMetricRepository, MetricRepository>();

// Register application services from extensions
builder.Services.AddApplicationServices();
builder.Services.AddEventBus();
builder.Services.AddBackgroundServices();
builder.Services.AddLogStreaming();

// Register alert rules engine with escalation policies and on-call rotation
builder.Services.AddAlertRulesEngine(builder.Configuration);

// Configure caching
builder.Services.Configure<SystemdServiceMonitor.Caching.CacheOptions>(options =>
{
    options.DefaultTtlSeconds = 300;
    options.MaxSizeMb = 100;
});

builder.Services.AddControllers(options =>
{
    // Add global filters
    options.Filters.Add<ApiExceptionFilter>();
    options.Filters.Add<ValidateModelFilter>();
})
.AddJsonOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiDocumentation();
builder.Services.AddResponseCaching();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use application middleware
app.UseApplicationMiddleware(app.Environment);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "systemd-service-monitor API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseResponseCaching();
app.UseAuthorization();
app.MapControllers();
app.MapLogStreamEndpoints();

// Map health check endpoint
app.MapHealthChecks("/health");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
