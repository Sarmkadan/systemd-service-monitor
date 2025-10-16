# Frequently Asked Questions

## General Questions

### Q1: What operating systems are supported?

**A:** systemd-service-monitor requires a Linux distribution with systemd. Confirmed compatible systems include:
- Ubuntu 20.04 LTS and later
- Debian 11 and later
- CentOS 8 and later
- Fedora 35 and later
- RHEL 8 and later
- Any distribution with systemd 230+

Windows and macOS are not supported due to systemd dependency.

---

### Q2: Do I need root privileges?

**A:** For monitoring only (read service status, logs), standard user privileges with D-Bus access may be sufficient. However, for service control operations (start/stop/restart), root or systemd-privileged user is required.

**Running with least privilege:**
```bash
# Create dedicated user
sudo useradd -r -s /bin/false systemd-monitor

# Add to system groups with D-Bus access
sudo usermod -aG systemd-journal systemd-monitor

# Update systemd unit file
# [Service]
# User=systemd-monitor
```

---

### Q3: What's the difference between this and other monitoring tools?

**A:** systemd-service-monitor is specifically designed for systemd services with these strengths:
- **D-Bus Integration**: Direct systemd communication (no polling files)
- **Journal Access**: Full systemd journald integration
- **Web Dashboard**: Built-in ASP.NET web interface
- **REST API**: Complete API for integration
- **Service Control**: Direct service management
- **Resource Tracking**: Per-service metrics collection

Compared to Prometheus: More integrated, less scraping overhead
Compared to systemctl UI: More modern interface, remote access

---

### Q4: Can I monitor remote systems?

**A:** Current version monitors the local system only. For remote monitoring:

**Option 1: Separate instance per system**
Deploy on each system, expose APIs behind load balancer.

**Option 2: Remote D-Bus forwarding** (Advanced)
```bash
# SSH tunnel to remote system's D-Bus
ssh -L /tmp/dbus-socket:/run/dbus/system_bus_socket user@remote-host
```

**Option 3: Future enhancement**
Remote D-Bus support is planned for v2.0.

---

## Installation Questions

### Q5: How much disk space do I need?

**A:** Minimum requirements:
- **Application**: 50-100 MB (depends on .NET runtime)
- **Logs**: 100 MB per week (with default retention)
- **Metrics**: 50-100 MB per week (with default collection)
- **Total**: 1 GB recommended

For long-term metrics storage, use external database:
```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "..."
  }
}
```

---

### Q6: What are the memory requirements?

**A:** Memory usage depends on:
- Number of monitored services
- Metric collection frequency
- Log retention period
- Dashboard concurrent users

**Typical usage:**
- Idle: 50-100 MB
- 50 services, 5s metrics: 150-200 MB
- 200 services, 1s metrics: 500-1000 MB

Configure limits in systemd unit:
```ini
[Service]
MemoryMax=2G
```

---

### Q7: Which .NET version do I need?

**A:** .NET 10 runtime is required (must be installed first).

**Installation:**
```bash
# Ubuntu/Debian
sudo apt-get install dotnet-runtime-10.0

# CentOS/RHEL
sudo dnf install dotnet-runtime-10.0

# Manual download
# https://dotnet.microsoft.com/download/dotnet/10.0
```

**Why .NET 10?**
- Latest features and performance improvements
- Extended support timeline
- Security patches for 3 years

---

## Configuration Questions

### Q8: How do I change the port number?

**A:** Edit `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      }
    }
  }
}
```

Or use environment variable:
```bash
export ASPNETCORE_URLS=http://0.0.0.0:8080
dotnet run
```

---

### Q9: How do I enable HTTPS?

**A:** For production deployments:

```json
{
  "Kestrel": {
    "Endpoints": {
      "HttpsInlineCertAndKey": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/etc/systemd-monitor/cert.pfx",
          "Password": "your-password"
        }
      }
    }
  }
}
```

Generate certificate:
```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
openssl pkcs12 -export -in cert.pem -inkey key.pem -out cert.pfx
```

---

### Q10: How do I change metric collection frequency?

**A:** Edit `appsettings.json`:

```json
{
  "Systemd": {
    "MetricCollectionIntervalMs": 5000
  }
}
```

**Values:**
- 1000 ms: High-frequency (higher CPU/memory)
- 5000 ms: Default (balanced)
- 30000 ms: Low-frequency (lower overhead)

---

### Q11: How do I adjust log retention?

**A:** Edit `appsettings.json`:

```json
{
  "Systemd": {
    "LogRetentionDays": 30
  }
}
```

**Values:**
- 7 days: Less storage, quick data loss
- 30 days: Balanced (default)
- 90 days: Long-term history, more storage

---

## Usage Questions

### Q12: How do I access the dashboard?

**A:** Open your browser:
- **Local**: `https://localhost:5001`
- **Remote**: `https://your-server-ip:5001`

Self-signed certificate warning is normal in development.

---

### Q13: How do I use the REST API?

**A:** Query endpoints with curl, Postman, or code:

```bash
# List services
curl -k https://localhost:5001/api/services | jq .

# Get service details
curl -k https://localhost:5001/api/services/nginx.service | jq .

# Start service
curl -k -X POST https://localhost:5001/api/services/nginx.service/start
```

**Documentation**: See `docs/api-reference.md` or `/swagger` endpoint.

---

### Q14: Can I automate service restarts?

**A:** Yes, use the REST API with cron:

```bash
#!/bin/bash
# Restart service if not running (crontab: */5 * * * *)
SERVICE=nginx.service
STATUS=$(curl -s -k https://localhost:5001/api/services/$SERVICE | jq -r '.data.status.isActive')

if [ "$STATUS" = "false" ]; then
    curl -k -X POST https://localhost:5001/api/services/$SERVICE/start
    logger "Restarted $SERVICE"
fi
```

---

### Q15: How do I export metrics?

**A:** The API returns JSON directly:

```bash
# Export to file
curl -k "https://localhost:5001/api/metrics/services/nginx.service" > metrics.json

# Parse with jq
curl -k "https://localhost:5001/api/metrics/services/nginx.service" | jq '.data.metrics'

# Import into external system
# Use API client library for your programming language
```

---

## Troubleshooting Questions

### Q16: D-Bus connection failed error

**A:** Verify systemd and D-Bus are running:

```bash
# Check systemd
systemctl status systemd-journald

# Check D-Bus
systemctl status dbus

# Test D-Bus access
dbus-send --system --print-reply / org.freedesktop.DBus.Introspectable.Introspect

# Check socket permissions
ls -la /run/dbus/system_bus_socket
```

**Solution**: Run with elevated privileges:
```bash
sudo dotnet run
# or
sudo systemctl start systemd-service-monitor
```

---

### Q17: "Permission denied" when managing services

**A:** The application must have sufficient privileges for service control.

**Solution 1: Run as root**
```bash
sudo dotnet run
```

**Solution 2: Use sudo without password** (not recommended)
```bash
sudo visudo
# Add: www-data ALL=(ALL) NOPASSWD: /path/to/systemd-service-monitor
```

**Solution 3: Implement authorization layer** (recommended)
Add OAuth/JWT authentication in future versions.

---

### Q18: Services not appearing in the list

**A:** Verify service exists and is discoverable:

```bash
# List all services
systemctl list-units --type=service --all

# Check specific service
systemctl status nginx.service

# Verify service isn't masked
systemctl is-enabled nginx.service
```

**Common issues:**
- Service name doesn't include `.service` suffix
- Service is masked: `systemctl unmask service-name`
- Service file in non-standard location

---

### Q19: API responses are slow

**A:** Troubleshoot performance:

```bash
# Measure endpoint response time
time curl -k https://localhost:5001/api/services

# Check D-Bus performance
strace -e openat,close dbus-send ... 2>&1 | grep duration

# Profile application
dotnet trace collect -p <pid> -d 30 -o trace.nettrace
dotnet trace report trace.nettrace
```

**Solutions:**
- Reduce service list: `?pageSize=10`
- Increase metric collection interval
- Cache frequently-accessed services
- Add database for persistence

---

### Q20: High CPU/memory usage

**A:** Monitor resource consumption:

```bash
# Check process
ps aux | grep systemd-service-monitor

# Monitor over time
watch -n 1 'ps aux | grep systemd-service-monitor'

# Check memory breakdown
dotnet-counters monitor --process-id <pid>
```

**Solutions:**
- Reduce metric collection frequency
- Decrease log retention days
- Limit log entries per request
- Implement memory caching
- Use database instead of in-memory storage

---

## Advanced Questions

### Q21: How do I integrate with Nagios/Icinga?

**A:** Create a check script:

```bash
#!/bin/bash
SERVICE=${1:-nginx.service}
API="https://localhost:5001/api/services/$SERVICE"

RESPONSE=$(curl -s -k "$API")
STATE=$(echo "$RESPONSE" | jq -r '.data.status.isActive')

if [ "$STATE" = "true" ]; then
    echo "OK: Service is running"
    exit 0
else
    echo "CRITICAL: Service is stopped"
    exit 2
fi
```

Register in Icinga:
```yaml
object Service "nginx-status" {
  check_command = "check_systemd_service"
  vars.service = "nginx.service"
}
```

---

### Q22: Can I use this with Kubernetes?

**A:** Yes, see `docs/deployment.md` for Kubernetes manifests.

**Key requirements:**
- Privileged container access
- D-Bus socket mounting
- Host network access

```bash
kubectl apply -f deployment.yaml
```

---

### Q23: How do I implement custom health checks?

**A:** Create custom health check service:

```csharp
public class CustomHealthProbe : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var services = new[] { "nginx.service", "mysql.service" };
        foreach (var service in services)
        {
            var status = await _monitorService.GetServiceStatusAsync(service);
            if (!status.IsActive)
                return HealthCheckResult.Unhealthy($"{service} is down");
        }
        return HealthCheckResult.Healthy();
    }
}
```

Register:
```csharp
services.AddHealthChecks()
    .AddCheck<CustomHealthProbe>("custom");
```

---

### Q24: How can I contribute?

**A:** Contributions are welcome:

1. Fork the repository
2. Create feature branch: `git checkout -b feature/description`
3. Make changes with tests
4. Submit pull request with description

**Guidelines:**
- Follow C# naming conventions
- Include XML documentation
- Write unit tests
- Update README if needed

See CONTRIBUTING.md for details.

---

### Q25: What's the roadmap?

**A:** Planned features for v2.0:

- [ ] Remote D-Bus monitoring
- [ ] Authentication (OAuth/JWT)
- [ ] Distributed deployment support
- [ ] Prometheus metrics export
- [ ] Service dependency visualization
- [ ] Advanced alerting system
- [ ] Mobile app
- [ ] Historical trend analysis

Follow GitHub issues for updates.

---

## Getting Help

### Documentation
- [Getting Started Guide](getting-started.md)
- [Architecture Guide](architecture.md)
- [API Reference](api-reference.md)
- [Deployment Guide](deployment.md)

### Examples
- See `examples/` directory for code samples
- Check `/swagger` endpoint for interactive API docs

### Community
- **Issues**: [GitHub Issues](https://github.com/Sarmkadan/systemd-service-monitor/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Sarmkadan/systemd-service-monitor/discussions)
- **Email**: Contact at https://sarmkadan.com

### Debug Logging

Enable debug logging for troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

---

## Additional Resources

- [systemd Manual](https://www.freedesktop.org/software/systemd/man/)
- [D-Bus Specification](https://dbus.freedesktop.org/doc/dbus-daemon.1.html)
- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/)
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/)
