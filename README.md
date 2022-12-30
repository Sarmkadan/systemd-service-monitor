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
