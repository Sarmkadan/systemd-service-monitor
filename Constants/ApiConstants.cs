#nullable enable

namespace SystemdServiceMonitor.Constants;

/// <summary>
/// API-related constants used throughout the application.
/// Centralizes magic strings and configuration values for consistency.
/// </summary>
public static class ApiConstants
{
    /// <summary>
    /// API version string.
    /// </summary>
    public const string ApiVersion = "v1";

    /// <summary>
    /// Default page size for paginated responses.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// Maximum allowed page size to prevent abuse.
    /// </summary>
    public const int MaxPageSize = 10000;

    /// <summary>
    /// Default rate limit: requests per minute per IP.
    /// </summary>
    public const int DefaultRateLimit = 300;

    /// <summary>
    /// Cache TTL in seconds.
    /// </summary>
    public const int DefaultCacheTtlSeconds = 300;

    /// <summary>
    /// Maximum log lines to retrieve in a single request.
    /// </summary>
    public const int MaxLogLines = 10000;

    /// <summary>
    /// Default log lines to retrieve.
    /// </summary>
    public const int DefaultLogLines = 100;

    /// <summary>
    /// HTTP header names.
    /// </summary>
    public static class Headers
    {
        public const string RequestId = "X-Request-Id";
        public const string CorrelationId = "X-Correlation-Id";
        public const string RateLimit = "X-RateLimit-Remaining";
        public const string RateLimitReset = "X-RateLimit-Reset";
        public const string ContentType = "Content-Type";
        public const string Authorization = "Authorization";
    }

    /// <summary>
    /// Content type constants.
    /// </summary>
    public static class ContentTypes
    {
        public const string Json = "application/json";
        public const string Csv = "text/csv";
        public const string Xml = "application/xml";
        public const string PlainText = "text/plain";
    }

    /// <summary>
    /// API route segments.
    /// </summary>
    public static class Routes
    {
        public const string ApiPrefix = "/api";
        public const string HealthCheck = "/health";
        public const string Services = "/api/services";
        public const string Logs = "/api/logs";
        public const string Metrics = "/api/metrics";
        public const string System = "/api/system";
    }

    /// <summary>
    /// HTTP status code messages.
    /// </summary>
    public static class StatusMessages
    {
        public const string Success = "Request completed successfully";
        public const string Created = "Resource created successfully";
        public const string BadRequest = "The request was invalid or malformed";
        public const string Unauthorized = "Authentication is required";
        public const string Forbidden = "Access is denied";
        public const string NotFound = "The requested resource was not found";
        public const string Conflict = "The request conflicts with existing data";
        public const string InternalError = "An unexpected error occurred";
        public const string ServiceUnavailable = "The service is temporarily unavailable";
        public const string TooManyRequests = "Rate limit exceeded";
    }

    /// <summary>
    /// Service-related constants.
    /// </summary>
    public static class Service
    {
        public const string ServiceExtension = ".service";
        public const int DefaultRestartDelaySeconds = 100;
        public const int MaxRestartDelaySeconds = 3600;
        public const int DefaultMaxRestarts = 5;
        public const string DefaultUser = "root";
    }

    /// <summary>
    /// Log-related constants.
    /// </summary>
    public static class Logging
    {
        public const string SeverityEmergency = "EMERG";
        public const string SeverityAlert = "ALERT";
        public const string SeverityCritical = "CRIT";
        public const string SeverityError = "ERR";
        public const string SeverityWarning = "WARN";
        public const string SeverityNotice = "NOTICE";
        public const string SeverityInfo = "INFO";
        public const string SeverityDebug = "DEBUG";

        public static readonly string[] AllSeverities = new[]
        {
            SeverityEmergency, SeverityAlert, SeverityCritical, SeverityError,
            SeverityWarning, SeverityNotice, SeverityInfo, SeverityDebug
        };
    }

    /// <summary>
    /// Cache key patterns.
    /// </summary>
    public static class CacheKeys
    {
        public const string ServicePrefix = "service:";
        public const string LogPrefix = "logs:";
        public const string MetricsPrefix = "metrics:";
        public const string SystemPrefix = "system:";
        public const string AllServices = "services:all";
        public const string SystemMetrics = "metrics:system";

        public static string GetServiceKey(string serviceName) => $"{ServicePrefix}{serviceName}";
        public static string GetLogsKey(string serviceName) => $"{LogPrefix}{serviceName}";
        public static string GetMetricsKey(string serviceName) => $"{MetricsPrefix}{serviceName}";
    }

    /// <summary>
    /// Validation constants.
    /// </summary>
    public static class Validation
    {
        public const int MaxServiceNameLength = 255;
        public const int MaxDescriptionLength = 1000;
        public const int MaxLogMessageLength = 5000;
        public const int MaxUrlLength = 2000;
        public const int MinPortNumber = 1;
        public const int MaxPortNumber = 65535;
        public const int MaxSearchTextLength = 500;
    }

    /// <summary>
    /// Time constants.
    /// </summary>
    public static class Time
    {
        public const int DefaultTimeoutSeconds = 30;
        public const int MaxTimeoutSeconds = 300;
        public const int DefaultHistoryDays = 30;
        public const int MaxHistoryDays = 365;
    }

    /// <summary>
    /// Error codes for API responses.
    /// </summary>
    public static class ErrorCodes
    {
        public const string InvalidInput = "INVALID_INPUT";
        public const string ServiceNotFound = "SERVICE_NOT_FOUND";
        public const string PermissionDenied = "PERMISSION_DENIED";
        public const string OperationFailed = "OPERATION_FAILED";
        public const string TimeoutError = "TIMEOUT";
        public const string InternalError = "INTERNAL_ERROR";
        public const string DatabaseError = "DATABASE_ERROR";
        public const string ConnectionError = "CONNECTION_ERROR";
    }
}

/// <summary>
/// API configuration constants organized by feature.
/// </summary>
public static class FeatureDefaults
{
    /// <summary>
    /// Metrics collection defaults.
    /// </summary>
    public static class Metrics
    {
        public const int CollectionIntervalMs = 5000; // 5 seconds
        public const int HistoryRetentionHours = 24; // 1 day
        public const int AggregationIntervalMinutes = 5;
    }

    /// <summary>
    /// Health check defaults.
    /// </summary>
    public static class HealthCheck
    {
        public const int IntervalSeconds = 30;
        public const int TimeoutSeconds = 10;
        public const int FailureThreshold = 3;
    }

    /// <summary>
    /// Log collection defaults.
    /// </summary>
    public static class LogCollection
    {
        public const int BatchSizeBytes = 1024 * 1024; // 1 MB
        public const int RetentionDays = 7;
        public const int MaxEntriesPerRequest = 10000;
    }

    /// <summary>
    /// Background task defaults.
    /// </summary>
    public static class BackgroundTasks
    {
        public const int StatusUpdateIntervalSeconds = 30;
        public const int MetricsCollectionIntervalSeconds = 5;
        public const int LogCollectionIntervalSeconds = 60;
        public const int CleanupIntervalSeconds = 3600; // 1 hour
    }
}
