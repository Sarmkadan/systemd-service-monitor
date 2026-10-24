#nullable enable

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Settings for the Prometheus metrics exporter, bound from the <c>PrometheusExporter</c> section.
/// </summary>
/// <remarks>
/// Validated on start by <see cref="PrometheusExporterOptionsValidator"/>; an invalid value aborts host startup.
/// </remarks>
public sealed class PrometheusExporterOptions
{
    /// <summary>Configuration section key used during DI registration.</summary>
    public const string SectionName = "PrometheusExporter";

    /// <summary>TCP port the exporter listens on. Must be between 1 and 65535.</summary>
    public int Port { get; set; } = 9100;

    /// <summary>HTTP path that serves the metrics. Must start with <c>/</c>.</summary>
    public string MetricsPath { get; set; } = "/metrics";

    /// <summary>Prefix added to every exported metric name. Must match <c>[a-zA-Z_:][a-zA-Z0-9_:]*</c>.</summary>
    public string MetricPrefix { get; set; } = "systemd_";
}
