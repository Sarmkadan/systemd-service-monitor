// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Factory for creating and initializing service objects.
/// Provides convenient methods for constructing domain objects with sensible defaults.
/// </summary>
public static class ServiceFactory
{
    /// <summary>
    /// Creates a new ServiceInfo with default values.
    /// </summary>
    public static ServiceInfo CreateServiceInfo(
        string unitName,
        string description = "",
        string state = "Inactive")
    {
        return new ServiceInfo
        {
            Id = Guid.NewGuid(),
            UnitName = unitName,
            Description = description,
            State = Enum.TryParse<Enums.ServiceState>(state, true, out var parsedState)
                ? parsedState
                : Enums.ServiceState.Unknown,
            SubState = Enums.ServiceSubState.Unknown,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new ServiceMetric with default values.
    /// </summary>
    public static ServiceMetric CreateServiceMetric(string serviceName)
    {
        return new ServiceMetric
        {
            Id = Guid.NewGuid(),
            ServiceName = serviceName,
            Timestamp = DateTime.UtcNow,
            CpuPercentage = 0,
            MemoryUsageMb = 0,
            DiskReadBytesPerSec = 0,
            DiskWriteBytesPerSec = 0,
            NetworkBytesIn = 0,
            NetworkBytesOut = 0
        };
    }

    /// <summary>
    /// Creates a new ServiceLog entry.
    /// </summary>
    public static ServiceLog CreateServiceLog(
        string unit,
        string message,
        string severity = "INFO")
    {
        return new ServiceLog
        {
            Id = Guid.NewGuid(),
            Unit = unit,
            Message = message,
            Severity = severity,
            Timestamp = DateTime.UtcNow,
            Priority = GetPriorityFromSeverity(severity)
        };
    }

    /// <summary>
    /// Creates a new ServiceStatus snapshot.
    /// </summary>
    public static ServiceStatus CreateServiceStatus(ServiceInfo service)
    {
        return new ServiceStatus
        {
            Id = Guid.NewGuid(),
            ServiceId = service.Id,
            ServiceName = service.UnitName,
            IsActive = service.State.ToString() == "Active",
            State = service.State.ToString(),
            SubState = service.SubState.ToString(),
            MainPid = service.MainProcessId,
            Uptime = TimeSpan.FromSeconds(service.UptimeSeconds),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a RestartPolicyConfig from a policy string.
    /// </summary>
    public static RestartPolicyConfig CreateRestartPolicy(
        string policyName,
        int delaySec = 100,
        int maxAttempts = 5)
    {
        return new RestartPolicyConfig
        {
            Policy = policyName,
            RestartDelaySeconds = delaySec,
            MaxRestartAttempts = maxAttempts,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a ServiceInfo to a flattened details object for API responses.
    /// </summary>
    public static Dictionary<string, object> ServiceInfoToDictionary(ServiceInfo service)
    {
        return new Dictionary<string, object>
        {
            { "id", service.Id },
            { "unitName", service.UnitName },
            { "description", service.Description },
            { "state", service.State.ToString() },
            { "subState", service.SubState.ToString() },
            { "mainProcessId", service.MainProcessId },
            { "restart Count", service.RestartCount },
            { "autoStart", service.AutoStart },
            { "uptime", service.UptimeSeconds },
            { "runAsUser", service.RunAsUser },
            { "runAsGroup", service.RunAsGroup },
            { "createdAt", service.CreatedAt },
            { "updatedAt", service.UpdatedAt }
        };
    }

    /// <summary>
    /// Gets priority value from severity string.
    /// Used for log filtering and severity-based operations.
    /// </summary>
    private static int GetPriorityFromSeverity(string severity)
    {
        return severity?.ToUpperInvariant() switch
        {
            "EMERG" or "EMERGENCY" => 0,
            "ALERT" => 1,
            "CRIT" or "CRITICAL" => 2,
            "ERR" or "ERROR" => 3,
            "WARN" or "WARNING" => 4,
            "NOTICE" => 5,
            "INFO" => 6,
            "DEBUG" => 7,
            _ => 6 // Default to INFO
        };
    }

    /// <summary>
    /// Batch creates ServiceInfo objects from a list of names.
    /// </summary>
    public static List<ServiceInfo> CreateServicesFromNames(params string[] unitNames)
    {
        return unitNames
            .Select(name => CreateServiceInfo(name))
            .ToList();
    }
}
