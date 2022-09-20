#nullable enable

using System.Text;
using System.Text.Json;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Utility class for formatting output in various formats (JSON, CSV, table).
/// Useful for CLI tools and export functionality.
/// </summary>
public static class OutputFormatter
{
    /// <summary>
    /// Formats services as a JSON string.
    /// </summary>
    public static string FormatAsJson<T>(IEnumerable<T> items, bool indent = true) where T : class
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indent,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(items, options);
    }

    /// <summary>
    /// Formats services as a CSV string.
    /// </summary>
    public static string FormatAsCsv(IEnumerable<ServiceInfo> services)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Name,State,SubState,ProcessId,Restarts,AutoStart,Uptime(s)");

        foreach (var service in services)
        {
            var row = $"{EscapeCsv(service.UnitName)}," +
                     $"{service.State}," +
                     $"{service.SubState}," +
                     $"{service.MainProcessId}," +
                     $"{service.RestartCount}," +
                     $"{service.AutoStart}," +
                     $"{service.UptimeSeconds}";
            csv.AppendLine(row);
        }

        return csv.ToString();
    }

    /// <summary>
    /// Formats services as a table string suitable for console output.
    /// </summary>
    public static string FormatAsTable(IEnumerable<ServiceInfo> services)
    {
        var serviceList = services.ToList();
        if (!serviceList.Any())
            return "No services found";

        // Column widths
        const int nameWidth = 30;
        const int stateWidth = 12;
        const int pidWidth = 10;
        const int restartsWidth = 10;
        const int uptimeWidth = 12;

        var sb = new StringBuilder();

        // Header
        sb.AppendLine(new string('═', nameWidth + stateWidth + pidWidth + restartsWidth + uptimeWidth + 8));
        sb.AppendLine($"{"SERVICE",-nameWidth} {"STATE",-stateWidth} {"PID",-pidWidth} {"RESTARTS",-restartsWidth} {"UPTIME",-uptimeWidth}");
        sb.AppendLine(new string('─', nameWidth + stateWidth + pidWidth + restartsWidth + uptimeWidth + 8));

        // Rows
        foreach (var service in serviceList)
        {
            var uptime = ServiceHealthChecker.FormatUptime(service.UptimeSeconds);
            sb.AppendLine($"{Truncate(service.UnitName, nameWidth),-nameWidth} " +
                         $"{service.State.ToString(),-stateWidth} " +
                         $"{service.MainProcessId,-pidWidth} " +
                         $"{service.RestartCount,-restartsWidth} " +
                         $"{uptime,-uptimeWidth}");
        }

        sb.AppendLine(new string('═', nameWidth + stateWidth + pidWidth + restartsWidth + uptimeWidth + 8));

        return sb.ToString();
    }

    /// <summary>
    /// Formats metrics as a key-value table.
    /// </summary>
    public static string FormatMetricsAsTable(SystemResource metrics)
    {
        var sb = new StringBuilder();
        sb.AppendLine("System Metrics");
        sb.AppendLine(new string('─', 40));
        sb.AppendLine($"CPU Usage:           {metrics.CpuUsagePercent:F2}%");
        sb.AppendLine($"Memory Usage:        {metrics.MemoryUsagePercent:F2}%");
        sb.AppendLine($"Memory Available:    {metrics.AvailableMemoryMb} MB");
        sb.AppendLine($"Disk Usage:          {metrics.DiskUsagePercent:F2}%");
        sb.AppendLine($"Disk Available:      {metrics.AvailableDiskGb:F2} GB");
        sb.AppendLine($"Last Updated:        {metrics.RecordedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('─', 40));

        return sb.ToString();
    }

    /// <summary>
    /// Formats a service as a detailed text summary.
    /// </summary>
    public static string FormatServiceDetails(ServiceInfo service)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Service: {service.UnitName}");
        sb.AppendLine(new string('─', 50));
        sb.AppendLine($"Description:         {service.Description}");
        sb.AppendLine($"State:               {service.State} ({service.SubState})");
        sb.AppendLine($"Main Process ID:     {service.MainProcessId}");
        sb.AppendLine($"Uptime:              {ServiceHealthChecker.FormatUptime(service.UptimeSeconds)}");
        sb.AppendLine($"Restart Count:       {service.RestartCount}");
        sb.AppendLine($"Auto-start:          {service.AutoStart}");
        sb.AppendLine($"Restart Policy:      {service.RestartPolicy}");
        sb.AppendLine($"Run As User:         {service.RunAsUser}");
        sb.AppendLine($"Run As Group:        {service.RunAsGroup}");
        sb.AppendLine($"Last Start:          {(service.LastStartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never")}");
        sb.AppendLine($"Last Stop:           {(service.LastStopTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never")}");

        if (service.Dependencies.Any())
            sb.AppendLine($"Dependencies:        {string.Join(", ", service.Dependencies)}");

        if (service.Dependents.Any())
            sb.AppendLine($"Dependents:          {string.Join(", ", service.Dependents)}");

        sb.AppendLine(new string('─', 50));

        return sb.ToString();
    }

    /// <summary>
    /// Creates a simple progress bar string.
    /// </summary>
    public static string CreateProgressBar(double percentage, int width = 20)
    {
        if (percentage < 0 || percentage > 100)
            percentage = Math.Clamp(percentage, 0, 100);

        var filledChars = (int)((percentage / 100) * width);
        var emptyChars = width - filledChars;

        var bar = new string('█', filledChars) + new string('░', emptyChars);
        return $"[{bar}] {percentage:F1}%";
    }

    /// <summary>
    /// Escapes a string for safe CSV inclusion.
    /// </summary>
    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    /// <summary>
    /// Truncates a string to the specified length.
    /// </summary>
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - 3) + "...";
    }
}
