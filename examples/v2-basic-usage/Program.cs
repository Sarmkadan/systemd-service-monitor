#nullable enable
using System;
using System.Threading.Tasks;
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;

namespace V2BasicUsage
{
    /// <summary>
    /// Basic example showing v2.0 features including the new alert rules engine
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize the service monitor
            var monitor = new ServiceMonitorService();

            // Basic service status check (v1.x functionality)
            var status = await monitor.CheckServiceStatusAsync("example-service");
            Console.WriteLine($"Service status: {status}");

            // New v2.0 alert rules engine usage
            var alertEngine = new AlertRulesEngine();

            // Create a basic alert rule
            var rule = new AlertRule
            {
                Name = "ServiceDown",
                Condition = "ActiveState == 'inactive'",
                Severity = "High"
            };

            // Evaluate the rule
            var alerts = alertEngine.EvaluateRule(rule, service);

            if (alerts.Any())
            {
                // Handle alerts (new v2.0 feature)
                foreach (var alert in alerts)
                {
                    Console.WriteLine($"Alert triggered: {alert.Name} - {alert.Description}");
                }
            }
        }
    }
}