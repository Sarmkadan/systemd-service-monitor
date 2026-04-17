# Getting Started Guide

## Prerequisites

Before installing systemd-service-monitor, ensure your system meets the following requirements:

### System Requirements
- **OS**: Linux with systemd (Ubuntu 20.04+, Debian 11+, CentOS 8+)
- **.NET Runtime**: .NET 10.0 or later
- **Memory**: Minimum 512MB, recommended 2GB
- **Disk**: 1GB available space
- **User**: Root or user with systemd privileges

### Verify Prerequisites

```bash
# Check systemd
systemctl --version

# Check .NET installation
dotnet --version

# Check D-Bus availability
dbus-send --system --print-reply / org.freedesktop.DBus.Introspectable.Introspect | head -20

# List available services
systemctl list-units --type=service --all
```

## Installation Methods

### Method 1: Build from Source

1. **Clone the repository**
```bash
git clone https://github.com/Sarmkadan/systemd-service-monitor.git
cd systemd-service-monitor
```

2. **Restore NuGet packages**
```bash
dotnet restore
```

3. **Build the project**
```bash
dotnet build -c Release
```

4. **Run the application**
```bash
# Development mode (HTTP, Swagger UI)
dotnet run

# Production mode
dotnet publish -c Release -o ./publish
cd publish
./systemd-service-monitor
```

5. **Access the dashboard**
- Navigate to `https://localhost:5001` in your web browser
- Swagger API docs available at `https://localhost:5001/swagger`

### Method 2: Docker Container

1. **Build Docker image**
```bash
docker build -t systemd-monitor:latest .
```

2. **Run container with D-Bus access**
```bash
docker run -d \
  --name systemd-monitor \
  --privileged \
  -p 5001:5001 \
  -v /run/dbus/system_bus_socket:/run/dbus/system_bus_socket \
  -v /var/log/journal:/var/log/journal:ro \
  -v /sys:/sys:ro \
  -v /proc:/proc:ro \
  systemd-monitor:latest
```

3. **View logs**
```bash
docker logs -f systemd-monitor
```

4. **Stop container**
```bash
docker stop systemd-monitor
docker rm systemd-monitor
```

### Method 3: Docker Compose

1. **Review compose file**
```bash
cat docker-compose.yml
```

2. **Start services**
```bash
docker-compose up -d
```

3. **Monitor startup**
```bash
docker-compose logs -f systemd-monitor
```

4. **Shutdown**
```bash
docker-compose down
```

### Method 4: Systemd Service

1. **Prepare installation directory**
```bash
sudo mkdir -p /opt/systemd-service-monitor
cd systemd-service-monitor
dotnet publish -c Release -o /opt/systemd-service-monitor
```

2. **Create systemd unit file**
```bash
sudo tee /etc/systemd/system/systemd-service-monitor.service > /dev/null <<EOF
[Unit]
Description=systemd Service Monitor
After=network.target dbus.service
Wants=dbus.service

[Service]
Type=simple
User=root
Group=root
WorkingDirectory=/opt/systemd-service-monitor
ExecStart=/opt/systemd-service-monitor/systemd-service-monitor
Restart=always
RestartSec=10s
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF
```

3. **Enable and start service**
```bash
sudo systemctl daemon-reload
sudo systemctl enable systemd-service-monitor
sudo systemctl start systemd-service-monitor
sudo systemctl status systemd-service-monitor
```

4. **View logs**
```bash
sudo journalctl -u systemd-service-monitor -f
```

## Initial Configuration

### 1. Review Configuration File

```bash
cat appsettings.json
```

Default settings are suitable for development. For production, adjust:

### 2. Customize appsettings.json

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
    "CommandTimeoutSeconds": 30
  }
}
```

### 3. Set Environment Variables (Optional)

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=https://0.0.0.0:5001
export Systemd__EnableMonitoring=true
export Systemd__MetricCollectionIntervalMs=5000
```

## First Steps

### 1. Verify Installation

```bash
# Check application is running
curl -k https://localhost:5001/health

# Should return:
# {"status":"Healthy","timestamp":"2026-05-04T..."}
```

### 2. Access Web Dashboard

Open your browser:
- **Dashboard**: https://localhost:5001
- **API Docs**: https://localhost:5001/swagger

### 3. Test API Endpoints

```bash
# List all services
curl -k https://localhost:5001/api/services | jq .

# Get specific service
curl -k https://localhost:5001/api/services/nginx.service | jq .

# Get system resources
curl -k https://localhost:5001/api/system/resources | jq .
```

### 4. Common First Tasks

**Monitor a specific service**
```bash
curl -k "https://localhost:5001/api/services/systemd-journald.service" | jq '.data'
```

**Get service logs**
```bash
curl -k "https://localhost:5001/api/services/nginx.service/logs?lines=50" | jq '.data.logs'
```

**Retrieve metrics**
```bash
curl -k "https://localhost:5001/api/metrics/services/nginx.service" | jq '.data.metrics'
```

## Development Workflow

### Setting Up Development Environment

1. **Install Visual Studio Code extensions** (optional)
   - C# Dev Kit
   - REST Client
   - Thunder Client

2. **Load project in IDE**
```bash
code .
```

3. **Enable debugging**
   - Press F5 to start with debugging
   - Set breakpoints in code
   - Use Debug Console for inspection

### Running in Development Mode

```bash
# With file watching and hot reload
dotnet watch run

# Standard run
dotnet run

# With environment override
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

### Testing Configuration Changes

```bash
# Create appsettings.Development.json for dev overrides
cat > appsettings.Development.json <<EOF
{
  "Systemd": {
    "MetricCollectionIntervalMs": 10000
  }
}
EOF

# Run with development settings
dotnet run
```

## Verification Checklist

After installation, verify:

- [ ] Application starts without errors
- [ ] Health check endpoint responds (GET /health)
- [ ] Services are listed via API (GET /api/services)
- [ ] Dashboard loads in browser
- [ ] Service details can be retrieved
- [ ] Logs can be accessed
- [ ] System resources are displayed
- [ ] No D-Bus connection errors in logs

## Troubleshooting Installation

### Error: "Unable to load shared library 'libdbus'"

**Solution**:
```bash
# Ubuntu/Debian
sudo apt-get install libdbus-1-dev

# CentOS/RHEL
sudo yum install dbus-devel
```

### Error: "Permission denied" when accessing D-Bus

**Solution**:
```bash
# Run with elevated privileges
sudo dotnet run

# Or update D-Bus socket permissions
sudo chmod 666 /run/dbus/system_bus_socket
```

### Error: ".NET 10 runtime not found"

**Solution**:
```bash
# Check installed versions
dotnet --info

# Install .NET 10
# Visit https://dotnet.microsoft.com/download/dotnet/10.0
```

### Error: "Port 5001 already in use"

**Solution**:
```bash
# Change port in appsettings.json
# Or set environment variable
export ASPNETCORE_URLS=https://0.0.0.0:5002
dotnet run
```

## Next Steps

1. **Read [Architecture Guide](architecture.md)** for system design details
2. **Review [API Reference](api-reference.md)** for available endpoints
3. **Check [Deployment Guide](deployment.md)** for production setup
4. **Explore [examples/](../examples/)** for code samples
5. **Run built-in tests**: `dotnet test`

## Quick Reference

| Task | Command |
|------|---------|
| Build | `dotnet build -c Release` |
| Run | `dotnet run` |
| Publish | `dotnet publish -c Release -o ./publish` |
| Test | `dotnet test` |
| Clean | `dotnet clean` |
| Watch (auto-rebuild) | `dotnet watch run` |
| View logs | `tail -f logs/systemd-monitor-*.txt` |
| Stop service | `systemctl stop systemd-service-monitor` |
| View service logs | `journalctl -u systemd-service-monitor -f` |

## Support and Help

- Check [FAQ](faq.md) for common questions
- Review example scripts in `examples/` directory
- Read API documentation at `/swagger` endpoint
- Check application logs in `logs/` directory
