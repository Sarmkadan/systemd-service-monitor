// existing content ...

## ServiceStatus

The `ServiceStatus` class captures a point-in-time snapshot of a service's operational state, including performance metrics and health status. It tracks key attributes like CPU/memory usage, process state, and health check results to provide a comprehensive view of service health.

### Usage Example

```csharp
var status = new ServiceStatus
{
    ServiceInfoId = Guid.NewGuid(),
    UnitName = "nginx.service",
    State = ServiceState.Active,
    SubState = ServiceSubState.Running,
    IsEnabled = true,
    IsRunning = true,
    ProcessId = 1234,
    CpuUsagePercent = 2.5m,
    MemoryUsageMb = 50,
    HasFailed = false,
    ExitCode = 0,
    UptimeSeconds = 3600,
    HealthStatus = HealthStatus.Healthy,
    HealthMessage = "Service is running normally.",
    RecordedAt = DateTime.UtcNow
};

Console.WriteLine($"Service {status.UnitName} is {status.State} with {status.CpuUsagePercent}% CPU usage");
```

## ServiceInfo

The `ServiceInfo` class stores metadata and configuration about a systemd service, including its identity, dependencies, and operational policies. It provides a comprehensive view of a service's static configuration and relationships, which is used alongside `ServiceStatus` to monitor and manage services.

### Usage Example

```csharp
ServiceInfo info = new ServiceInfo
{
    Id = Guid.NewGuid(),
    LoadState = ServiceLoadState.Running,
    CpuUsagePercent = 50,
    MemoryUsageMb = 1024,
    UnitName = "nginx.service",
    Description = "High-performance HTTP server and reverse proxy",
    UnitFilePath = "/lib/systemd/system/nginx.service",
    State = ServiceState.Active,
    SubState = ServiceSubState.Running,
    MainProcessId = 1234,
    Result = "success",
    RestartPolicy = RestartPolicy.Always,
    AutoStart = true,
    Restart = true,
    Dependencies = new List<string> { "network.target", "syslog.socket" },
    Dependents = new List<string> { "website.target" },
    LastStartTime = DateTime.UtcNow.AddMinutes(-10),
    LastStopTime = null,
    UptimeSeconds = 600,
    RestartCount = 0
};

Console.WriteLine($"Service {info.UnitName} is configured with {info.Dependencies.Count} dependencies");
```

## AlertRule

The `AlertRule` class defines monitoring policies that evaluate service status against configurable conditions to automatically open incidents when services exceed defined thresholds or enter problematic states. It supports both state-based conditions (service failed, inactive) and metric-based conditions (CPU, memory, uptime, restarts) with configurable severity levels, cooldown periods, and escalation policies.

### Usage Example

```csharp
var cpuAlertRule = new AlertRule
{
    Name = "High CPU Usage Alert",
    Description = "Alert when CPU usage exceeds 80% for 5 consecutive evaluations",
    ServicePattern = "nginx*",
    Condition = AlertCondition.CpuThresholdExceeded,
    Threshold = 80.0m,
    Severity = AlertSeverity.High,
    CooldownMinutes = 30,
    ConsecutiveEvaluationsRequired = 5,
    Tags = new List<string> { "performance", "nginx", "production" }
};

var failedServiceRule = new AlertRule
{
    Name = "Service Failure Alert",
    Description = "Alert when any service enters failed state",
    ServicePattern = "*",
    Condition = AlertCondition.ServiceFailed,
    Severity = AlertSeverity.Critical,
    IsEnabled = true,
    Tags = new List<string> { "critical", "all-services" }
};

Console.WriteLine($"Created alert rule '{cpuAlertRule.Name}' for services matching '{cpuAlertRule.ServicePattern}'");
```

## ServiceLog

The `ServiceLog` class represents a structured log entry captured from systemd journald for a specific service. It provides comprehensive diagnostic details including severity level, process identifiers, source code location, and system-specific metadata to facilitate troubleshooting and service monitoring.

### Usage Example

```csharp
var log = new ServiceLog
{
    Id = Guid.NewGuid(),
    ServiceInfoId = Guid.NewGuid(),
    UnitName = "nginx.service",
    Level = SyslogLevel.Error,
    Message = "Failed to bind to port 80: Address already in use",
    ProcessId = 1234,
    UserId = 0,
    Hostname = "web-server-01",
    CodeFile = "nginx.c",
    CodeLine = 42,
    CodeFunction = "main",
    ErrNo = "EADDRINUSE",
    MessageId = "1234567890abcdef",
    Sequence = 1001,
    BootId = "a1b2c3d4e5f6g7h8",
    Timestamp = DateTime.UtcNow,
    Metadata = new Dictionary<string, string> { { "Environment", "Production" } }
};

Console.WriteLine(log.ToString());
```

