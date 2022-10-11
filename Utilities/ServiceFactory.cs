#nullable enable

using SystemdServiceMonitor.Enums;
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
            State = Enum.TryParse<ServiceState>(state, true, out var parsedState)
                ? parsedState
                : ServiceState.Unknown,
            SubState = ServiceSubState.Unknown,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new ServiceMetric with default values.
    /// </summary>
    public static ServiceMetric CreateServiceMetric(string unitName)
    {
        return new ServiceMetric
        {
            Id = Guid.NewGuid(),
            UnitName = unitName,
            Timestamp = DateTime.UtcNow,
            MetricType = MetricType.CpuUsage,
            Value = 0
        };
    }

    /// <summary>
    /// Creates a new ServiceLog entry.
    /// </summary>
    public static ServiceLog CreateServiceLog(
        string unitName,
        string message,
        string severity = "INFO")
    {
        var level = severity?.ToUpperInvariant() switch
        {
            "ERROR" or "ERR" => SyslogLevel.Error,
            "WARN" or "WARNING" => SyslogLevel.Warning,
            "DEBUG" => SyslogLevel.Debug,
            _ => SyslogLevel.Info
        };
        return new ServiceLog
        {
            Id = Guid.NewGuid(),
            UnitName = unitName,
            Message = message,
            Level = level,
            Timestamp = DateTime.UtcNow
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
            ServiceInfoId = service.Id,
            UnitName = service.UnitName,
            IsRunning = service.State == ServiceState.Active,
            State = service.State,
            SubState = service.SubState,
            ProcessId = service.MainProcessId,
            UptimeSeconds = service.UptimeSeconds,
            RecordedAt = DateTime.UtcNow
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
        Enum.TryParse<RestartPolicy>(policyName, true, out var policy);
        return new RestartPolicyConfig
        {
            PolicyType = policy,
            RestartDelaySec = delaySec,
            MaxRestarts = maxAttempts,
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
    /// Batch creates ServiceInfo objects from a list of names.
    /// </summary>
    public static List<ServiceInfo> CreateServicesFromNames(params string[] unitNames)
    {
        return unitNames
            .Select(name => CreateServiceInfo(name))
            .ToList();
    }
}
