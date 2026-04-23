#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Dtos;

/// <summary>
/// Data transfer object for service details.
/// Used in API responses to present service information in a consumable format.
/// </summary>
public class ServiceDetailsDto
{
    public Guid Id { get; set; }

    public string UnitName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string SubState { get; set; } = string.Empty;

    public int MainProcessId { get; set; }

    public string Result { get; set; } = string.Empty;

    public string RestartPolicy { get; set; } = string.Empty;

    public bool AutoStart { get; set; }

    public bool Restart { get; set; }

    public List<string> Dependencies { get; set; } = new();

    public List<string> Dependents { get; set; } = new();

    public DateTime? LastStartTime { get; set; }

    public DateTime? LastStopTime { get; set; }

    public long UptimeSeconds { get; set; }

    public int RestartCount { get; set; }

    public string RunAsUser { get; set; } = string.Empty;

    public string RunAsGroup { get; set; } = string.Empty;

    public string? StatusSummary { get; set; }

    public string? HealthStatus { get; set; }
}

/// <summary>
/// DTO for creating a new service configuration.
/// </summary>
public class CreateServiceDto
{
    public string UnitName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ExecStart { get; set; }

    public string? ExecStop { get; set; }

    public string? WorkingDirectory { get; set; }

    public string User { get; set; } = "root";

    public string Group { get; set; } = string.Empty;

    public bool AutoStart { get; set; } = false;

    public bool Restart { get; set; } = true;

    public string RestartPolicy { get; set; } = "always";
}

/// <summary>
/// DTO for updating service configuration.
/// </summary>
public class UpdateServiceDto
{
    public string? Description { get; set; }

    public bool? AutoStart { get; set; }

    public bool? Restart { get; set; }

    public string? RestartPolicy { get; set; }

    public int? RestartDelaySeconds { get; set; }

    public Dictionary<string, string>? EnvironmentVariables { get; set; }
}

/// <summary>
/// DTO for service control operations.
/// </summary>
public class ServiceControlDto
{
    public string Operation { get; set; } = string.Empty; // start, stop, restart, reload

    public int? TimeoutSeconds { get; set; } = 30;

    public bool Force { get; set; } = false;
}

/// <summary>
/// DTO for service status response.
/// </summary>
public class ServiceStatusDto
{
    public string UnitName { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string SubState { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int MainProcessId { get; set; }

    public long UptimeSeconds { get; set; }

    public int RestartCount { get; set; }

    public DateTime LastStateChange { get; set; } = DateTime.UtcNow;

    public string? StatusIcon => State.ToLower() switch
    {
        "active" => "✓",
        "inactive" => "○",
        "failed" => "✗",
        "activating" => "↻",
        "deactivating" => "↻",
        _ => "?"
    };
}

/// <summary>
/// DTO for batch service operations response.
/// </summary>
public class BatchOperationResultDto
{
    public int TotalServices { get; set; }

    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

    public List<ServiceOperationResultDto> Results { get; set; } = new();

    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// DTO for individual service operation result.
/// </summary>
public class ServiceOperationResultDto
{
    public string ServiceName { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? ErrorCode { get; set; }
}
