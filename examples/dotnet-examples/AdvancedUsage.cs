using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SystemdServiceMonitor.Services;

// This example demonstrates monitoring a service continuously and handling statistics.
public class AdvancedUsage
{
    public async Task RunExampleAsync(IServiceProvider serviceProvider)
    {
        var monitorService = serviceProvider.GetRequiredService<IServiceMonitorService>();
        var cts = new CancellationTokenSource();

        // 1. Start monitoring a service
        Console.WriteLine("Starting monitoring...");
        await monitorService.StartMonitoringAsync("nginx.service", intervalMs: 2000, ct: cts.Token);

        // 2. Retrieve aggregated statistics
        var stats = await monitorService.GetStatisticsAsync();
        Console.WriteLine($"Monitored: {stats.MonitoredServices} services");
        Console.WriteLine($"Avg CPU: {stats.AverageCpuUsage}%");
        Console.WriteLine($"Avg Mem: {stats.AverageMemoryUsage} MB");

        // 3. Stop after some time
        await Task.Delay(10000, cts.Token);
        await monitorService.StopMonitoringAsync("nginx.service");
        Console.WriteLine("Monitoring stopped.");
    }
}
