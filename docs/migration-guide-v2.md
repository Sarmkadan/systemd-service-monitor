# Migration Guide to v2.0

This guide will help you migrate from v1.x to v2.0 of the systemd-service-monitor application.

## New in v2.0

### Alert Rules Engine
The major feature added in v2.0 is the alert rules engine with escalation policies and on-call rotation support.

### Breaking Changes
- Alert configuration has been moved from basic monitoring to a full alert rules engine
- New required dependencies for alerting system
- Configuration structure has changed to support alert rules

## Migration Steps

### 1. Update Configuration
Update your `appsettings.json` to include the new alert configuration:

```json
{
  "Systemd": {
    "Services": {
      "EnableMonitoring": true
    }
  },
  "Alerts": {
    "Rules": [
      {
        "Name": "ServiceDown",
        "Condition": "status == 'inactive'",
        "Severity": "Critical",
        "Notification": {
          "Enabled": true,
          "Channels": ["email", "webhook"]
        }
      }
    ],
    "Policies": [
      {
        "Name": "DefaultEscalation",
        "Steps": [
          {
            "Level": 1,
            "Timeout": "00:05:00",
            "Action": "email",
            "Recipients": ["admin@localhost"]
          }
        ]
      }
    ]
  }
}
```

### 2. Update your service registration
Service registration has been enhanced to support the new alert engine:

```csharp
// v1.x
services.AddServiceMonitoring();

// v2.0
services.AddServiceMonitoring(config => {
  config.EnableAlerts = true;
  config.AlertRules = new[] {
    new AlertRule {
      Name = "ServiceDown",
      Description = "Service status monitoring",
      Enabled = true
    }
  };
});
```

## Code Examples

### Old vs New API

v1.x approach:
```csharp
var status = ServiceMonitor.CheckStatus("my-service");
```

v2.0 approach:
```csharp
var status = await _serviceMonitorService.CheckStatusAsync("my-service");
var alerts = _alertRulesEngine.Evaluate(service);
```

## Configuration Changes

### New Alert Configuration
The new alert system requires additional configuration in your `appsettings.json`:

```json
{
  "Alerting": {
    "Enabled": true,
    "Rules": [
      {
        "Name": "ServiceDown",
        "Type": "StatusCheck",
        "Condition": "ActiveState != 'active'",
        "Severity": "High"
      }
    ]
  }
}
```

## Docker Updates

The Docker image has been updated to include the new alerting system. Update your docker-compose:

```yaml
# v2.0 docker-compose.yml
version: '3.4'
services:
  systemd-service-monitor:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ALERT_ENABLED=true
```