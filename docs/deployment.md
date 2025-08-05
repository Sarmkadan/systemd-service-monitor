# Deployment Guide

## Pre-Deployment Checklist

- [ ] .NET 10 runtime installed on target system
- [ ] systemd available and running
- [ ] D-Bus service operational
- [ ] Network ports available (5001 for HTTP/HTTPS)
- [ ] Adequate disk space (1GB+)
- [ ] Firewall rules configured
- [ ] Backup strategy in place
- [ ] Monitoring/alerting configured
- [ ] Documentation reviewed by operations team

## Deployment Methods

### 1. Standalone Service (Systemd Integration)

#### Installation

```bash
# Create application directory
sudo mkdir -p /opt/systemd-service-monitor
cd /opt/systemd-service-monitor

# Publish application
dotnet publish -c Release -o .

# Set proper permissions
sudo chown -R root:root /opt/systemd-service-monitor
sudo chmod -R 755 /opt/systemd-service-monitor
```

#### Create Systemd Unit File

Create `/etc/systemd/system/systemd-service-monitor.service`:

```ini
[Unit]
Description=systemd Service Monitor - ASP.NET Service Monitoring Dashboard
Documentation=https://github.com/Sarmkadan/systemd-service-monitor
After=network.target dbus.service
Wants=dbus.service
Requires=dbus.service

[Service]
Type=simple
User=root
Group=root
WorkingDirectory=/opt/systemd-service-monitor
ExecStart=/opt/systemd-service-monitor/systemd-service-monitor

# Service behavior
Restart=always
RestartSec=10
StartLimitInterval=600
StartLimitBurst=3
KillMode=process

# Logging
StandardOutput=journal
StandardError=journal
SyslogIdentifier=systemd-monitor

# Security
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=/var/log/journal /run/dbus

# Resource limits
LimitNOFILE=65536
MemoryMax=2G

# Timeouts
TimeoutStopSec=30
TimeoutStartSec=60

[Install]
WantedBy=multi-user.target
```

#### Enable and Start

```bash
# Reload systemd configuration
sudo systemctl daemon-reload

# Enable on boot
sudo systemctl enable systemd-service-monitor

# Start service
sudo systemctl start systemd-service-monitor

# Check status
sudo systemctl status systemd-service-monitor

# View logs
sudo journalctl -u systemd-service-monitor -f
```

---

### 2. Docker Container Deployment

#### Build Docker Image

```bash
# Build from project
docker build -t systemd-monitor:latest .

# Tag with version
docker tag systemd-monitor:latest systemd-monitor:1.0.0
```

#### Run Single Container

```bash
docker run -d \
  --name systemd-monitor \
  --restart always \
  -p 5001:5001 \
  --privileged \
  -v /run/dbus/system_bus_socket:/run/dbus/system_bus_socket \
  -v /var/log/journal:/var/log/journal:ro \
  -v /sys:/sys:ro \
  -v /proc:/proc:ro \
  -v systemd-monitor-logs:/app/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  systemd-monitor:latest
```

#### Container Management

```bash
# View logs
docker logs -f systemd-monitor

# Check resource usage
docker stats systemd-monitor

# Stop container
docker stop systemd-monitor

# Remove container
docker rm systemd-monitor

# View container details
docker inspect systemd-monitor
```

---

### 3. Docker Compose Deployment

#### Setup Docker Compose

```bash
# Copy docker-compose.yml to deployment directory
cp docker-compose.yml /opt/docker/systemd-monitor/

# Create required directories
mkdir -p /opt/docker/systemd-monitor/logs

# Start services
cd /opt/docker/systemd-monitor
docker-compose up -d
```

#### Docker Compose Operations

```bash
# View service status
docker-compose ps

# View logs
docker-compose logs -f systemd-monitor

# Restart service
docker-compose restart systemd-monitor

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

---

### 4. Kubernetes Deployment

#### Create Kubernetes Namespace

```bash
kubectl create namespace systemd-monitor
```

#### Create ConfigMap for Configuration

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: systemd-monitor-config
  namespace: systemd-monitor
data:
  appsettings.json: |
    {
      "Systemd": {
        "EnableMonitoring": true,
        "MetricCollectionIntervalMs": 5000,
        "LogRetentionDays": 30
      },
      "Database": {
        "Provider": "InMemory"
      }
    }
```

#### Create Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: systemd-monitor
  namespace: systemd-monitor
spec:
  replicas: 1
  selector:
    matchLabels:
      app: systemd-monitor
  template:
    metadata:
      labels:
        app: systemd-monitor
    spec:
      hostNetwork: true
      hostPID: true
      containers:
      - name: systemd-monitor
        image: systemd-monitor:latest
        ports:
        - containerPort: 5001
          name: http
        volumeMounts:
        - name: dbus
          mountPath: /run/dbus
        - name: journal
          mountPath: /var/log/journal
          readOnly: true
        - name: sys
          mountPath: /sys
          readOnly: true
        - name: proc
          mountPath: /proc
          readOnly: true
        - name: config
          mountPath: /app/config
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "2"
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
            scheme: HTTPS
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 5001
            scheme: HTTPS
          initialDelaySeconds: 5
          periodSeconds: 5
      volumes:
      - name: dbus
        hostPath:
          path: /run/dbus
      - name: journal
        hostPath:
          path: /var/log/journal
      - name: sys
        hostPath:
          path: /sys
      - name: proc
        hostPath:
          path: /proc
      - name: config
        configMap:
          name: systemd-monitor-config
```

#### Create Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: systemd-monitor
  namespace: systemd-monitor
spec:
  selector:
    app: systemd-monitor
  ports:
  - port: 5001
    targetPort: 5001
    protocol: TCP
  type: ClusterIP
```

#### Deploy to Kubernetes

```bash
# Apply manifests
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml

# Check deployment status
kubectl get deployment -n systemd-monitor
kubectl get pods -n systemd-monitor

# View logs
kubectl logs -n systemd-monitor -f deployment/systemd-monitor
```

---

## Production Configuration

### Security Setup

#### HTTPS Configuration

Edit `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "HttpsInlineCertAndKey": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/etc/systemd-monitor/cert.pfx",
          "Password": "your-secure-password"
        }
      }
    }
  }
}
```

Generate self-signed certificate:

```bash
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes
openssl pkcs12 -export -in cert.pem -inkey key.pem -out cert.pfx -name "systemd-monitor"
```

#### Firewall Configuration

```bash
# Allow only from trusted networks (example)
sudo ufw allow from 192.168.1.0/24 to any port 5001

# Or restrict to localhost
sudo ufw allow from 127.0.0.1 to any port 5001
```

### Performance Tuning

#### Database Optimization

For production, implement a real database:

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=db.example.com;Username=monitor;Password=xxxxx;Database=systemd_monitor",
    "MaxConnectionPoolSize": 30,
    "CommandTimeoutSeconds": 60
  }
}
```

#### Metrics Collection Tuning

```json
{
  "Systemd": {
    "MetricCollectionIntervalMs": 10000,
    "LogRetentionDays": 90,
    "MaxLogEntriesPerRequest": 500
  }
}
```

### Logging Configuration

#### File Rotation

Configure in appsettings:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/systemd-monitor/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600
        }
      }
    ]
  }
}
```

#### Centralized Logging

Send logs to ELK/Splunk:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Http",
        "Args": {
          "requestUri": "https://elk.example.com:9200/logs",
          "batchSizeLimit": 100
        }
      }
    ]
  }
}
```

---

## Monitoring and Alerting

### Health Checks

```bash
# Monitor health endpoint
watch -n 5 'curl -s -k https://localhost:5001/health | jq .'
```

### Prometheus Metrics (Optional)

Add Prometheus exporter:

```csharp
builder.Services.AddPrometheusMetrics();
app.MapPrometheusMetrics();
```

### Log Aggregation

```bash
# Monitor logs
tail -f /var/log/systemd-monitor/app-*.txt | grep -E "ERROR|WARNING"
```

### Service Monitoring

```bash
# Check service status
sudo systemctl status systemd-service-monitor

# Monitor resource usage
ps aux | grep systemd-service-monitor
```

---

## Backup and Recovery

### Backup Strategy

```bash
#!/bin/bash
# Backup application and data
BACKUP_DIR="/backups/systemd-monitor"
mkdir -p "$BACKUP_DIR"

# Backup application
tar -czf "$BACKUP_DIR/app-$(date +%Y%m%d).tar.gz" /opt/systemd-service-monitor

# Backup logs
tar -czf "$BACKUP_DIR/logs-$(date +%Y%m%d).tar.gz" /opt/systemd-service-monitor/logs

# Backup database (if using external DB)
# mysqldump ... > "$BACKUP_DIR/db-$(date +%Y%m%d).sql"

# Cleanup old backups (keep 30 days)
find "$BACKUP_DIR" -name "*.tar.gz" -mtime +30 -delete
```

### Recovery Procedure

```bash
# Stop service
sudo systemctl stop systemd-service-monitor

# Restore from backup
cd /opt
sudo tar -xzf /backups/systemd-monitor/app-20260504.tar.gz

# Restart service
sudo systemctl start systemd-service-monitor

# Verify
sudo systemctl status systemd-service-monitor
```

---

## Troubleshooting Deployments

### Issue: Service won't start

**Diagnostics**
```bash
# Check systemd journal
sudo journalctl -u systemd-service-monitor -n 50

# Test manual run
cd /opt/systemd-service-monitor
./systemd-service-monitor
```

**Solutions**
- Verify .NET 10 runtime: `dotnet --info`
- Check D-Bus access: `ls -la /run/dbus/system_bus_socket`
- Check permissions: `sudo chown -R root:root /opt/systemd-service-monitor`

### Issue: High memory usage

**Diagnostics**
```bash
# Check process memory
ps aux | grep systemd-service-monitor

# Monitor memory over time
watch -n 2 'ps aux | grep systemd-service-monitor'
```

**Solutions**
- Reduce `MetricCollectionIntervalMs`
- Decrease `MaxLogEntriesPerRequest`
- Implement log rotation
- Check for memory leaks: `dotnet-counters monitor systemd-service-monitor`

### Issue: Slow API responses

**Diagnostics**
```bash
# Measure endpoint response time
time curl -k https://localhost:5001/api/services

# Check D-Bus performance
dbus-send --system --print-reply --dest=org.freedesktop.DBus /org/freedesktop/DBus org.freedesktop.DBus.Introspectable.Introspect
```

**Solutions**
- Reduce service list size
- Implement caching
- Optimize D-Bus queries
- Profile: `dotnet trace collect -p <pid>`

---

## Scaling Considerations

### Horizontal Scaling

1. Deploy multiple instances behind load balancer
2. Use shared database backend
3. Implement distributed caching
4. Configure health checks for failover

### Load Balancer Configuration (Nginx)

```nginx
upstream systemd_monitor {
    server 10.0.1.10:5001 weight=5;
    server 10.0.1.11:5001 weight=5;
    server 10.0.1.12:5001;
}

server {
    listen 443 ssl;
    server_name monitor.example.com;
    
    ssl_certificate /etc/ssl/certs/monitor.crt;
    ssl_certificate_key /etc/ssl/private/monitor.key;
    
    location / {
        proxy_pass https://systemd_monitor;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Maintenance

### Regular Tasks

```bash
# Weekly: Check logs and metrics
sudo journalctl -u systemd-service-monitor --since "7 days ago" | grep ERROR

# Monthly: Clean old data
sqlite3 /var/lib/systemd-monitor/data.db "DELETE FROM metrics WHERE timestamp < date('now', '-90 days');"

# Quarterly: Backup and archival
# Run backup script quarterly
```

### Updates and Upgrades

```bash
# Backup current version
cp -r /opt/systemd-service-monitor /opt/systemd-service-monitor.backup

# Download and publish new version
dotnet publish -c Release -o /opt/systemd-service-monitor.new

# Stop service
sudo systemctl stop systemd-service-monitor

# Swap directories
mv /opt/systemd-service-monitor /opt/systemd-service-monitor.old
mv /opt/systemd-service-monitor.new /opt/systemd-service-monitor

# Start service
sudo systemctl start systemd-service-monitor

# Verify
sudo systemctl status systemd-service-monitor

# Cleanup old version (after verification)
rm -rf /opt/systemd-service-monitor.old
```

---

## Validation Checklist

After deployment, verify:

- [ ] Service starts automatically on reboot
- [ ] Health check endpoint responds
- [ ] API endpoints are accessible
- [ ] Logs are being written
- [ ] Metrics are being collected
- [ ] D-Bus connection is working
- [ ] Services are discoverable
- [ ] HTTPS is working (production)
- [ ] Firewall rules are in place
- [ ] Backups are configured
- [ ] Monitoring/alerting is active
- [ ] Documentation is up to date

---

## Reference Documentation

- [systemd Documentation](https://www.freedesktop.org/software/systemd/man/)
- [ASP.NET Core Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
