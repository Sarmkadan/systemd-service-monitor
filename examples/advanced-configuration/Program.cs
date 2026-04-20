using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

namespace AdvancedConfiguration
{
    /// <summary>
    /// Advanced configuration example showing complex alert rules and escalation policies
    /// </summary>
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize with advanced configuration
            var config = new SystemdOptions
            {
                Services = new ServiceOptions
                {
                    EnableMonitoring = true,
                    MonitoringInterval = TimeSpan.FromSeconds(30)
                }
            };

            var alertEngine = new AlertRulesEngine();

            // Configure complex alert rules
            var alertRules = new List<AlertRule>
            {
                new AlertRule
                {
                    Name = "ServiceDown",
                    Type = "StatusCheck",
                    Condition = "ActiveState != 'active'",
                    Severity = "High",
                    Description = "Service is not in active state"
                },
                new AlertRule
                {
                    Name = "HighMemoryUsage",
                    Type = "ResourceCheck",
                    Condition = "MemoryUsage > 100MB",
                    Severity = "Medium",
                    Description = "Service using excessive memory"
                }
            };

            // Configure escalation policies
            var escalationPolicy = new EscalationPolicy
            {
                Name = "DefaultEscalation",
                Steps = new List<EscalationStep>
                {
                    new EscalationStep
                    {
                        Level = 1,
                        Timeout = TimeSpan.FromMinutes(5),
                        Action = "email",
                        Recipients = new List<string> { "admin@example.com" }
                    },
                    new EscalationStep
                    {
                        Level = 2,
                        Timeout = TimeSpan.FromMinutes(10),
                        Action = "sms",
                        Recipients = new List<string> { "admin-phone@example.com" }
                    }
                }
            };

            Console.WriteLine("Advanced configuration loaded with alert rules and escalation policies.");
        }
    }
}