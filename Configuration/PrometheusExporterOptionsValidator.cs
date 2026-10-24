#nullable enable

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace SystemdServiceMonitor.Configuration;

/// <summary>
/// Validates <see cref="PrometheusExporterOptions"/> at startup.
/// </summary>
/// <remarks>
/// Every problem is collected into a single <see cref="ValidateOptionsResult"/>. Each message names the
/// offending property using its configuration key (for example <c>PrometheusExporter:Port</c>).
/// </remarks>
public sealed class PrometheusExporterOptionsValidator : IValidateOptions<PrometheusExporterOptions>
{
    private const string Section = PrometheusExporterOptions.SectionName;

    // Prometheus metric name grammar: [a-zA-Z_:][a-zA-Z0-9_:]*. \z rejects a trailing newline that '$' would allow.
    private static readonly Regex MetricPrefixPattern = new(@"^[a-zA-Z_:][a-zA-Z0-9_:]*\z", RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, PrometheusExporterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (options.Port is < 1 or > 65535)
        {
            failures.Add($"{Section}:Port must be between 1 and 65535, but was {options.Port}.");
        }

        if (string.IsNullOrEmpty(options.MetricsPath) || !options.MetricsPath.StartsWith('/'))
        {
            failures.Add($"{Section}:MetricsPath must start with '/', but was '{options.MetricsPath}'.");
        }

        if (string.IsNullOrEmpty(options.MetricPrefix) || !MetricPrefixPattern.IsMatch(options.MetricPrefix))
        {
            failures.Add($"{Section}:MetricPrefix must match Prometheus naming rules ([a-zA-Z_:][a-zA-Z0-9_:]*), but was '{options.MetricPrefix}'.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
