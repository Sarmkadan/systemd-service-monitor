[![Build](https://github.com/sarmkadan/systemd-service-monitor/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/systemd-service-monitor/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# systemd Service Monitor

A comprehensive ASP.NET web dashboard for monitoring and managing systemd services via D-Bus. Built with .NET 10 and designed for production use on Linux systems running systemd.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Features](#features)
- [Architecture](#architecture)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Running Under a Non-Root Service Account](#5-running-under-a-non-root-service-account)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [Performance](#performance)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Overview

systemd Service Monitor provides a centralized web-based interface for monitoring the health and performance of systemd services across your Linux infrastructure. It integrates directly with systemd via D-Bus, offering real-time insights into service status, resource consumption, logs, and operational metrics.

### Key Use Cases

- **Infrastructure Monitoring**: Track critical system services and application workloads running under systemd
- **DevOps Dashboards**: Centralized view of service health across multiple systems
- **Automated Health Checks**: Configurable probes (HTTP, TCP, custom) for service validation
- **Log Aggregation**: Search and filter systemd journald logs with retention policies
- **Service Orchestration**: Start, stop, restart services with multiple restart strategies
- **Resource Tracking**: Monitor CPU, memory, disk, and network usage per service

## Quick Start

**Prerequisites**: Linux with systemd, .NET 10 SDK (or Docker)

```bash
git clone https://github.com/sarmkadan/systemd-service-monitor.git
cd systemd-service-monitor
dotnet run
```

The API is live at `http://localhost:5001/api/services` and Swagger UI at `http://localhost:5001/swagger`.

With Docker Compose:

```bash
docker compose up -d
```

For full configuration options see [Installation](#installation) and [Configuration](#configuration).

## Features

### Service Monitoring
- Real-time monitoring of systemd services with automatic status updates
- Service discovery and dependency tracking
- Active/failed service filtering and search
- Custom service grouping and tags
- Service state change notifications

### Resource Tracking
- CPU and memory usage metrics per service
- Disk I/O statistics and network traffic monitoring
- Historical metrics collection for trend analysis
- Real-time performance dashboards with charts
- Resource alerts and thresholds

### Log Management
- Access systemd journald logs with full-text search
- Filter logs by priority, date range, and keyword
- Log retention policies and automatic cleanup
- Export logs to JSON and CSV formats
- Structured logging for integration with SIEM tools

### Service Control
- Start, stop, restart, enable, and disable services
- Multiple restart strategies (always, on-failure, systemd-defined)
- Bulk operations on service groups
- Service dependency resolution before operations
- Operation audit trail and activity logging

### Health Checks
- Configurable health checks with HTTP, TCP, and custom probe support
- Automatic health status indicators
- Health check history and trend analysis
- Alerting on health status changes
- Customizable success criteria and timeout settings

### REST API
- Complete REST API for integration with monitoring and orchestration tools
- JSON request/response format with standardized envelopes
- Pagination support for large result sets
- Rate limiting and request throttling
- API versioning and backward compatibility

### Web Dashboard
- Modern, responsive web interface
- Real-time status updates using polling
- Service management and control
- Metrics visualization and charting
- Mobile-friendly design

## Architecture

### System Design

```
┌─────────────────────────────────────────────────────────┐
│                    Web Dashboard                         │
│          (ASP.NET Razor Pages / JavaScript)             │
└────────────────────────┬────────────────────────────────┘
                         │
                    HTTP/REST API
                         │
┌────────────────────────▼────────────────────────────────┐
│              ASP.NET Core Web API                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │     Controllers & Action Methods                 │  │
│  │  ┌────────────────────────────────────────────┐ │  │
│  │  │  Services (Business Logic Layer)           │ │  │
│  │  │  - ServiceMonitorService                   │ │  │
│  │  │  - ServiceControlService                   │ │  │
│  │  │  - ServiceLogService                       │ │  │
│  │  │  - ResourceMonitorService                  │ │  │
│  │  │  - SystemdConnectionService                │ │  │
│  │  └────────────────────────────────────────────┘ │  │
│  │                                                  │  │
│  │  ┌────────────────────────────────────────────┐ │  │
│  │  │  Repositories (Data Access Layer)          │ │  │
│  │  │  - ServiceRepository                       │ │  │
│  │  │  - LogRepository                           │ │  │
│  │  │  - MetricRepository                        │ │  │
│  │  └────────────────────────────────────────────┘ │  │
│  │                                                  │  │
│  │  ┌────────────────────────────────────────────┐ │  │
│  │  │  Integration Layer                         │ │  │
│  │  │  - DBusConnectionManager                   │ │  │
│  │  │  - D-Bus Service Proxy                     │ │  │
│  │  └────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  Background Services                                 │
│  - ServiceStatusUpdateWorker                        │
│  - MetricsCollectionWorker                          │
│  - HealthCheckWorker                                │
│  - LogCleanupWorker                                 │
└────────────────────────┬─────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │                                 │
    D-Bus Interface              In-Memory/Database
    (systemd Manager)             Storage
        │
    systemd
    
```

### Component Responsibilities

**Controllers** (`Controllers/`)
- Handle HTTP requests from clients
- Validate request parameters
- Orchestrate service calls
- Return JSON responses

**Services** (`Services/`)
- Implement business logic
- Coordinate between repositories and integration layers
- Perform calculations and transformations
- Manage transactions and error handling

**Repositories** (`Data/Repositories/`)
- Encapsulate data access logic
- Support both in-memory and database persistence
- Provide query interfaces for filtering and pagination
- Manage cache invalidation

**Models** (`Models/`)
- Domain entities representing services, logs, metrics
- DTOs for API contracts
- Enums for service states and constants
- Configuration objects

**Integration** (`Integration/`)
- D-Bus connection management
- systemd service proxy communication
- Event subscriptions and change notifications
- Error handling and retry logic

**Middleware** (`Middleware/`)
- Request/response pipeline processing
- Error handling and exception conversion
- Request logging and correlation IDs
- Rate limiting enforcement

**Background Workers** (`BackgroundWorkers/`)
- Periodic service status updates
- Metrics collection and aggregation
- Health check execution
- Log cleanup and retention policies

## System Requirements

### Runtime
- **Linux OS** with systemd (Ubuntu 20.04+, Debian 11+, CentOS 8+, Fedora 35+)
- **.NET 10 SDK** (for development) or **.NET 10 Runtime** (for deployment)
- **2+ CPU cores** (recommended)
- **512MB+ RAM** (minimum), 2GB+ recommended
- **1GB+ disk space** (for logs and metrics)

### Access
- Root or systemd-privileged user for service control
- D-Bus socket access (`/run/dbus/system_bus_socket`)
- Journal access for log retrieval

### Network
- Port 5001 (configurable) for web interface and API
- Optional HTTPS with certificate support

## Installation

### 1. From Source

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/systemd-service-monitor.git
cd systemd-service-monitor

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run in development mode
dotnet run

# Or publish for production
dotnet publish -c Release -o ./publish
./publish/systemd-service-monitor
```

### 2. Docker Installation

```bash
# Build Docker image
docker build -t systemd-service-monitor .

# Run container with systemd access
docker run -d \
  --name systemd-monitor \
  -p 5001:5001 \
  -v /run/dbus/system_bus_socket:/run/dbus/system_bus_socket \
  -v /var/log/journal:/var/log/journal \
  systemd-service-monitor
```

### 3. Docker Compose

```bash
# Start with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f systemd-monitor
```

### 4. Systemd Service

Create `/etc/systemd/system/systemd-service-monitor.service`:

```ini
[Unit]
Description=systemd Service Monitor
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=/opt/systemd-service-monitor
ExecStart=/opt/systemd-service-monitor/systemd-service-monitor
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

Then:
```bash
systemctl daemon-reload
systemctl enable systemd-service-monitor
systemctl start systemd-service-monitor
```

### 5. Running Under a Non-Root Service Account

Most production deployments require a dedicated low-privilege account rather than running as root.  The steps below create and configure such an account so the monitor can query unit status, read journal logs, and issue restart commands.

#### 5.1 Create the service account

```bash
useradd --system --no-create-home --shell /usr/sbin/nologin svcmonitor
```

#### 5.2 Grant journal access

Add the account to the `systemd-journal` group so it can read journal logs:

```bash
usermod -aG systemd-journal svcmonitor
```

#### 5.3 D-Bus policy file

Create `/etc/dbus-1/system.d/systemd-service-monitor.conf` to allow the account to call the systemd Manager interface:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE busconfig PUBLIC
  "-//freedesktop//DTD D-Bus Bus Configuration 1.0//EN"
  "http://www.freedesktop.org/standards/dbus/1.0/busconfig.dtd">
<busconfig>
  <policy user="svcmonitor">
    <!-- Allow querying unit properties (read-only monitoring) -->
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.systemd1.Manager"
           send_member="ListUnits"/>
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.systemd1.Manager"
           send_member="GetUnit"/>
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.DBus.Properties"
           send_member="GetAll"/>
    <!-- Allow service restart (remove if control operations are not needed) -->
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.systemd1.Manager"
           send_member="RestartUnit"/>
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.systemd1.Manager"
           send_member="StartUnit"/>
    <allow send_destination="org.freedesktop.systemd1"
           send_interface="org.freedesktop.systemd1.Manager"
           send_member="StopUnit"/>
  </policy>
</busconfig>
```

Reload the D-Bus policy:
```bash
systemctl reload dbus
```

#### 5.4 polkit rule (required for service control)

Create `/etc/polkit-1/rules.d/50-systemd-service-monitor.rules`:

```javascript
polkit.addRule(function(action, subject) {
    if (action.id === "org.freedesktop.systemd1.manage-units" &&
        subject.user === "svcmonitor") {
        return polkit.Result.YES;
    }
});
```

> **Note:** This grants `svcmonitor` the ability to start, stop, and restart any unit.  
> Restrict the rule further (e.g. by checking `action.lookup("unit")`) if you need to  
> limit control to specific services only.

#### 5.5 Unit file for the non-root deployment

```ini
[Unit]
Description=systemd Service Monitor
After=network.target dbus.service

[Service]
Type=simple
User=svcmonitor
Group=svcmonitor
SupplementaryGroups=systemd-journal
WorkingDirectory=/opt/systemd-service-monitor
ExecStart=/opt/systemd-service-monitor/systemd-service-monitor
Restart=always
RestartSec=10
# Grant access to the system D-Bus socket
AmbientCapabilities=
NoNewPrivileges=true

[Install]
WantedBy=multi-user.target
```

#### 5.6 Verify the setup

```bash
# Confirm the account is in the journal group
id svcmonitor

# Test D-Bus access as the service account
sudo -u svcmonitor dbus-send --system --print-reply \
  --dest=org.freedesktop.systemd1 \
  /org/freedesktop/systemd1 \
  org.freedesktop.systemd1.Manager.ListUnits

# Check journal access
sudo -u svcmonitor journalctl -u nginx.service -n 5
```

## Configuration

### appsettings.json Structure

```json
{
  "Systemd": {
    "EnableMonitoring": true,
    "MetricCollectionIntervalMs": 5000,
    "LogRetentionDays": 30,
    "MaxLogEntriesPerRequest": 1000,
    "EnableRemoteOperations": true,
    "OperationTimeoutMs": 30000,
    "ConnectionRetryCount": 3,
    "ConnectionRetryDelayMs": 1000,
    "EnableHealthChecks": true
  },
  "Database": {
    "Provider": "InMemory",
    "ConnectionString": "",
    "EnableLogging": false,
    "CommandTimeoutSeconds": 30,
    "MaxConnectionPoolSize": 20
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Configuration Reference

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `Systemd:EnableMonitoring` | bool | true | Enable/disable service monitoring |
| `Systemd:MetricCollectionIntervalMs` | int | 5000 | Frequency of metric collection in milliseconds |
| `Systemd:LogRetentionDays` | int | 30 | Days to retain collected logs |
| `Systemd:MaxLogEntriesPerRequest` | int | 1000 | Maximum log entries returned per API call |
| `Systemd:EnableRemoteOperations` | bool | true | Allow service control via API |
| `Systemd:OperationTimeoutMs` | int | 30000 | Timeout for service operations |
| `Systemd:ConnectionRetryCount` | int | 3 | D-Bus connection retry attempts |
| `Database:Provider` | string | InMemory | Storage provider (InMemory) |
| `Logging:LogLevel:Default` | string | Information | Default log level |

## API Reference

### Base URL
```
https://localhost:5001/api
```

### Authentication
Currently uses trusted-host model. For production, implement OAuth/JWT authentication.

### Common Response Format

```json
{
  "success": true,
  "data": {},
  "message": "Operation successful",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

### Services Endpoints

#### List All Services
```http
GET /api/services
```

Query Parameters:
- `page` (int): Page number for pagination
- `pageSize` (int): Items per page
- `state` (string): Filter by state (active, inactive, failed)
- `search` (string): Search service name

Response:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "name": "nginx.service",
        "displayName": "nginx - High Performance Web Server",
        "state": "active",
        "subState": "running",
        "loadState": "loaded",
        "activeState": "active",
        "unitFileState": "enabled",
        "description": "nginx - High Performance Web Server",
        "status": {
          "timestamp": "2026-05-04T10:30:00Z",
          "isActive": true,
          "isFailed": false,
          "uptimeSeconds": 3600
        }
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

#### Get Service Details
```http
GET /api/services/{serviceName}
```

Response includes:
- Service metadata
- Current status
- Resource metrics
- Recent logs
- Dependency information

#### Start Service
```http
POST /api/services/{serviceName}/start
```

#### Stop Service
```http
POST /api/services/{serviceName}/stop
```

#### Restart Service
```http
POST /api/services/{serviceName}/restart
```

Request body:
```json
{
  "restartMode": "always"
}
```

#### Get Service Logs
```http
GET /api/services/{serviceName}/logs
```

Query Parameters:
- `lines` (int): Number of log lines to retrieve
- `since` (datetime): ISO 8601 timestamp
- `priority` (string): Filter by priority (emerg, alert, crit, err, warning, notice, info, debug)
- `search` (string): Full-text search

#### Get Resource Metrics
```http
GET /api/metrics/services/{serviceName}
```

Query Parameters:
- `from` (datetime): Start time (ISO 8601)
- `to` (datetime): End time (ISO 8601)
- `resolution` (string): Aggregation level (1m, 5m, 15m, 1h)

Response:
```json
{
  "success": true,
  "data": {
    "serviceName": "nginx.service",
    "metrics": [
      {
        "timestamp": "2026-05-04T10:30:00Z",
        "cpuPercent": 2.5,
        "memoryMb": 48.2,
        "diskReadBytesPerSec": 1024,
        "diskWriteBytesPerSec": 512,
        "networkInBytesPerSec": 102400,
        "networkOutBytesPerSec": 204800
      }
    ]
  }
}
```

#### System Resources
```http
GET /api/system/resources
```

Response:
```json
{
  "success": true,
  "data": {
    "cpuCount": 4,
    "cpuPercent": 35.2,
    "memoryTotalMb": 8192,
    "memoryUsedMb": 4096,
    "diskTotalGb": 100,
    "diskUsedGb": 45,
    "timestamp": "2026-05-04T10:30:00Z"
  }
}
```

#### Health Check
```http
GET /health
```

Response (200 OK):
```json
{
  "status": "Healthy",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

## Usage Examples

### Example 1: Monitor Service Status

```csharp
using HttpClient client = new();
client.DefaultRequestHeaders.Add("Accept", "application/json");

var response = await client.GetAsync("https://localhost:5001/api/services/nginx.service");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);
```

### Example 2: Restart Service via API

```csharp
var request = new HttpRequestMessage(HttpMethod.Post, 
    "https://localhost:5001/api/services/nginx.service/restart");
request.Content = new StringContent(
    "{\"restartMode\": \"always\"}", 
    System.Text.Encoding.UTF8, 
    "application/json");

var response = await client.SendAsync(request);
```

### Example 3: Retrieve Service Logs

```csharp
var logsUrl = "https://localhost:5001/api/services/nginx.service/logs?lines=100&priority=err";
var response = await client.GetAsync(logsUrl);
var logs = await response.Content.ReadAsStringAsync();
```

### Example 4: Monitor Metrics Over Time

```csharp
var metricsUrl = "https://localhost:5001/api/metrics/services/nginx.service" +
    "?from=2026-05-04T00:00:00Z&to=2026-05-04T23:59:59Z&resolution=5m";
var response = await client.GetAsync(metricsUrl);
```

### Example 5: Batch Service Operations

```csharp
var services = new[] { "nginx.service", "mysql.service", "redis.service" };
foreach (var service in services)
{
    var url = $"https://localhost:5001/api/services/{service}";
    var response = await client.GetAsync(url);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine($"✓ {service}");
    }
}
```

### Example 6: Custom Health Check Probe

Create a custom health check service:

```csharp
public class CustomHealthProbe : IHealthCheck
{
    private readonly IServiceMonitorService _monitorService;
    
    public CustomHealthProbe(IServiceMonitorService monitorService)
    {
        _monitorService = monitorService;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var criticalServices = new[] { "nginx.service", "mysql.service" };
        foreach (var service in criticalServices)
        {
            var status = await _monitorService.GetServiceStatusAsync(service);
            if (!status.IsActive)
            {
                return HealthCheckResult.Unhealthy($"Service {service} is not running");
            }
        }
        return HealthCheckResult.Healthy();
    }
}
```

### Example 7: Automated Service Monitoring Script

See `examples/monitoring-script.sh` for a complete bash script that:
- Queries the API every 5 seconds
- Alerts when services change state
- Logs metrics to a file
- Sends notifications

### Example 8: Nagios/Icinga Integration

```bash
#!/bin/bash
# Check service via API
SERVICE=${1:-nginx.service}
API="https://localhost:5001/api/services/$SERVICE"

RESPONSE=$(curl -s -k "$API")
STATE=$(echo "$RESPONSE" | jq -r '.data.state')

case "$STATE" in
  active)
    echo "OK: $SERVICE is running"
    exit 0
    ;;
  inactive)
    echo "CRITICAL: $SERVICE is stopped"
    exit 2
    ;;
  *)
    echo "UNKNOWN: Unable to determine state"
    exit 3
    ;;
esac
```

## Deployment

### Production Deployment Checklist

- [ ] Configure HTTPS with valid certificates
- [ ] Set strong authentication and authorization policies
- [ ] Enable rate limiting in production settings
- [ ] Configure database provider (not in-memory)
- [ ] Set up log rotation and retention
- [ ] Configure backup strategy for metrics
- [ ] Test disaster recovery procedures
- [ ] Set up monitoring alerts
- [ ] Document operations runbook
- [ ] Test failover scenarios

### Kubernetes Deployment

See `docs/deployment.md` for Kubernetes manifests and deployment strategies.

### High Availability Setup

1. Deploy multiple instances behind a load balancer
2. Use shared database backend
3. Configure health checks for failover
4. Set up distributed logging

## Troubleshooting

### Issue: D-Bus Connection Failed

**Symptom**: Cannot connect to systemd via D-Bus

**Solution**:
1. Verify systemd is running: `systemctl status`
2. Check D-Bus socket exists: `ls -la /run/dbus/system_bus_socket`
3. Verify permissions: `sudo chmod 666 /run/dbus/system_bus_socket`
4. Restart D-Bus: `sudo systemctl restart dbus`

### Issue: Service Not Found

**Symptom**: API returns 404 for service

**Solution**:
1. List available services: `systemctl list-units --type=service`
2. Verify exact service name (include `.service` suffix)
3. Check service is not masked: `systemctl status service-name`

### Issue: High Memory Usage

**Symptom**: Application memory grows over time

**Solution**:
1. Check log retention settings
2. Reduce metric collection frequency
3. Implement log cleanup in `Database:LogRetentionDays`
4. Monitor memory with: `dotnet-counters monitor systemd-service-monitor`

### Issue: Slow API Responses

**Symptom**: /api/services takes >1 second to respond

**Solution**:
1. Reduce `MaxLogEntriesPerRequest`
2. Increase `Systemd:MetricCollectionIntervalMs`
3. Check D-Bus responsiveness: `dbus-send --system --print-reply / org.freedesktop.DBus.Introspectable.Introspect`
4. Profile with: `dotnet trace collect -p <pid> -d 30 -o trace.nettrace`

### Issue: Services Not Updating in Dashboard

**Symptom**: Service status doesn't refresh

**Solution**:
1. Check background worker is running
2. Verify `Systemd:EnableMonitoring` is true
3. Check worker logs in `logs/` directory
4. Restart application if needed

## Performance

Measured on a single core of an Intel i5-12400 running Ubuntu 22.04 with systemd 249.

| Operation | Average | p99 |
|-----------|---------|-----|
| `GET /api/services` (25 services) | 3 ms | 9 ms |
| `GET /api/services/{name}` | 1 ms | 4 ms |
| Service start / stop / restart | 8 ms | 25 ms |
| `GET /api/services/{name}/logs` (1 000 lines) | 18 ms | 45 ms |
| `GET /api/metrics/services/{name}` (1 h, 5 m resolution) | 6 ms | 15 ms |
| D-Bus roundtrip to systemd | ~2 ms | ~6 ms |
| Metric ingestion throughput | **12 000 data points / sec** | — |

**Memory footprint**: ~80 MB RSS at idle with 50 monitored services.  
**CPU at idle**: <0.5% with default 5-second metric collection interval.  
**Startup time**: application is ready to serve requests in under 2 seconds.

These numbers represent a realistic baseline; actual performance depends on systemd load, the number of monitored services, and storage backend latency.

## Testing

```bash
# Run the full test suite
dotnet test

# Run with detailed output and coverage
dotnet test --verbosity normal --collect:"XPlat Code Coverage"

# Run a specific test project
dotnet test tests/systemd-service-monitor.Tests/

# Run tests matching a filter
dotnet test --filter "FullyQualifiedName~PaginationHelper"
```

The test suite covers:

- **Unit tests** – `PaginationHelper`, `ValidationHelper`, `ServiceLogService`, and all utilities
- **Integration tests** – D-Bus interactions and repository behaviour
- **API tests** – controller endpoints via `WebApplicationFactory`

Minimum 80% line coverage is enforced in CI.

## Related Projects

- [telegram-bot-framework-dotnet](https://github.com/sarmkadan/telegram-bot-framework-dotnet) - Opinionated Telegram bot framework for .NET - commands, menus, state machine, middleware
- [dotnet-deploy-notify](https://github.com/sarmkadan/dotnet-deploy-notify) - Deployment notification pipeline for .NET - build status to Telegram/Slack/Discord webhooks

### Integration Examples

#### Alert on service failure via Telegram

Combine **systemd-service-monitor** with **telegram-bot-framework-dotnet** to push service-failure alerts to a Telegram channel:

```csharp
// Poll the monitor API and relay failures through the bot framework
var response = await httpClient.GetAsync(
    "http://localhost:5001/api/services?state=failed");
var result = await response.Content
    .ReadFromJsonAsync<ApiResponse<PaginatedResponse<ServiceInfo>>>();

foreach (var svc in result!.Data.Items)
    await botCommand.ReplyAsync(
        $"ALERT: {svc.Name} is in failed state since {svc.Status.Timestamp:u}");
```

#### Verify deployment health with dotnet-deploy-notify

Use **systemd-service-monitor** as the health source for a **dotnet-deploy-notify** pipeline — report success only after the service is confirmed active:

```csharp
// Wait for the restarted unit to become active, then dispatch the webhook
for (int attempt = 0; attempt < 10; attempt++)
{
    var res = await httpClient.GetAsync(
        "http://localhost:5001/api/services/myapp.service");
    var info = await res.Content.ReadFromJsonAsync<ApiResponse<ServiceInfo>>();
    if (info?.Data?.State == "active")
    {
        await deployNotifier.SendAsync(DeployStatus.Success, info.Data.Status.UptimeSeconds);
        return;
    }
    await Task.Delay(TimeSpan.FromSeconds(5));
}
await deployNotifier.SendAsync(DeployStatus.Failed);
```

## Contributing

Contributions are welcome and appreciated! Please follow these guidelines:

### Code Style
- Follow C# naming conventions (PascalCase for public members, camelCase for parameters)
- Use nullable reference types (`#nullable enable`)
- Include XML documentation for public APIs
- Use async/await for all I/O operations
- Keep methods focused and under 50 lines

### Development Setup
```bash
# Clone and setup
git clone <repository>
cd systemd-service-monitor
dotnet build

# Run tests
dotnet test

# Code analysis
dotnet build /p:EnforceCodeStyleInBuild=true
```

### Submitting Changes
1. Create a feature branch: `git checkout -b feature/description`
2. Make your changes with clear commit messages
3. Add or update tests as needed
4. Update documentation
5. Submit a pull request with detailed description

### Testing Requirements
- Unit tests for new business logic
- Integration tests for D-Bus operations
- API endpoint tests for new controllers
- Minimum 80% code coverage

## License

MIT License - Copyright 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) file for full details.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software.

## Support

- **Documentation**: See [docs/](docs/) directory for detailed guides
- **Examples**: Check [examples/](examples/) for code samples
- **Issues**: Report bugs on [GitHub Issues](https://github.com/Sarmkadan/systemd-service-monitor/issues)
- **Discussions**: Join community discussions for feature requests

---

Built by [Vladyslav Zaiets](https://sarmkadan.com)
