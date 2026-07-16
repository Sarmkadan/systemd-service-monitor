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

## RestartPolicyConfig

The `RestartPolicyConfig` class defines detailed restart behavior configuration for systemd services. It controls when and how services are automatically restarted after failures, including timing, limits, and custom commands to execute before and after restarts. This configuration is used to implement sophisticated restart strategies beyond simple systemd restart policies.

### Usage Example

```csharp
var restartConfig = new RestartPolicyConfig
{
    Id = Guid.NewGuid(),
    ServiceInfoId = Guid.NewGuid(),
    PolicyType = RestartPolicy.OnFailure,
    RestartDelaySec = 5,
    MaxRestarts = 3,
    RestartWindowSec = 120,
    StartLimitIntervalSec = 10,
    StartLimitBurst = 5,
    TimeoutStartSec = 120,
    TimeoutStopSec = 60,
    RestartStrategy = RestartStrategy.Graceful,
    IsEnabled = true,
    PreRestartCommand = "systemctl stop dependent-service.service",
    PostRestartCommand = "systemctl start backup-service.service",
    NotifyOnRestart = true,
    TrackRestartHistory = true
};

Console.WriteLine($"Configured restart policy: {restartConfig.PolicyType} with {restartConfig.MaxRestarts} max restarts");
```

## SystemResource

The `SystemResource` class captures a snapshot of overall host metrics such as memory, CPU, disk, network, and process counts. It is useful for monitoring system health and correlating resource usage with service performance.

### Usage Example

```csharp
using System;
using SystemdServiceMonitor.Models;

var resource = new SystemResource
{
    Id = Guid.NewGuid(),
    TotalMemoryMb = 16384,
    AvailableMemoryMb = 8192,
    UsedMemoryMb = 8192,
    CachedMemoryMb = 1024,
    CpuCoreCount = 8,
    CpuLoad1Min = 0.75m,
    CpuLoad5Min = 0.60m,
    CpuLoad15Min = 0.55m,
    CpuUsagePercent = 45.3m,
    TotalDiskGb = 500,
    UsedDiskGb = 200,
    AvailableDiskGb = 300,
    DiskIopsPerSecond = 1500,
    NetworkBytesIn = 123456789,
    NetworkBytesOut = 987654321,
    RunningProcesses = 124,
    SystemUptimeSeconds = 86400,
    LoadAveragePercent = 30.5m,
    MemoryUsagePercent = 50.0m
};

Console.WriteLine(resource);
```
