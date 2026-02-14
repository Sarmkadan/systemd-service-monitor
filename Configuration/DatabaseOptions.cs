#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Configuration options for database connections.
/// Supports multiple database types and connection scenarios.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Database provider type (InMemory, SQLite, PostgreSQL, SqlServer, MySql).
    /// </summary>
    public string Provider { get; set; } = "InMemory";

    /// <summary>
    /// Connection string for the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Database name.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Host server address.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Port number for the database server.
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of connection pool connections.
    /// </summary>
    public int MaxPoolSize { get; set; } = 20;

    /// <summary>
    /// Enable connection pooling.
    /// </summary>
    public bool UseConnectionPooling { get; set; } = true;

    /// <summary>
    /// Enable SSL/TLS for connections.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Enable automatic migrations on startup.
    /// </summary>
    public bool EnableAutoMigration { get; set; } = false;

    /// <summary>
    /// Log SQL commands (for debugging).
    /// </summary>
    public bool LogSqlCommands { get; set; } = false;

    /// <summary>
    /// Builds the connection string from individual components.
    /// </summary>
    public string BuildConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            return ConnectionString;
        }

        return Provider.ToLower() switch
        {
            "sqlite" => $"Data Source={DatabaseName ?? "systemd-monitor.db"}",
            "postgresql" => BuildPostgreSqlConnectionString(),
            "sqlserver" => BuildSqlServerConnectionString(),
            "mysql" => BuildMySqlConnectionString(),
            _ => "Data Source=:memory:" // InMemory
        };
    }

    private string BuildPostgreSqlConnectionString()
    {
        var cs = $"Host={Host ?? "localhost"};Port={Port};";
        if (!string.IsNullOrWhiteSpace(Username))
            cs += $"Username={Username};";
        if (!string.IsNullOrWhiteSpace(Password))
            cs += $"Password={Password};";
        if (!string.IsNullOrWhiteSpace(DatabaseName))
            cs += $"Database={DatabaseName};";

        cs += $"Command Timeout={CommandTimeoutSeconds};";

        if (UseSsl)
            cs += "SSL Mode=Require;";

        return cs;
    }

    private string BuildSqlServerConnectionString()
    {
        var cs = $"Server={Host ?? "localhost"},{Port};";
        if (!string.IsNullOrWhiteSpace(DatabaseName))
            cs += $"Database={DatabaseName};";
        if (!string.IsNullOrWhiteSpace(Username))
            cs += $"User Id={Username};";
        if (!string.IsNullOrWhiteSpace(Password))
            cs += $"Password={Password};";

        cs += $"Connection Timeout={CommandTimeoutSeconds};";

        if (!UseConnectionPooling)
            cs += "Pooling=false;";

        return cs;
    }

    private string BuildMySqlConnectionString()
    {
        var cs = $"server={Host ?? "localhost"};port={Port};";
        if (!string.IsNullOrWhiteSpace(Username))
            cs += $"uid={Username};";
        if (!string.IsNullOrWhiteSpace(Password))
            cs += $"pwd={Password};";
        if (!string.IsNullOrWhiteSpace(DatabaseName))
            cs += $"database={DatabaseName};";

        cs += $"connection timeout={CommandTimeoutSeconds};";

        if (UseSsl)
            cs += "SslMode=Required;";

        return cs;
    }
}
