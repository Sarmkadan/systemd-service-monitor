#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SystemdMonitorExamples
{
    /// <summary>
    /// Simple REST API client for systemd-service-monitor
    /// Demonstrates how to interact with the API programmatically
    /// </summary>
    public class ServiceMonitorClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;

        public ServiceMonitorClient(string baseUrl = "https://localhost:5001")
        {
            _baseUrl = baseUrl;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _client = new HttpClient(handler);
        }

        /// <summary>
        /// Get list of all services
        /// </summary>
        public async Task<List<ServiceInfo>> GetServicesAsync(int page = 1, int pageSize = 10)
        {
            var url = $"{_baseUrl}/api/services?page={page}&pageSize={pageSize}";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);
            var services = new List<ServiceInfo>();

            if (document.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    services.Add(new ServiceInfo
                    {
                        Name = item.GetProperty("name").GetString() ?? "",
                        DisplayName = item.GetProperty("displayName").GetString() ?? "",
                        State = item.GetProperty("state").GetString() ?? "",
                        IsActive = item.GetProperty("status").GetProperty("isActive").GetBoolean()
                    });
                }
            }

            return services;
        }

        /// <summary>
        /// Get detailed information about a service
        /// </summary>
        public async Task<ServiceDetails> GetServiceDetailsAsync(string serviceName)
        {
            var url = $"{_baseUrl}/api/services/{serviceName}";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);
            var dataElement = document.RootElement.GetProperty("data");

            return new ServiceDetails
            {
                Name = dataElement.GetProperty("name").GetString() ?? "",
                State = dataElement.GetProperty("state").GetString() ?? "",
                Description = dataElement.GetProperty("description").GetString() ?? "",
                IsActive = dataElement.GetProperty("status").GetProperty("isActive").GetBoolean(),
                UptimeSeconds = dataElement.GetProperty("status").GetProperty("uptimeSeconds").GetInt32(),
                Pid = dataElement.TryGetProperty("pid", out var pid) ? pid.GetInt32() : 0
            };
        }

        /// <summary>
        /// Start a service
        /// </summary>
        public async Task<bool> StartServiceAsync(string serviceName)
        {
            var url = $"{_baseUrl}/api/services/{serviceName}/start";
            var response = await _client.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Stop a service
        /// </summary>
        public async Task<bool> StopServiceAsync(string serviceName)
        {
            var url = $"{_baseUrl}/api/services/{serviceName}/stop";
            var response = await _client.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Restart a service
        /// </summary>
        public async Task<bool> RestartServiceAsync(string serviceName, string mode = "always")
        {
            var url = $"{_baseUrl}/api/services/{serviceName}/restart";
            var content = new StringContent(
                $"{{\"restartMode\": \"{mode}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");
            var response = await _client.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Get service logs
        /// </summary>
        public async Task<List<LogEntry>> GetServiceLogsAsync(string serviceName, int lines = 50)
        {
            var url = $"{_baseUrl}/api/services/{serviceName}/logs?lines={lines}";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);
            var logs = new List<LogEntry>();

            if (document.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("logs", out var logsElement))
            {
                foreach (var log in logsElement.EnumerateArray())
                {
                    logs.Add(new LogEntry
                    {
                        Timestamp = log.GetProperty("timestamp").GetString() ?? "",
                        Priority = log.GetProperty("priority").GetString() ?? "",
                        Message = log.GetProperty("message").GetString() ?? ""
                    });
                }
            }

            return logs;
        }

        /// <summary>
        /// Get system resources
        /// </summary>
        public async Task<SystemResources> GetSystemResourcesAsync()
        {
            var url = $"{_baseUrl}/api/system/resources";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var document = JsonDocument.Parse(json);
            var dataElement = document.RootElement.GetProperty("data");

            return new SystemResources
            {
                CpuPercent = dataElement.GetProperty("cpuPercent").GetDecimal(),
                MemoryUsedMb = dataElement.GetProperty("memoryUsedMb").GetInt32(),
                MemoryTotalMb = dataElement.GetProperty("memoryTotalMb").GetInt32(),
                DiskUsedGb = dataElement.GetProperty("diskUsedGb").GetInt32(),
                DiskTotalGb = dataElement.GetProperty("diskTotalGb").GetInt32()
            };
        }

        /// <summary>
        /// Check application health
        /// </summary>
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var url = $"{_baseUrl}/health";
                var response = await _client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class ServiceInfo
    {
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string State { get; set; } = "";
        public bool IsActive { get; set; }
    }

    public class ServiceDetails
    {
        public string Name { get; set; } = "";
        public string State { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; }
        public int UptimeSeconds { get; set; }
        public int Pid { get; set; }
    }

    public class LogEntry
    {
        public string Timestamp { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class SystemResources
    {
        public decimal CpuPercent { get; set; }
        public int MemoryUsedMb { get; set; }
        public int MemoryTotalMb { get; set; }
        public int DiskUsedGb { get; set; }
        public int DiskTotalGb { get; set; }
    }

    // Example usage
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new ServiceMonitorClient();

            Console.WriteLine("=== systemd Service Monitor Client ===\n");

            // Check health
            var isHealthy = await client.IsHealthyAsync();
            Console.WriteLine($"Application Health: {(isHealthy ? "✓ Healthy" : "✗ Unhealthy")}\n");

            // Get system resources
            Console.WriteLine("System Resources:");
            var resources = await client.GetSystemResourcesAsync();
            Console.WriteLine($"  CPU: {resources.CpuPercent}%");
            Console.WriteLine($"  Memory: {resources.MemoryUsedMb}/{resources.MemoryTotalMb} MB");
            Console.WriteLine($"  Disk: {resources.DiskUsedGb}/{resources.DiskTotalGb} GB\n");

            // List services
            Console.WriteLine("Active Services:");
            var services = await client.GetServicesAsync(pageSize: 10);
            foreach (var service in services)
            {
                var status = service.IsActive ? "✓" : "✗";
                Console.WriteLine($"  {status} {service.Name,-30} {service.State}");
            }

            // Get details for a specific service
            if (services.Count > 0)
            {
                var serviceName = services[0].Name;
                Console.WriteLine($"\nService Details: {serviceName}");
                var details = await client.GetServiceDetailsAsync(serviceName);
                Console.WriteLine($"  State: {details.State}");
                Console.WriteLine($"  PID: {details.Pid}");
                Console.WriteLine($"  Uptime: {details.UptimeSeconds}s");

                // Get logs
                Console.WriteLine($"\nRecent Logs ({serviceName}):");
                var logs = await client.GetServiceLogsAsync(serviceName, lines: 5);
                foreach (var log in logs)
                {
                    Console.WriteLine($"  [{log.Priority}] {log.Message}");
                }
            }
        }
    }
}
