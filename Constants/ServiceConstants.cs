#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Constants;

/// <summary>
/// Constants used throughout the systemd service monitoring application.
/// </summary>
public static class ServiceConstants
{
    public const string DefaultServiceBusPath = "org.freedesktop.systemd1";
    public const string DefaultServiceBusObjectPath = "/org/freedesktop/systemd1";
    public const string ServiceManagerInterface = "org.freedesktop.systemd1.Manager";
    public const string UnitInterface = "org.freedesktop.systemd1.Unit";
    public const string ServiceInterface = "org.freedesktop.systemd1.Service";

    public const int DefaultMetricCollectionIntervalMs = 5000;
    public const int MaxLogEntriesPerRequest = 1000;
    public const int DefaultLogRetentionDays = 30;

    public const string SystemdServiceNameSuffix = ".service";
    public const string SystemdTimerNameSuffix = ".timer";
    public const string SystemdSocketNameSuffix = ".socket";

    public static class DBusSignals
    {
        public const string PropertiesChanged = "PropertiesChanged";
        public const string UnitNew = "UnitNew";
        public const string UnitRemoved = "UnitRemoved";
    }

    public static class HealthCheckPatterns
    {
        public const string HttpGetPattern = "GET";
        public const string HttpPostPattern = "POST";
        public const int DefaultTimeoutSeconds = 10;
        public const int DefaultIntervalSeconds = 30;
    }
}
