#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Extensions;

/// <summary>
/// Extension methods for registering the alert rules engine, on-call schedule service,
/// and the background escalation worker into the ASP.NET Core DI container.
/// </summary>
public static class AlertExtensions
{
    /// <summary>
    /// Registers all alert rules engine dependencies:
    /// <list type="bullet">
    ///   <item><see cref="IAlertRulesEngine"/> / <see cref="AlertRulesEngine"/> — singleton engine</item>
    ///   <item><see cref="IOnCallScheduleService"/> / <see cref="InMemoryOnCallScheduleService"/> — singleton schedule store</item>
    ///   <item><see cref="AlertEscalationWorker"/> — hosted background service for periodic evaluation and escalation</item>
    ///   <item><see cref="AlertOptions"/> bound from the <c>Alerts</c> configuration section</item>
    ///   <item>Named <see cref="HttpClient"/> for outbound webhook delivery</item>
    /// </list>
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">The application configuration root.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAlertRulesEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AlertOptions>(configuration.GetSection(AlertOptions.SectionName));

        services.AddSingleton<IOnCallScheduleService, InMemoryOnCallScheduleService>();
        services.AddSingleton<IAlertRulesEngine, AlertRulesEngine>();

        services.AddHttpClient(nameof(AlertRulesEngine), (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AlertOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(opts.Webhook.TimeoutSeconds);

            foreach (var (key, value) in opts.Webhook.DefaultHeaders)
                client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        });

        services.AddHostedService<AlertEscalationWorker>();

        return services;
    }
}

/// <summary>
/// Long-running background service that drives two periodic responsibilities:
/// <list type="number">
///   <item>
///     <b>Service evaluation</b> — resolves the current status of all monitored services
///     via <see cref="IServiceMonitorService"/> and feeds each snapshot into
///     <see cref="IAlertRulesEngine.EvaluateServiceAsync"/>.
///   </item>
///   <item>
///     <b>Escalation promotion</b> — inspects open incidents whose last escalation
///     timestamp is older than the policy's configured delay and advances them to the
///     next escalation level.
///   </item>
/// </list>
/// </summary>
public sealed class AlertEscalationWorker : BackgroundService
{
    private readonly ILogger<AlertEscalationWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAlertRulesEngine _alertEngine;
    private readonly AlertOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="AlertEscalationWorker"/>.
    /// </summary>
    public AlertEscalationWorker(
        ILogger<AlertEscalationWorker> logger,
        IServiceScopeFactory scopeFactory,
        IAlertRulesEngine alertEngine,
        IOptions<AlertOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _alertEngine = alertEngine ?? throw new ArgumentNullException(nameof(alertEngine));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Alert rules engine is disabled — escalation worker will not run");
            return;
        }

        _logger.LogInformation("Alert escalation worker starting (startup delay: {Delay}s)",
            _options.StartupDelaySeconds);

        await Task.Delay(TimeSpan.FromSeconds(_options.StartupDelaySeconds), stoppingToken);

        _logger.LogInformation(
            "Alert escalation worker active — evaluation every {EvalInterval}s, escalation check every {EscInterval}s",
            _options.ServiceEvaluationIntervalSeconds, _options.EscalationCheckIntervalSeconds);

        var evaluationTimer = TimeSpan.FromSeconds(_options.ServiceEvaluationIntervalSeconds);
        var escalationTimer = TimeSpan.FromSeconds(_options.EscalationCheckIntervalSeconds);
        var lastEscalationCheck = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateServicesAsync(stoppingToken);

                if (DateTime.UtcNow - lastEscalationCheck >= escalationTimer)
                {
                    await PromoteDueEscalationsAsync(stoppingToken);
                    lastEscalationCheck = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert escalation worker encountered an error; resuming after back-off");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                continue;
            }

            await Task.Delay(evaluationTimer, stoppingToken);
        }

        _logger.LogInformation("Alert escalation worker stopped");
    }

    // -------------------------------------------------------------------------
    // Periodic tasks
    // -------------------------------------------------------------------------

    private async Task EvaluateServicesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var monitorService = scope.ServiceProvider.GetService<IServiceMonitorService>();

        if (monitorService is null)
        {
            _logger.LogDebug("IServiceMonitorService not registered — skipping service evaluation pass");
            return;
        }

        IEnumerable<ServiceInfo> services;
        try
        {
            services = await monitorService.GetAllServicesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve services for alert evaluation");
            return;
        }

        foreach (var service in services)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await monitorService.GetServiceStatusAsync(service.UnitName, cancellationToken);
            if (status is null) continue;

            try
            {
                await _alertEngine.EvaluateServiceAsync(status, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating alert rules for service {ServiceName}", service.UnitName);
            }
        }
    }

    private async Task PromoteDueEscalationsAsync(CancellationToken cancellationToken)
    {
        var activeIncidents = await _alertEngine.GetActiveIncidentsAsync(cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var incident in activeIncidents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Acknowledged incidents are paused; silenced incidents are suppressed.
            if (incident.State is AlertIncidentState.Acknowledged or AlertIncidentState.Silenced)
                continue;

            // Determine when the last escalation event occurred.
            var lastActionAt = incident.EscalationHistory.Count > 0
                ? incident.EscalationHistory.Max(h => h.OccurredAt)
                : incident.CreatedAt;

            var delayMinutes = incident.CurrentEscalationLevel == 0
                ? _options.EscalationDefaults.InitialEscalationDelayMinutes
                : _options.EscalationDefaults.SubsequentEscalationDelayMinutes;

            if (incident.CurrentEscalationLevel >= _options.EscalationDefaults.MaxEscalationLevels)
            {
                _logger.LogDebug(
                    "Incident {IncidentId} has reached the maximum escalation level ({Max}); no further promotion",
                    incident.Id, _options.EscalationDefaults.MaxEscalationLevels);
                continue;
            }

            if ((now - lastActionAt).TotalMinutes >= delayMinutes)
            {
                _logger.LogInformation(
                    "Incident {IncidentId} due for escalation after {Minutes} minutes unacknowledged",
                    incident.Id, (int)(now - lastActionAt).TotalMinutes);

                try
                {
                    await _alertEngine.EscalateIncidentAsync(incident.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error escalating incident {IncidentId}", incident.Id);
                }
            }
        }
    }
}
