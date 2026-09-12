# API constants

[`ApiConstants`](../Constants/ApiConstants.cs) centralizes API limits, protocol strings, routes, defaults, validation bounds, and error identifiers. The same source file also defines `FeatureDefaults`, which groups defaults for metrics, health checks, log collection, and background tasks.

All values are compile-time constants unless noted otherwise. Time units are included in constant names where applicable.

## General API defaults

| Constant | Value | Purpose |
| --- | ---: | --- |
| `ApiVersion` | `"v1"` | Current API version identifier. |
| `DefaultPageSize` | `50` | Default number of items in a paginated response. |
| `MaxPageSize` | `10000` | Maximum permitted page size. |
| `DefaultRateLimit` | `300` | Default requests allowed per minute for each IP address. |
| `DefaultCacheTtlSeconds` | `300` | Default cache lifetime, in seconds. |
| `MaxLogLines` | `10000` | Maximum log lines returned by one request. |
| `DefaultLogLines` | `100` | Default log lines returned by a request. |

## `Headers`

| Constant | Value | Purpose |
| --- | --- | --- |
| `RequestId` | `X-Request-Id` | Request identifier header. |
| `CorrelationId` | `X-Correlation-Id` | Correlation identifier header used to associate related operations. |
| `RateLimit` | `X-RateLimit-Remaining` | Header reporting the remaining request allowance. |
| `RateLimitReset` | `X-RateLimit-Reset` | Header reporting when the rate-limit allowance resets. |
| `ContentType` | `Content-Type` | Standard media-type header. |
| `Authorization` | `Authorization` | Standard credentials header. |

## `ContentTypes`

| Constant | Value |
| --- | --- |
| `Json` | `application/json` |
| `Csv` | `text/csv` |
| `Xml` | `application/xml` |
| `PlainText` | `text/plain` |

## `Routes`

| Constant | Value | Purpose |
| --- | --- | --- |
| `ApiPrefix` | `/api` | Common API route prefix. |
| `HealthCheck` | `/health` | Health-check endpoint. |
| `Services` | `/api/services` | Service operations endpoint. |
| `Logs` | `/api/logs` | Log operations endpoint. |
| `Metrics` | `/api/metrics` | Metrics endpoint. |
| `System` | `/api/system` | System information and operations endpoint. |

## `StatusMessages`

These are reusable human-readable messages for API responses.

| Constant | Value |
| --- | --- |
| `Success` | `Request completed successfully` |
| `Created` | `Resource created successfully` |
| `BadRequest` | `The request was invalid or malformed` |
| `Unauthorized` | `Authentication is required` |
| `Forbidden` | `Access is denied` |
| `NotFound` | `The requested resource was not found` |
| `Conflict` | `The request conflicts with existing data` |
| `InternalError` | `An unexpected error occurred` |
| `ServiceUnavailable` | `The service is temporarily unavailable` |
| `TooManyRequests` | `Rate limit exceeded` |

## `Service`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `ServiceExtension` | `.service` | Standard systemd service-unit suffix. |
| `DefaultRestartDelaySeconds` | `100` | Default delay before restarting a service, in seconds. |
| `MaxRestartDelaySeconds` | `3600` | Maximum restart delay, in seconds. |
| `DefaultMaxRestarts` | `5` | Default maximum restart count. |
| `DefaultUser` | `root` | Default account used for service-related operations. |

## `Logging`

| Constant | Value | Syslog severity |
| --- | --- | ---: |
| `SeverityEmergency` | `EMERG` | Emergency |
| `SeverityAlert` | `ALERT` | Alert |
| `SeverityCritical` | `CRIT` | Critical |
| `SeverityError` | `ERR` | Error |
| `SeverityWarning` | `WARN` | Warning |
| `SeverityNotice` | `NOTICE` | Notice |
| `SeverityInfo` | `INFO` | Informational |
| `SeverityDebug` | `DEBUG` | Debug |

`AllSeverities` is a static read-only string array, rather than a compile-time constant. It contains all eight values in the order shown above, from emergency through debug.

## `CacheKeys`

| Constant | Value | Purpose |
| --- | --- | --- |
| `ServicePrefix` | `service:` | Prefix for an individual service cache key. |
| `LogPrefix` | `logs:` | Prefix for a service log cache key. |
| `MetricsPrefix` | `metrics:` | Prefix for a service metrics cache key. |
| `SystemPrefix` | `system:` | Prefix reserved for system cache keys. |
| `AllServices` | `services:all` | Key for the complete service collection. |
| `SystemMetrics` | `metrics:system` | Key for system-wide metrics. |

The helper methods append a service name directly to the corresponding prefix:

| Method | Result |
| --- | --- |
| `GetServiceKey(serviceName)` | `service:{serviceName}` |
| `GetLogsKey(serviceName)` | `logs:{serviceName}` |
| `GetMetricsKey(serviceName)` | `metrics:{serviceName}` |

The helpers do not normalize or validate `serviceName`.

## `Validation`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `MaxServiceNameLength` | `255` | Maximum service-name length. |
| `MaxDescriptionLength` | `1000` | Maximum description length. |
| `MaxLogMessageLength` | `5000` | Maximum log-message length. |
| `MaxUrlLength` | `2000` | Maximum URL length. |
| `MinPortNumber` | `1` | Lowest valid port number. |
| `MaxPortNumber` | `65535` | Highest valid port number. |
| `MaxSearchTextLength` | `500` | Maximum search-text length. |

String lengths are measured in characters by consuming validation code.

## `Time`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `DefaultTimeoutSeconds` | `30` | Default operation timeout, in seconds. |
| `MaxTimeoutSeconds` | `300` | Maximum operation timeout, in seconds. |
| `DefaultHistoryDays` | `30` | Default history window, in days. |
| `MaxHistoryDays` | `365` | Maximum history window, in days. |

## `ErrorCodes`

These stable machine-readable identifiers can be returned with API errors.

| Constant | Value |
| --- | --- |
| `InvalidInput` | `INVALID_INPUT` |
| `ServiceNotFound` | `SERVICE_NOT_FOUND` |
| `PermissionDenied` | `PERMISSION_DENIED` |
| `OperationFailed` | `OPERATION_FAILED` |
| `TimeoutError` | `TIMEOUT` |
| `InternalError` | `INTERNAL_ERROR` |
| `DatabaseError` | `DATABASE_ERROR` |
| `ConnectionError` | `CONNECTION_ERROR` |

## `FeatureDefaults`

`FeatureDefaults` is a separate static class in the same file. Its nested classes organize operational defaults by feature.

### `Metrics`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `CollectionIntervalMs` | `5000` | Metric collection interval in milliseconds (5 seconds). |
| `HistoryRetentionHours` | `24` | Metric-history retention in hours (1 day). |
| `AggregationIntervalMinutes` | `5` | Metric aggregation interval in minutes. |

### `HealthCheck`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `IntervalSeconds` | `30` | Interval between health checks, in seconds. |
| `TimeoutSeconds` | `10` | Health-check timeout, in seconds. |
| `FailureThreshold` | `3` | Number of failures represented by the default threshold. |

### `LogCollection`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `BatchSizeBytes` | `1048576` | Log collection batch size in bytes (1 MiB). |
| `RetentionDays` | `7` | Log retention period in days. |
| `MaxEntriesPerRequest` | `10000` | Maximum log entries handled per request. |

### `BackgroundTasks`

| Constant | Value | Purpose |
| --- | ---: | --- |
| `StatusUpdateIntervalSeconds` | `30` | Service-status update interval, in seconds. |
| `MetricsCollectionIntervalSeconds` | `5` | Metrics collection interval, in seconds. |
| `LogCollectionIntervalSeconds` | `60` | Log collection interval, in seconds. |
| `CleanupIntervalSeconds` | `3600` | Cleanup interval, in seconds (1 hour). |

## Usage

```csharp
using SystemdServiceMonitor.Constants;

var pageSize = ApiConstants.DefaultPageSize;
var servicesRoute = ApiConstants.Routes.Services;
var cacheKey = ApiConstants.CacheKeys.GetServiceKey("nginx.service");
var healthCheckTimeout = FeatureDefaults.HealthCheck.TimeoutSeconds;
```
