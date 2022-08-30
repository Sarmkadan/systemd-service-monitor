# API Reference

## Base Information

### Base URL
```
https://localhost:5001/api
```

### Response Format
All responses use a standard envelope format:

```json
{
  "success": boolean,
  "data": any,
  "message": string,
  "timestamp": "ISO 8601 datetime",
  "errors": [ "error1", "error2" ]
}
```

### Status Codes
- `200 OK` - Request successful
- `201 Created` - Resource created
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Permission denied
- `404 Not Found` - Resource not found
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error
- `503 Service Unavailable` - Service temporarily unavailable

## Services Endpoints

### List All Services

Retrieve a paginated list of all systemd services.

**Endpoint**
```http
GET /services
```

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | integer | 1 | Page number (1-indexed) |
| `pageSize` | integer | 10 | Items per page (1-100) |
| `state` | string | null | Filter by state: `active`, `inactive`, `failed` |
| `search` | string | null | Search service name or description |
| `sortBy` | string | `name` | Sort column: `name`, `state`, `activeTime` |
| `sortOrder` | string | `asc` | Sort direction: `asc`, `desc` |

**Example Request**
```bash
curl -k "https://localhost:5001/api/services?page=1&pageSize=20&state=active"
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "name": "nginx.service",
        "displayName": "nginx",
        "description": "nginx - High Performance Web Server",
        "state": "active",
        "subState": "running",
        "loadState": "loaded",
        "activeState": "active",
        "unitFileState": "enabled",
        "status": {
          "timestamp": "2026-05-04T10:30:00Z",
          "isActive": true,
          "isFailed": false,
          "uptimeSeconds": 3600
        }
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 20
  },
  "message": "Services retrieved successfully",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

**Error Response (400)**
```json
{
  "success": false,
  "data": null,
  "message": "Invalid page size",
  "errors": [ "pageSize must be between 1 and 100" ],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

### Get Service Details

Retrieve detailed information about a specific service.

**Endpoint**
```http
GET /services/{serviceName}
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `serviceName` | string | Service name (e.g., `nginx.service`) |

**Example Request**
```bash
curl -k https://localhost:5001/api/services/nginx.service
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "displayName": "nginx",
    "description": "nginx - High Performance Web Server",
    "state": "active",
    "subState": "running",
    "loadState": "loaded",
    "activeState": "active",
    "unitFileState": "enabled",
    "status": {
      "timestamp": "2026-05-04T10:30:00Z",
      "isActive": true,
      "isFailed": false,
      "uptimeSeconds": 3600
    },
    "pid": 1234,
    "mainPid": 1234,
    "memory": {
      "rss": 48.2,
      "vsz": 512.5
    },
    "lastStatusChangeTime": "2026-05-04T09:30:00Z",
    "recentLogs": [
      {
        "timestamp": "2026-05-04T10:29:55Z",
        "priority": "notice",
        "message": "worker process started"
      }
    ]
  },
  "message": "Service details retrieved",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

**Error Response (404)**
```json
{
  "success": false,
  "data": null,
  "message": "Service not found",
  "errors": [ "Service 'nonexistent.service' not found" ],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

### Start Service

Start a systemd service.

**Endpoint**
```http
POST /services/{serviceName}/start
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `serviceName` | string | Service name (e.g., `nginx.service`) |

**Example Request**
```bash
curl -k -X POST https://localhost:5001/api/services/nginx.service/start
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "previousState": "inactive",
    "currentState": "active",
    "message": "Service started successfully",
    "timestamp": "2026-05-04T10:30:00Z"
  },
  "message": "Service started successfully",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

**Error Response (409)**
```json
{
  "success": false,
  "data": null,
  "message": "Service operation failed",
  "errors": [ "Service is already running" ],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

### Stop Service

Stop a systemd service.

**Endpoint**
```http
POST /services/{serviceName}/stop
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `serviceName` | string | Service name (e.g., `nginx.service`) |

**Example Request**
```bash
curl -k -X POST https://localhost:5001/api/services/nginx.service/stop
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "previousState": "active",
    "currentState": "inactive",
    "message": "Service stopped successfully",
    "timestamp": "2026-05-04T10:30:00Z"
  },
  "message": "Service stopped successfully",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

### Restart Service

Restart a systemd service with configurable strategy.

**Endpoint**
```http
POST /services/{serviceName}/restart
```

**Path Parameters**
| Parameter | Type | Description |
|-----------|------|-------------|
| `serviceName` | string | Service name (e.g., `nginx.service`) |

**Request Body**
```json
{
  "restartMode": "always",
  "waitForReady": true,
  "timeoutSeconds": 30
}
```

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `restartMode` | string | `always` | Restart strategy: `always`, `on-failure`, `systemd-default` |
| `waitForReady` | boolean | true | Wait for service to be fully ready |
| `timeoutSeconds` | integer | 30 | Timeout for restart operation |

**Example Request**
```bash
curl -k -X POST https://localhost:5001/api/services/nginx.service/restart \
  -H "Content-Type: application/json" \
  -d '{"restartMode": "always"}'
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "previousState": "active",
    "currentState": "active",
    "message": "Service restarted successfully",
    "restartMode": "always",
    "durationSeconds": 2.5,
    "timestamp": "2026-05-04T10:30:00Z"
  },
  "message": "Service restarted successfully",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

### Enable Service

Enable a service to start at boot.

**Endpoint**
```http
POST /services/{serviceName}/enable
```

**Example Request**
```bash
curl -k -X POST https://localhost:5001/api/services/nginx.service/enable
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "previousState": "disabled",
    "currentState": "enabled",
    "message": "Service enabled"
  }
}
```

---

### Disable Service

Disable a service from starting at boot.

**Endpoint**
```http
POST /services/{serviceName}/disable
```

**Example Request**
```bash
curl -k -X POST https://localhost:5001/api/services/nginx.service/disable
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "name": "nginx.service",
    "previousState": "enabled",
    "currentState": "disabled"
  }
}
```

---

## Logs Endpoints

### Get Service Logs

Retrieve logs for a specific service from systemd journal.

**Endpoint**
```http
GET /services/{serviceName}/logs
```

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `lines` | integer | 100 | Number of log lines to retrieve |
| `since` | string (ISO 8601) | null | Start time for logs |
| `until` | string (ISO 8601) | null | End time for logs |
| `priority` | string | null | Filter by priority: `emerg`, `alert`, `crit`, `err`, `warning`, `notice`, `info`, `debug` |
| `search` | string | null | Full-text search in log messages |
| `reverse` | boolean | false | Reverse sort order (oldest first) |

**Example Request**
```bash
curl -k "https://localhost:5001/api/services/nginx.service/logs?lines=50&priority=err"
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "serviceName": "nginx.service",
    "logs": [
      {
        "timestamp": "2026-05-04T10:29:55Z",
        "priority": "err",
        "message": "connection timeout",
        "processId": 1234,
        "threadId": 1,
        "systemdUnit": "nginx.service"
      },
      {
        "timestamp": "2026-05-04T10:29:50Z",
        "priority": "warning",
        "message": "slow request detected",
        "processId": 1234
      }
    ],
    "totalCount": 2,
    "retrievedCount": 2
  },
  "message": "Logs retrieved successfully",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

## Metrics Endpoints

### Get Service Metrics

Retrieve performance metrics for a service.

**Endpoint**
```http
GET /metrics/services/{serviceName}
```

**Query Parameters**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `from` | string (ISO 8601) | 1 hour ago | Start time for metrics |
| `to` | string (ISO 8601) | now | End time for metrics |
| `resolution` | string | `5m` | Aggregation: `1m`, `5m`, `15m`, `1h` |

**Example Request**
```bash
curl -k "https://localhost:5001/api/metrics/services/nginx.service?from=2026-05-04T00:00:00Z&to=2026-05-04T23:59:59Z&resolution=5m"
```

**Example Response**
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
      },
      {
        "timestamp": "2026-05-04T10:35:00Z",
        "cpuPercent": 3.1,
        "memoryMb": 49.5,
        "diskReadBytesPerSec": 512,
        "diskWriteBytesPerSec": 1024,
        "networkInBytesPerSec": 98304,
        "networkOutBytesPerSec": 210944
      }
    ],
    "statistics": {
      "cpuAvg": 2.8,
      "cpuMax": 3.1,
      "memoryAvg": 48.85,
      "memoryMax": 49.5
    }
  }
}
```

---

### Get System Resources

Retrieve overall system resource usage.

**Endpoint**
```http
GET /system/resources
```

**Example Request**
```bash
curl -k https://localhost:5001/api/system/resources
```

**Example Response**
```json
{
  "success": true,
  "data": {
    "cpuCount": 4,
    "cpuPercent": 35.2,
    "loadAverage": {
      "oneMinute": 0.8,
      "fiveMinutes": 1.2,
      "fifteenMinutes": 1.5
    },
    "memoryTotalMb": 8192,
    "memoryUsedMb": 4096,
    "memoryFreeMb": 4096,
    "memoryPercent": 50.0,
    "diskTotalGb": 100,
    "diskUsedGb": 45,
    "diskFreeGb": 55,
    "diskPercent": 45.0,
    "uptime": "45 days, 12 hours",
    "timestamp": "2026-05-04T10:30:00Z"
  }
}
```

---

## Health Check Endpoint

### System Health

Get overall system health status.

**Endpoint**
```http
GET /health
```

**Example Request**
```bash
curl -k https://localhost:5001/health
```

**Example Response (200 OK)**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "DatabaseConnection",
      "status": "Healthy"
    },
    {
      "name": "SystemdConnection",
      "status": "Healthy"
    },
    {
      "name": "MemoryUsage",
      "status": "Healthy"
    }
  ],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

**Example Response (503 Service Unavailable)**
```json
{
  "status": "Unhealthy",
  "checks": [
    {
      "name": "SystemdConnection",
      "status": "Unhealthy",
      "description": "Cannot connect to D-Bus"
    }
  ],
  "timestamp": "2026-05-04T10:30:00Z"
}
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse.

**Rate Limits** (default configuration)
- 100 requests per minute per IP address
- 1000 requests per hour per IP address

**Headers**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 99
X-RateLimit-Reset: 1630000000
```

**Error Response (429)**
```json
{
  "success": false,
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 60
}
```

---

## Error Handling

### Common Error Codes

| Code | Status | Description |
|------|--------|-------------|
| `SERVICE_NOT_FOUND` | 404 | Requested service doesn't exist |
| `INVALID_PARAMETERS` | 400 | Request parameters are invalid |
| `DBUS_CONNECTION_ERROR` | 503 | Cannot connect to D-Bus |
| `OPERATION_TIMEOUT` | 504 | Service operation exceeded timeout |
| `PERMISSION_DENIED` | 403 | Insufficient permissions for operation |
| `SERVICE_STATE_ERROR` | 409 | Service is in invalid state for operation |
| `INTERNAL_ERROR` | 500 | Unexpected server error |

### Error Response Format

```json
{
  "success": false,
  "data": null,
  "message": "Operation failed",
  "errors": [
    "Service 'nginx.service' not found"
  ],
  "timestamp": "2026-05-04T10:30:00Z",
  "errorCode": "SERVICE_NOT_FOUND"
}
```

---

## Request Examples

### cURL Examples

```bash
# List services
curl -k https://localhost:5001/api/services | jq .

# Get service details
curl -k https://localhost:5001/api/services/nginx.service | jq .

# Start service
curl -k -X POST https://localhost:5001/api/services/nginx.service/start

# Get logs (last 50 lines)
curl -k "https://localhost:5001/api/services/nginx.service/logs?lines=50"

# Get metrics for past 24 hours
curl -k "https://localhost:5001/api/metrics/services/nginx.service?from=2026-05-03T10:30:00Z&to=2026-05-04T10:30:00Z"

# Health check
curl -k https://localhost:5001/health | jq .
```

### PowerShell Examples

```powershell
# Get all services
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/services" -SkipCertificateCheck
$response.data.items

# Start a service
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/services/nginx.service/start" `
  -Method Post `
  -SkipCertificateCheck
$response.data
```

### .NET C# Examples

```csharp
using HttpClientHandler handler = new()
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};
using HttpClient client = new(handler);

// Get services
var response = await client.GetAsync("https://localhost:5001/api/services");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);

// Start service
var request = new HttpRequestMessage(
    HttpMethod.Post,
    "https://localhost:5001/api/services/nginx.service/start");
response = await client.SendAsync(request);
```

---

## OpenAPI / Swagger

The complete API is documented in OpenAPI format.

**Access Swagger UI**
```
https://localhost:5001/swagger
```

**Download OpenAPI Spec**
```
https://localhost:5001/swagger/v1/swagger.json
```

---

## Pagination

Endpoints supporting pagination use standard parameters:

```
GET /api/services?page=2&pageSize=20
```

**Response**
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 2,
  "pageSize": 20,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

---

## Performance Guidelines

**Recommended Query Limits**
- `pageSize`: Keep under 100 items
- `lines` (logs): Keep under 10,000
- `resolution` (metrics): Use larger buckets for longer time ranges
- `from`/`to` (metrics): Limit to 30 days for quick queries

**Example**: Metrics for past year
```bash
# Instead of: ?from=2025-05-04&to=2026-05-04&resolution=1m (bad)
# Use:        ?from=2025-05-04&to=2026-05-04&resolution=1h (good)
```

---

## Versioning

Current API version: `v1`

All endpoints use `/api/` prefix. Future versions will use `/api/v2/` etc., maintaining backward compatibility with v1.
