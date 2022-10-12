![CI](https://github.com/sarmkadan/systemd-service-monitor/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/systemd-service-monitor)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

# systemd Service Monitor

ASP.NET web dashboard for monitoring and controlling systemd services via D-Bus. Blazor Server UI + REST API.

## Requirements

- Linux with systemd
- .NET 10 SDK
- D-Bus access (the process user must be in the `systemd-journal` group or run as root)

## Quick Start

```bash
git clone https://github.com/sarmkadan/systemd-service-monitor
cd systemd-service-monitor
dotnet run
```

Open `http://localhost:5000`. Swagger UI is available at `http://localhost:5000/swagger` in development.

## Features

- List all systemd services with state, sub-state, PID, uptime
- Start / stop / restart services via REST API
- Stream service logs from journald
- CPU and memory metrics per service
- Service dependency graph
- Alert rules engine with escalation policies

## Configuration

`appsettings.json`:

```json
{
  "Systemd": {
    "EnableMonitoring": true,
    "MetricCollectionIntervalMs": 30000,
    "LogRetentionDays": 7,
    "EnableRemoteOperations": true,
    "OperationTimeoutMs": 30000,
    "ConnectionRetryCount": 3
  }
}
```

## Running as a systemd service

```ini
[Unit]
Description=systemd Service Monitor
After=network.target

[Service]
ExecStart=/usr/bin/dotnet /opt/systemd-monitor/systemd-service-monitor.dll
WorkingDirectory=/opt/systemd-monitor
User=monitor
SupplementaryGroups=systemd-journal
Restart=on-failure

[Install]
WantedBy=multi-user.target
```

## API

Base path: `/api`

| Method | Path | Description |
|--------|------|-------------|
| GET | `/services` | List all services |
| GET | `/services/{name}` | Get service details |
| POST | `/services/{name}/start` | Start a service |
| POST | `/services/{name}/stop` | Stop a service |
| POST | `/services/{name}/restart` | Restart a service |
| GET | `/logs/{name}` | Get service logs |
| GET | `/metrics/service/{name}` | CPU/memory metrics |
| GET | `/metrics/system` | System-wide metrics |
| GET | `/health` | Health check |

## License

MIT
