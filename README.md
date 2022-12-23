# systemd Service Monitor

**Real-time web dashboard for monitoring and controlling systemd services on Linux systems**

[![Build](https://github.com/sarmkadan/systemd-service-monitor/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/systemd-service-monitor/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Description


ASP.NET web dashboard for monitoring and controlling systemd services via D-Bus. Blazor Server UI + REST API.

## Requirements


- Linux with systemd
- .NET 10 SDK
- D-Bus access (the process user must be in the `systemd-journal` group or run as root)

## Installation


### Option 1: Install as .NET Global Tool

```bash
dotnet tool install --global SystemdServiceMonitor.Tool
```

### Option 2: Clone and Build

```bash
git clone https://github.com/sarmkadan/systemd-service-monitor.git
cd systemd-service-monitor
dotnet build --configuration Release
```

### Option 3: Use Docker

Build and run the application using Docker:

```bash
# Build the Docker image
docker build -t systemd-service-monitor .

# Run the container with required privileges and volume mounts
# Note: Using --net=host is recommended for systemd monitoring
docker run --privileged --net=host \
  -v /var/run/dbus/system_bus_socket:/var/run/dbus/system_bus_socket \
  -v /var/log/journal:/var/log/journal:ro \
  -v /sys:/sys:ro \
  -v /proc:/proc:ro \
  -v ./logs:/app/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=https://0.0.0.0:5001 \
  --name systemd-monitor \
  -d -p 5001:5001 \
  systemd-service-monitor
```

#### Using Docker Compose

For production deployments, use Docker Compose:

```bash
# Start the service in production mode
docker compose up -d

# View logs
docker compose logs -f

# Stop the service
docker compose down
```

For development with additional services (PostgreSQL, Redis):

```bash
# Start development environment
docker compose -f docker-compose.dev.yml up -d

# Access development services
# PostgreSQL: localhost:5432
# Redis: localhost:6379
```

## Quick Start

```bash
dotnet run --configuration Release
```

Open `http://localhost:5000` in your browser. Swagger UI is available at `http://localhost:5000/swagger` for API documentation.

## Features

- List all systemd services with state, sub-state, PID, uptime
- Start / stop / restart services via REST API
- Stream service logs from journald
- CPU and memory metrics per service
- Service dependency graph
- Alert rules engine with escalation policies

## Configuration

Configuration is done via `appsettings.json`:

```json
{
  "Systemd": {
    "EnableMonitoring": true,
    "MetricCollectionIntervalMs": 30000,
    "LogRetentionDays": 7,
    "EnableRemoteOperations": true,
    "OperationTimeoutMs": 30000,
    "ConnectionRetryCount": 3
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/systemd-service-monitor-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## Performance Benchmarks

Performance benchmarks are implemented using [BenchmarkDotNet](https://benchmarkdotnet.org/).


### Running Benchmarks

```bash
cd tests/systemd-service-monitor.Benchmarks
dotnet run -c Release
```

### Results (ServiceRepository)

| Method | Mean | Error | StdDev | Gen0 | Allocated |
|--------------- |-----------:|----------:|----------:|-------:|----------:|
| GetAllServices | 491.269 us | 9.2551 us | 7.7284 us | 2.9297 | 28440 B |
| GetByUnitName | 2.152 us | 0.0426 us | 0.0700 us | 0.0114 | 96 B |

## API Reference

Base path: `/api`


| Method | Path | Description |
|--------|------|-------------|
| GET | `/services` | List all services |
| GET | `/services/{name}` | Get service details |
| POST | `/services/{name}/start` | Start a service |
| POST | `/services/{name}/stop` | Stop a service |
| POST | `/services/{name}/restart` | Restart a service |
| GET | `/services/{name}/logs` | Get service logs |
| GET | `/services/{name}/metrics` | CPU/memory metrics |
| GET | `/services/{name}/dependencies` | Service dependency graph |
| GET | `/system/metrics` | System-wide metrics |
| GET | `/health` | Health check |

## Usage Examples

### REST API

#### Start a service

```bash
curl -X POST http://localhost:5000/api/services/nginx.service/start \
  -H "Content-Type: application/json"
```

#### Get service metrics

```bash
curl http://localhost:5000/api/services/nginx.service/metrics
```

#### Stream service logs

```bash
curl http://localhost:5000/api/services/nginx.service/logs?limit=50
```

### C# Library

Examples on how to use `systemd-service-monitor` as a library:

- [BasicUsage.cs](examples/dotnet-examples/BasicUsage.cs): Basic retrieval of service status.
- [AdvancedUsage.cs](examples/dotnet-examples/AdvancedUsage.cs): Continuous monitoring and statistics.
- [IntegrationExample.cs](examples/dotnet-examples/IntegrationExample.cs): Dependency injection configuration.

## DateTimeExtensions

The `DateTimeExtensions` class provides a comprehensive set of static extension methods for DateTime operations commonly used in monitoring and logging scenarios. It includes utilities for formatting dates as human-readable strings, converting between DateTime and Unix timestamps, and calculating time ranges and boundaries.


### Usage Example


```csharp
using SystemdServiceMonitor.Extensions;

// Calculate relative time from a past event
var eventTime = DateTime.UtcNow.AddHours(-2);
var relativeTime = eventTime.ToRelativeTime();
Console.WriteLine($"Event occurred: {relativeTime}"); // "2 hours ago"

// Convert DateTime to Unix timestamp
var now = DateTime.UtcNow;
var unixTimestamp = now.ToUnixTimestamp();
Console.WriteLine($"Unix timestamp (seconds): {unixTimestamp}");

// Convert DateTime to Unix timestamp in milliseconds
var unixTimestampMs = now.ToUnixTimestampMilliseconds();
Console.WriteLine($"Unix timestamp (milliseconds): {unixTimestampMs}");

// Convert Unix timestamp back to DateTime
var dateFromTimestamp = DateTimeExtensions.FromUnixTimestamp(unixTimestamp);
Console.WriteLine($"Date from timestamp: {dateFromTimestamp}");

// Convert Unix timestamp in milliseconds back to DateTime
var dateFromTimestampMs = DateTimeExtensions.FromUnixTimestampMilliseconds(unixTimestampMs);
Console.WriteLine($"Date from timestamp (ms): {dateFromTimestampMs}");

// Format DateTime as ISO 8601 string
var iso8601Date = now.ToIso8601String();
Console.WriteLine($"ISO 8601 format: {iso8601Date}");

// Check if a DateTime is within a specific range
var isWithinRange = now.IsWithinRange(
    DateTime.UtcNow.AddHours(-1),
    DateTime.UtcNow.AddHours(1)
);
Console.WriteLine($"Is within range: {isWithinRange}");

// Round DateTime to nearest 15-minute interval
var roundedTime = now.RoundToNearest(TimeSpan.FromMinutes(15));
Console.WriteLine($"Rounded to nearest 15 minutes: {roundedTime}");

// Get start and end of day
var startOfDay = now.StartOfDay();
var endOfDay = now.EndOfDay();
Console.WriteLine($"Start of day: {startOfDay}");
Console.WriteLine($"End of day: {endOfDay}");

// Get start and end of hour
var startOfHour = now.StartOfHour();
Console.WriteLine($"Start of hour: {startOfHour}");
var endOfHour = now.EndOfHour();
Console.WriteLine($"End of hour: {endOfHour}");

// Convert TimeSpan to human-readable string
var uptime = TimeSpan.FromHours(48).Add(TimeSpan.FromMinutes(30));
var humanReadable = uptime.ToHumanReadableString();
Console.WriteLine($"Uptime: {humanReadable}"); // "2d"
```

## AlertRuleExtensions

The `AlertRuleExtensions` class provides static extension methods for `AlertRule` objects that simplify common alert evaluation operations. It includes utilities for severity comparison, tag management, cooldown calculations, and rule evaluation state checks.

### Usage Example

```csharp
using SystemdServiceMonitor.Models;

// Create an alert rule
var alertRule = new AlertRule
{
    Name = "High CPU Usage",
    Severity = AlertSeverity.Critical,
    ServicePattern = "nginx.service",
    Condition = "cpu_usage > 90",
    Threshold = 90,
    Tags = new List<string> { "performance", "nginx", "production" },
    CooldownMinutes = 5,
    IsEnabled = true,
    ConsecutiveEvaluationsRequired = 3
};

// Check if rule severity meets minimum requirements
bool isCriticalOrHigher = alertRule.IsSeverityAtLeast(AlertSeverity.Critical);
bool isGreaterThanWarning = alertRule.IsSeverityGreaterThan(AlertSeverity.Warning);

// Check for specific tags
bool hasNginxTag = alertRule.HasAnyTag("nginx");
bool hasAllTags = alertRule.HasAllTags("performance", "production");

// Get cooldown period in seconds
int cooldownSeconds = alertRule.GetCooldownSeconds();

// Check if rule is active and ready for evaluation
bool isActive = alertRule.IsActive();

// Get a formatted summary of the rule
string summary = alertRule.GetSummary();

// Check if rule requires consecutive evaluations
bool requiresConsecutive = alertRule.RequiresConsecutiveEvaluations();
int requiredEvaluations = alertRule.GetRequiredEvaluationCount();
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
