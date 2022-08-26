#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Exceptions;

/// <summary>
/// Base exception for all systemd service monitor related errors.
/// </summary>
public class ServiceMonitorException : Exception
{
    public ServiceMonitorException(string message) : base(message) { }

    public ServiceMonitorException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when a requested service is not found in the system.
/// </summary>
public class ServiceNotFoundException : ServiceMonitorException
{
    public string ServiceName { get; }

    public ServiceNotFoundException(string serviceName)
        : base($"Service '{serviceName}' not found")
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Thrown when D-Bus connection cannot be established or maintained.
/// </summary>
public class DBusConnectionException : ServiceMonitorException
{
    public string? BusName { get; }

    public DBusConnectionException(string message)
        : base($"D-Bus connection error: {message}") { }

    public DBusConnectionException(string message, string busName)
        : base($"D-Bus connection error: {message}")
    {
        BusName = busName;
    }

    public DBusConnectionException(string message, Exception innerException)
        : base($"D-Bus connection error: {message}", innerException) { }
}

/// <summary>
/// Thrown when the application lacks required permissions to perform an operation.
/// </summary>
public class InsufficientPermissionsException : ServiceMonitorException
{
    public string RequiredPermission { get; }

    public InsufficientPermissionsException(string requiredPermission)
        : base($"Insufficient permissions. Required: {requiredPermission}")
    {
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Thrown when a service operation fails (start, stop, restart).
/// </summary>
public class ServiceOperationException : ServiceMonitorException
{
    public string ServiceName { get; }
    public string Operation { get; }

    public ServiceOperationException(string serviceName, string operation, string message)
        : base($"Operation '{operation}' on service '{serviceName}' failed: {message}")
    {
        ServiceName = serviceName;
        Operation = operation;
    }

    public ServiceOperationException(string serviceName, string operation, string message, Exception innerException)
        : base($"Operation '{operation}' on service '{serviceName}' failed: {message}", innerException)
    {
        ServiceName = serviceName;
        Operation = operation;
    }
}

/// <summary>
/// Thrown when a log operation fails (read, query, export).
/// </summary>
public class LogAccessException : ServiceMonitorException
{
    public LogAccessException(string message) : base($"Log access error: {message}") { }

    public LogAccessException(string message, Exception innerException)
        : base($"Log access error: {message}", innerException) { }
}
