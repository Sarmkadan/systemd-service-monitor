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

## License


This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.