#nullable enable

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Configuration options for systemd D-Bus integration.
/// </summary>
public class SystemdOptions
{
    public bool EnableMonitoring { get; set; } = true;
    public int MetricCollectionIntervalMs { get; set; } = 5000;
    public int LogRetentionDays { get; set; } = 30;
    public int MaxLogEntriesPerRequest { get; set; } = 1000;
    public bool EnableRemoteOperations { get; set; } = true;
    public int OperationTimeoutMs { get; set; } = 30000;
    public int ConnectionRetryCount { get; set; } = 5;
    public int ConnectionRetryDelayMs { get; set; } = 2000;
    public bool EnableHealthChecks { get; set; } = true;
}

/// <summary>
/// Configuration options for data persistence and repository operations.
/// </summary>
public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = "InMemory";
    public bool EnableLogging { get; set; } = false;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public int MaxConnectionPoolSize { get; set; } = 20;
    public int QueryCacheExpirationMinutes { get; set; } = 5;
}

/// <summary>
/// Configuration for Swagger/OpenAPI documentation.
/// </summary>
public class SwaggerOptions
{
    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = "systemd Service Monitor API";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "REST API for monitoring and managing systemd services via D-Bus";
}
