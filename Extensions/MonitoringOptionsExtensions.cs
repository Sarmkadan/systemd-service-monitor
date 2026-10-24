#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SystemdServiceMonitor.Configuration;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for registering validated monitoring and exporter options.
/// </summary>
public static class MonitoringOptionsExtensions
{
    /// <summary>
    /// Binds and validates <see cref="PrometheusExporterOptions"/> and <see cref="ServiceMonitorOptions"/>.
    /// </summary>
    /// <remarks>
    /// Both options are validated with <c>ValidateOnStart</c>, so an invalid configuration aborts host startup
    /// with a message naming each offending property.
    /// </remarks>
    /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration"><see cref="IConfiguration"/> providing application configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddMonitoringOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<PrometheusExporterOptions>()
            .Bind(configuration.GetSection(PrometheusExporterOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PrometheusExporterOptions>, PrometheusExporterOptionsValidator>();

        services.AddOptions<ServiceMonitorOptions>()
            .Bind(configuration.GetSection(ServiceMonitorOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ServiceMonitorOptions>, ServiceMonitorOptionsValidator>();

        return services;
    }
}
