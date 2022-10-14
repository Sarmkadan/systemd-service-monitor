using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SystemdServiceMonitor.Services;

// This example demonstrates how to retrieve and display information about a systemd service.
public class BasicUsage
{
    public async Task RunAsync(IServiceProvider serviceProvider)
    {
        // 1. Resolve the monitor service from DI
        var monitorService = serviceProvider.GetRequiredService<IServiceMonitorService>();

        // 2. Fetch details for a specific service (e.g., 'nginx.service')
        string unitName = "nginx.service";
        var service = await monitorService.GetServiceByNameAsync(unitName);

        if (service != null)
        {
            Console.WriteLine($"Service: {service.Name}");
            Console.WriteLine($"State: {service.State}");
            Console.WriteLine($"PID: {service.Pid}");
        }
        else
        {
            Console.WriteLine($"Service {unitName} not found.");
        }
    }
}
