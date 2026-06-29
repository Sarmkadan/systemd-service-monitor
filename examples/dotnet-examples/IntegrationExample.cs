using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Caching;
using SystemdServiceMonitor.Integration;

// This example demonstrates how to configure the systemd-service-monitor services in an ASP.NET Core application.
public class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 1. Register required dependencies
        services.AddSingleton<IServiceCache, MemoryCacheProvider>();
        services.AddSingleton<ISystemdConnectionService, SystemdConnectionService>();
        
        // 2. Register the core monitoring service
        services.AddSingleton<IServiceMonitorService, ServiceMonitorService>();

        // 3. Optional: Register other dependent services
        services.AddSingleton<IServiceControlService, ServiceControlService>();
        services.AddSingleton<IServiceLogService, ServiceLogService>();
        services.AddSingleton<IAlertRulesEngine, AlertRulesEngine>();
    }
}
