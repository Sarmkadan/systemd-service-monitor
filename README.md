// existing content ...

## ServiceHealthCheck

The `ServiceHealthCheck` class encapsulates the configuration and runtime state of a health check performed on a systemd service. It tracks critical parameters like check type, interval, thresholds, and HTTP configuration, while maintaining live metrics such as the current health status, last response time, and failure/success counters to provide a detailed view of service availability.

### Usage Example

```csharp
using System;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums; // Assuming HealthStatus is in Enums

var healthCheck = new ServiceHealthCheck
{
    Id = Guid.NewGuid(),
    ServiceInfoId = Guid.NewGuid(),
    Name = "HTTP Health Check",
    CheckType = HealthCheckType.Http,
    Description = "Verify web service responsiveness",
    Endpoint = "http://localhost:8080/health",
    HttpMethod = "GET",
    ExpectedHttpStatus = 200,
    TimeoutSeconds = 5,
    IntervalSeconds = 60,
    UnhealthyThreshold = 3,
    HealthyThreshold = 2,
    IsEnabled = true,
    CurrentStatus = HealthStatus.Healthy,
    LastCheckMessage = "Success",
    LastCheckResponseMs = 150,
    LastCheckTime = DateTime.UtcNow,
    ConsecutiveFailures = 0,
    ConsecutiveSuccesses = 10,
    TotalChecks = 50
};

Console.WriteLine($"Check {healthCheck.Name} is currently {healthCheck.CurrentStatus}.");
```

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

## DependencyNode

The `DependencyNode` class represents a node in the service dependency graph, capturing information about a systemd service and its relationships with other services. Each node tracks the service's name, description, current state, and its dependencies/dependents, enabling visualization and analysis of service relationships across the system.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Enums;

// Create a dependency node for a web service
var webServiceNode = new DependencyNode
{
    ServiceName = "nginx.service",
    Description = "High-performance HTTP server and reverse proxy",
    State = ServiceState.Active,
    Dependencies = new List<string> { "network.target", "syslog.socket" },
    Dependents = new List<string> { "website.target", "monitoring-agent.service" },
    IsRootNode = false,
    IsLeafNode = false
};

// Create a dependency node for a database service
var dbServiceNode = new DependencyNode
{
    ServiceName = "postgresql.service",
    Description = "PostgreSQL database server",
    State = ServiceState.Active,
    Dependencies = new List<string> { "postgresql@.service", "network.target" },
    Dependents = new List<string> { "webapp.service", "backup.service" },
    IsRootNode = false,
    IsLeafNode = false
};

// Create a root-level dependency node (no dependencies)
var rootServiceNode = new DependencyNode
{
    ServiceName = "docker.service",
    Description = "Docker container runtime",
    State = ServiceState.Active,
    Dependencies = new List<string>(),
    Dependents = new List<string> { "containerd.service", "docker.socket" },
    IsRootNode = true,
    IsLeafNode = false
};

// Create a leaf dependency node (no dependents)
var leafServiceNode = new DependencyNode
{
    ServiceName = "cron.service",
    Description = "Periodic task scheduler",
    State = ServiceState.Active,
    Dependencies = new List<string> { "time-sync.target" },
    Dependents = new List<string>(),
    IsRootNode = false,
    IsLeafNode = true
};

Console.WriteLine($"Service {webServiceNode.ServiceName} has {webServiceNode.Dependencies.Count} dependencies and {webServiceNode.Dependents.Count} dependents");
Console.WriteLine($"Root node: {rootServiceNode.ServiceName} (IsRootNode={rootServiceNode.IsRootNode})");
Console.WriteLine($"Leaf node: {leafServiceNode.ServiceName} (IsLeafNode={leafServiceNode.IsLeafNode})");
```

## ServiceMetric

The `ServiceMetric` class represents a single metric measurement for a systemd service at a point in time. It captures detailed performance data including CPU, memory, network, and disk metrics, along with statistical aggregations (min, max, average) and contextual tags for filtering and analysis. This class is used to track service health and performance trends over time.

### Usage Example

```csharp
using System;
using System.Collections.Generic;
using SystemdServiceMonitor.Models;

// Create a CPU usage metric for nginx service
var cpuMetric = new ServiceMetric
{
    ServiceInfoId = Guid.NewGuid(),
    UnitName = "nginx.service",
    MetricType = MetricType.CpuUsage,
    Value = 45.5m,
    Unit = "%",
    MinValue = 42.1m,
    MaxValue = 48.7m,
    AvgValue = 45.3m,
    ProcessId = 1234,
    SampleCount = 60,
    Tags = new Dictionary<string, string>
    {
        { "environment", "production" },
        { "tier", "web" },
        { "region", "us-east-1" }
    },
    Timestamp = DateTime.UtcNow,
    ServiceName = "nginx",
    CpuPercentage = 45.5,
    MemoryUsageMb = 256.8,
    NetworkBytesIn = 12345678,
    NetworkBytesOut = 98765432,
    DiskReadBytesPerSec = 5432100,
    DiskWriteBytesPerSec = 3210987,
    DurationSeconds = 60
};

Console.WriteLine($"Service {cpuMetric.UnitName} CPU: {cpuMetric.Value}{cpuMetric.Unit}");

// Create a memory usage metric with statistical aggregation
var memoryMetric = new ServiceMetric
{
    ServiceInfoId = Guid.NewGuid(),
    UnitName = "postgresql.service",
    MetricType = MetricType.MemoryUsage,
    Value = 1024,
    Unit = "MB",
    MinValue = 950.5m,
    MaxValue = 1100.2m,
    AvgValue = 1024.8m,
    ProcessId = 5678,
    SampleCount = 120,
    Tags = new Dictionary<string, string>
    {
        { "database", "postgresql" },
        { "environment", "production" }
    },
    Timestamp = DateTime.UtcNow.AddMinutes(-5)
};

Console.WriteLine($"Service {memoryMetric.UnitName} Memory: {memoryMetric.Value}{memoryMetric.Unit}");
```
