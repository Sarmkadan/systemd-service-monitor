# Docker Guide for systemd-service-monitor

This guide provides instructions for running the systemd-service-monitor application using Docker and Docker Compose.

## Quick Start with Docker

To get started quickly with Docker, you can run the systemd-service-monitor directly from the command line:

```bash
docker run -d \
  --name systemd-monitor \
  -p 8080:80 \
  -v /var/run/dbus:/var/run/dbus \
  -v /run/dbus:/run/dbus \
  systemd-service-monitor:latest
```

## Docker Compose Usage

Create a `docker-compose.yml` file with the following content:

```yaml
version: '3.4'
services:
  systemd-service-monitor:
    build:
      context: .
      dockerfile: Dockerfile
    image: systemd-service-monitor:latest
    ports:
      - "8080:80"
    volumes:
      - /var/run/dbus:/var/run/dbus:ro
      - /run/dbus:/run/dbus:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

To start the service:
```bash
docker-compose up -d
```

## Environment Variables

| Variable | Description | Default |
|---------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Development |
| `MONITORING_ENABLED` | Enable service monitoring | true |
| `DBUS_ADDRESS` | D-Bus connection address | `unix:path=/run/dbus/system_bus_socket` |
| `ALERT_ENABLED` | Enable alerting system | false |
| `LOG_LEVEL` | Logging level (Debug, Info, Warn, Error) | Info |

## Production Deployment Checklist

1. **Security Configuration**
   - Set up proper authentication for the web interface
   - Configure HTTPS in production
   - Review and secure all API endpoints

2. **D-Bus Access**
   - Ensure D-Bus system socket is properly mounted
   - Verify user permissions for D-Bus access
   - Test D-Bus connectivity from within container

3. **Environment Variables**
   - Set `ASPNETCORE_ENVIRONMENT` to `Production`
   - Configure `MONITORING_ENABLED` based on your needs
   - Enable alerting with `ALERT_ENABLED` if using alert features

4. **Resource Limits**
   - Set appropriate memory limits for your container
   - Configure CPU constraints based on monitoring frequency

5. **Networking**
   - Expose only necessary ports
   - Use appropriate network policies
   - Consider using a reverse proxy for production deployments

## Example Production docker-compose.yml

```yaml
version: '3.4'
services:
  systemd-service-monitor:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
    volumes:
      - /var/run/dbus:/var/run/dbus:ro
      - /run/dbus:/run/dbus:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ALERT_ENABLED=true
      - LOG_LEVEL=Warn
    restart: unless-stopped
```

## Health Checks

The application includes health check endpoints that can be used for container orchestration:

```bash
# Health check endpoint
curl http://localhost:8080/health
```

## Volume Mounts

The application requires access to D-Bus system socket. Ensure the following volumes are mounted:

- `/var/run/dbus` - System D-Bus socket
- `/run/dbus` - Session D-Bus socket

## Dockerfile Details

The Dockerfile uses a multi-stage build process:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Runtime configuration
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Copy application files
COPY . .

# Run the application
ENTRYPOINT ["dotnet", "systemd-service-monitor.dll"]
```

## Monitoring and Logging

The container includes built-in health check endpoints that can be used for container orchestration:

- `/health` - Basic health check
- `/health/detailed` - Detailed system health check

These endpoints can be used for container orchestration platforms to determine container health status.