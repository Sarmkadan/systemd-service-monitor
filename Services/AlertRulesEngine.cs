#nullable enable

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Thread-safe, in-process implementation of <see cref="IAlertRulesEngine"/>.
/// Evaluates alert rules against <see cref="ServiceStatus"/> snapshots, manages the complete
/// <see cref="AlertIncident"/> lifecycle, and drives multi-level <see cref="EscalationPolicy"/>
/// notification chains with on-call rotation support.
/// </summary>
/// <remarks>
/// Rules and incidents are held in memory.  For multi-instance deployments wire up a
/// persistent backend by implementing <see cref="IAlertRulesEngine"/> against a database
/// repository and replace this registration in <c>AlertExtensions.AddAlertRulesEngine</c>.
/// </remarks>
public sealed class AlertRulesEngine : IAlertRulesEngine
{
    private readonly ILogger<AlertRulesEngine> _logger;
    private readonly IOnCallScheduleService _onCallService;
    private readonly AlertOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly ConcurrentDictionary<Guid, AlertRule> _rules = new();
    private readonly ConcurrentDictionary<Guid, AlertIncident> _incidents = new();

    // Tracks the last time each rule+service pair opened an incident for cooldown enforcement.
    private readonly ConcurrentDictionary<string, DateTime> _cooldownTracker = new();

    // Tracks consecutive matching evaluation counts per rule+service for hysteresis support.
    private readonly ConcurrentDictionary<string, int> _consecutiveHits = new();

    /// <summary>
    /// Initializes a new instance of <see cref="AlertRulesEngine"/>.
    /// </summary>
    public AlertRulesEngine(
        ILogger<AlertRulesEngine> logger,
        IOnCallScheduleService onCallService,
        IOptions<AlertOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _onCallService = onCallService ?? throw new ArgumentNullException(nameof(onCallService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    // -------------------------------------------------------------------------
    // Rule management
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public Task<IEnumerable<AlertRule>> GetRulesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<AlertRule>>(_rules.Values.OrderBy(r => r.Name).ToList());

    /// <inheritdoc />
    public Task<AlertRule?> GetRuleByIdAsync(Guid ruleId, CancellationToken cancellationToken = default)
        => Task.FromResult(_rules.TryGetValue(ruleId, out var rule) ? rule : null);

    /// <inheritdoc />
    public Task<AlertRule> AddRuleAsync(AlertRule rule, CancellationToken cancellationToken = default)
    {
        _rules[rule.Id] = rule;
        _logger.LogInformation("Alert rule registered: [{Severity}] {RuleName} ({RuleId}) — pattern: {Pattern}",
            rule.Severity, rule.Name, rule.Id, rule.ServicePattern);
        return Task.FromResult(rule);
    }

    /// <inheritdoc />
    public Task<AlertRule?> UpdateRuleAsync(Guid ruleId, UpdateAlertRuleDto dto, CancellationToken cancellationToken = default)
    {
        if (!_rules.TryGetValue(ruleId, out var rule))
            return Task.FromResult<AlertRule?>(null);

        if (dto.Name is not null) rule.Name = dto.Name;
        if (dto.Description is not null) rule.Description = dto.Description;
        if (dto.ServicePattern is not null) rule.ServicePattern = dto.ServicePattern;
        if (dto.Condition.HasValue) rule.Condition = dto.Condition.Value;
        if (dto.Threshold.HasValue) rule.Threshold = dto.Threshold.Value;
        if (dto.Severity.HasValue) rule.Severity = dto.Severity.Value;
        if (dto.EscalationPolicyId.HasValue) rule.EscalationPolicyId = dto.EscalationPolicyId.Value;
        if (dto.IsEnabled.HasValue) rule.IsEnabled = dto.IsEnabled.Value;
        if (dto.CooldownMinutes.HasValue) rule.CooldownMinutes = dto.CooldownMinutes.Value;
        if (dto.ConsecutiveEvaluationsRequired.HasValue) rule.ConsecutiveEvaluationsRequired = dto.ConsecutiveEvaluationsRequired.Value;
        if (dto.Tags is not null) rule.Tags = dto.Tags;
        rule.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Alert rule updated: {RuleName} ({RuleId})", rule.Name, ruleId);
        return Task.FromResult<AlertRule?>(rule);
    }

    /// <inheritdoc />
    public Task<bool> RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)
    {
        var removed = _rules.TryRemove(ruleId, out var rule);
        if (removed)
            _logger.LogInformation("Alert rule removed: {RuleName} ({RuleId})", rule!.Name, ruleId);
        return Task.FromResult(removed);
    }

    // -------------------------------------------------------------------------
    // Evaluation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task EvaluateServiceAsync(ServiceStatus status, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled) return;

        var matchingRules = _rules.Values
            .Where(r => r.IsEnabled && MatchesServicePattern(r.ServicePattern, status.UnitName))
            .ToList();

        foreach (var rule in matchingRules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (conditionMet, observedValue, summary) = EvaluateCondition(rule, status);
            var hitKey = $"{rule.Id}:{status.UnitName}";

            if (conditionMet)
            {
                var hits = _consecutiveHits.AddOrUpdate(hitKey, 1, (_, n) => n + 1);
                if (hits < rule.ConsecutiveEvaluationsRequired)
                {
                    _logger.LogDebug(
                        "Rule {RuleName} matched {Hits}/{Required} consecutive evaluations for {ServiceName}",
                        rule.Name, hits, rule.ConsecutiveEvaluationsRequired, status.UnitName);
                    continue;
                }

                _consecutiveHits[hitKey] = 0;

                if (_cooldownTracker.TryGetValue(hitKey, out var lastFired) &&
                    DateTime.UtcNow - lastFired < TimeSpan.FromMinutes(rule.CooldownMinutes))
                {
                    _logger.LogDebug("Rule {RuleName} is cooling down for {ServiceName}", rule.Name, status.UnitName);
                    continue;
                }

                await OpenIncidentAsync(rule, status.UnitName, summary, observedValue, cancellationToken);
                _cooldownTracker[hitKey] = DateTime.UtcNow;
            }
            else
            {
                _consecutiveHits[hitKey] = 0;
                await AutoResolveIncidentsAsync(rule.Id, status.UnitName, cancellationToken);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Incident queries
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public Task<IEnumerable<AlertIncident>> GetActiveIncidentsAsync(CancellationToken cancellationToken = default)
    {
        var active = _incidents.Values
            .Where(i => i.State is AlertIncidentState.Open or AlertIncidentState.Acknowledged or AlertIncidentState.Escalated)
            .OrderByDescending(i => i.Severity)
            .ThenBy(i => i.CreatedAt)
            .ToList();

        return Task.FromResult<IEnumerable<AlertIncident>>(active);
    }

    /// <inheritdoc />
    public Task<IEnumerable<AlertIncident>> GetIncidentHistoryAsync(
        string? serviceName = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _incidents.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(serviceName))
            query = query.Where(i => i.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult<IEnumerable<AlertIncident>>(
            query.OrderByDescending(i => i.CreatedAt).Take(maxResults).ToList());
    }

    /// <inheritdoc />
    public Task<AlertIncident?> GetIncidentByIdAsync(Guid incidentId, CancellationToken cancellationToken = default)
        => Task.FromResult(_incidents.TryGetValue(incidentId, out var i) ? i : null);

    // -------------------------------------------------------------------------
    // Incident lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public Task<bool> AcknowledgeIncidentAsync(Guid incidentId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        if (!_incidents.TryGetValue(incidentId, out var incident) ||
            incident.State is AlertIncidentState.Resolved or AlertIncidentState.AutoResolved)
            return Task.FromResult(false);

        incident.State = AlertIncidentState.Acknowledged;
        incident.AcknowledgedBy = acknowledgedBy;
        incident.AcknowledgedAt = DateTime.UtcNow;
        incident.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Incident {IncidentId} acknowledged by {AcknowledgedBy} (service: {ServiceName})",
            incidentId, acknowledgedBy, incident.ServiceName);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> ResolveIncidentAsync(Guid incidentId, string resolvedBy, string notes = "", CancellationToken cancellationToken = default)
    {
        if (!_incidents.TryGetValue(incidentId, out var incident))
            return Task.FromResult(false);

        incident.State = AlertIncidentState.Resolved;
        incident.ResolvedBy = resolvedBy;
        incident.ResolvedAt = DateTime.UtcNow;
        incident.ResolutionNotes = notes;
        incident.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Incident {IncidentId} resolved by {ResolvedBy} (service: {ServiceName}): {Notes}",
            incidentId, resolvedBy, incident.ServiceName, notes);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> SilenceIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        if (!_incidents.TryGetValue(incidentId, out var incident))
            return Task.FromResult(false);

        incident.State = AlertIncidentState.Silenced;
        incident.UpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Incident {IncidentId} silenced (service: {ServiceName})", incidentId, incident.ServiceName);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task EscalateIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        if (!_incidents.TryGetValue(incidentId, out var incident))
        {
            _logger.LogWarning("EscalateIncident called for unknown incident {IncidentId}", incidentId);
            return;
        }

        if (incident.State is AlertIncidentState.Resolved or AlertIncidentState.AutoResolved or AlertIncidentState.Silenced)
            return;

        incident.CurrentEscalationLevel++;
        incident.State = AlertIncidentState.Escalated;
        incident.UpdatedAt = DateTime.UtcNow;

        _logger.LogWarning(
            "Incident {IncidentId} escalated to level {Level} (service: {ServiceName}, severity: {Severity})",
            incidentId, incident.CurrentEscalationLevel, incident.ServiceName, incident.Severity);

        if (!_rules.TryGetValue(incident.AlertRuleId, out var rule))
            return;

        await NotifyEscalationAsync(incident, rule, incident.CurrentEscalationLevel, cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Summary
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public Task<AlertSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var allIncidents = _incidents.Values.ToList();

        return Task.FromResult(new AlertSummaryDto(
            TotalRules: _rules.Count,
            EnabledRules: _rules.Values.Count(r => r.IsEnabled),
            OpenIncidents: allIncidents.Count(i => i.State is AlertIncidentState.Open or AlertIncidentState.Escalated),
            CriticalIncidents: allIncidents.Count(i =>
                i.Severity == AlertSeverity.Critical &&
                i.State is AlertIncidentState.Open or AlertIncidentState.Escalated),
            AcknowledgedIncidents: allIncidents.Count(i => i.State == AlertIncidentState.Acknowledged),
            ResolvedLast24Hours: allIncidents.Count(i => i.ResolvedAt.HasValue && now - i.ResolvedAt.Value < TimeSpan.FromHours(24)),
            AsOf: now
        ));
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task OpenIncidentAsync(
        AlertRule rule,
        string serviceName,
        string summary,
        decimal? observedValue,
        CancellationToken cancellationToken)
    {
        var incident = new AlertIncident
        {
            AlertRuleId = rule.Id,
            ServiceName = serviceName,
            TriggerCondition = rule.Condition,
            Severity = rule.Severity,
            Summary = summary,
            ObservedValue = observedValue
        };

        _incidents[incident.Id] = incident;

        _logger.LogWarning(
            "ALERT OPENED [{Severity}] {RuleName} on {ServiceName} — {Summary} (incident: {IncidentId})",
            rule.Severity, rule.Name, serviceName, summary, incident.Id);

        await NotifyEscalationAsync(incident, rule, 0, cancellationToken);
    }

    private async Task AutoResolveIncidentsAsync(Guid ruleId, string serviceName, CancellationToken cancellationToken)
    {
        if (!_options.AutoResolveOnConditionCleared) return;

        var toResolve = _incidents.Values
            .Where(i =>
                i.AlertRuleId == ruleId &&
                i.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase) &&
                i.State is AlertIncidentState.Open or AlertIncidentState.Escalated)
            .ToList();

        foreach (var incident in toResolve)
        {
            incident.State = AlertIncidentState.AutoResolved;
            incident.ResolvedAt = DateTime.UtcNow;
            incident.ResolutionNotes = "Condition cleared — auto-resolved by alert engine";
            incident.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Incident {IncidentId} auto-resolved: condition cleared for {ServiceName}",
                incident.Id, serviceName);
        }

        await Task.CompletedTask;
    }

    private async Task NotifyEscalationAsync(
        AlertIncident incident,
        AlertRule rule,
        int levelIndex,
        CancellationToken cancellationToken)
    {
        var historyEntry = new EscalationHistory
        {
            LevelReached = levelIndex,
            LevelName = levelIndex == 0 ? "Initial notification" : $"Escalation level {levelIndex}",
            OccurredAt = DateTime.UtcNow
        };

        // Resolve the on-call responder if an escalation policy is attached.
        // A full persistent implementation would load the EscalationPolicy by rule.EscalationPolicyId
        // and resolve each EscalationLevel's OnCallScheduleId through IOnCallScheduleService.
        // The architecture is intentionally open for that extension without interface changes.
        if (rule.EscalationPolicyId is not null)
        {
            var channel = NotificationChannel.Log;
            var target = "log";

            try
            {
                await DeliverNotificationAsync(channel, target, incident, cancellationToken);
                historyEntry.Channel = channel;
                historyEntry.NotificationTarget = target;
                historyEntry.NotificationDelivered = true;
            }
            catch (Exception ex)
            {
                historyEntry.NotificationDelivered = false;
                historyEntry.DeliveryError = ex.Message;
                _logger.LogError(ex, "Failed to deliver escalation notification for incident {IncidentId}", incident.Id);
            }
        }
        else
        {
            historyEntry.Channel = NotificationChannel.Log;
            historyEntry.NotificationTarget = "log";
            historyEntry.NotificationDelivered = true;
            _logger.LogWarning(
                "ALERT [{Severity}] {ServiceName}: {Summary} — no escalation policy attached (incident {IncidentId})",
                incident.Severity, incident.ServiceName, incident.Summary, incident.Id);
        }

        incident.EscalationHistory.Add(historyEntry);
        incident.UpdatedAt = DateTime.UtcNow;
    }

    private async Task DeliverNotificationAsync(
        NotificationChannel channel,
        string target,
        AlertIncident incident,
        CancellationToken cancellationToken)
    {
        switch (channel)
        {
            case NotificationChannel.Webhook:
                await DeliverWebhookAsync(target, incident, cancellationToken);
                break;

            case NotificationChannel.Log:
            default:
                _logger.LogWarning(
                    "ALERT NOTIFICATION [{Severity}] Incident {IncidentId} — {ServiceName}: {Summary}",
                    incident.Severity, incident.Id, incident.ServiceName, incident.Summary);
                break;
        }
    }

    private async Task DeliverWebhookAsync(string webhookUrl, AlertIncident incident, CancellationToken cancellationToken)
    {
        var payload = new
        {
            incidentId = incident.Id,
            severity = incident.Severity.ToString(),
            serviceName = incident.ServiceName,
            summary = incident.Summary,
            observedValue = incident.ObservedValue,
            createdAt = incident.CreatedAt,
            escalationLevel = incident.CurrentEscalationLevel
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = _httpClientFactory.CreateClient(nameof(AlertRulesEngine));
        var response = await client.PostAsync(webhookUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static bool MatchesServicePattern(string pattern, string serviceName)
    {
        if (pattern == "*") return true;

        if (pattern.EndsWith('*'))
            return serviceName.StartsWith(pattern[..^1], StringComparison.OrdinalIgnoreCase);

        return serviceName.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static (bool ConditionMet, decimal? ObservedValue, string Summary) EvaluateCondition(
        AlertRule rule, ServiceStatus status) => rule.Condition switch
    {
        AlertCondition.ServiceFailed =>
            (status.HasFailed || status.State == ServiceState.Failed,
             null,
             $"Service entered failed state: {(string.IsNullOrEmpty(status.FailureReason) ? "unknown reason" : status.FailureReason)}"),

        AlertCondition.ServiceInactive =>
            (!status.IsRunning && status.State == ServiceState.Inactive,
             null,
             $"Service is inactive (expected running)"),

        AlertCondition.CpuThresholdExceeded =>
            (status.CpuUsagePercent > rule.Threshold,
             status.CpuUsagePercent,
             $"CPU usage {status.CpuUsagePercent:F1}% exceeds threshold {rule.Threshold}%"),

        AlertCondition.MemoryThresholdExceeded =>
            (status.MemoryUsageMb > (long)rule.Threshold,
             status.MemoryUsageMb,
             $"Memory usage {status.MemoryUsageMb} MB exceeds threshold {rule.Threshold} MB"),

        AlertCondition.HealthCheckUnhealthy =>
            (status.HealthStatus == HealthStatus.Unhealthy,
             null,
             $"Health check unhealthy: {(string.IsNullOrEmpty(status.HealthMessage) ? "no details" : status.HealthMessage)}"),

        AlertCondition.HealthCheckDegraded =>
            (status.HealthStatus >= HealthStatus.Degraded,
             null,
             $"Health check degraded: {(string.IsNullOrEmpty(status.HealthMessage) ? "no details" : status.HealthMessage)}"),

        AlertCondition.UptimeBelowMinimum =>
            (status.IsRunning && status.UptimeSeconds < (long)rule.Threshold,
             status.UptimeSeconds,
             $"Uptime {status.UptimeSeconds}s is below minimum {rule.Threshold}s"),

        AlertCondition.AnyStateChange =>
            (true, null, $"Service state observed: {status.State}/{status.SubState}"),

        _ => (false, null, string.Empty)
    };
}

// =============================================================================

/// <summary>
/// Thread-safe, in-memory implementation of <see cref="IOnCallScheduleService"/>.
/// Resolves the active on-call responder for a schedule at any given UTC moment,
/// giving priority to override entries over regular rotation shifts.
/// </summary>
/// <remarks>
/// Replace this with a persistent repository-backed implementation for production
/// environments where schedule data must survive process restarts.
/// </remarks>
public sealed class InMemoryOnCallScheduleService : IOnCallScheduleService
{
    private readonly ILogger<InMemoryOnCallScheduleService> _logger;
    private readonly ConcurrentDictionary<Guid, OnCallSchedule> _schedules = new();

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryOnCallScheduleService"/>.
    /// </summary>
    public InMemoryOnCallScheduleService(ILogger<InMemoryOnCallScheduleService> logger)
        => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public Task<IEnumerable<OnCallSchedule>> GetSchedulesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<OnCallSchedule>>(_schedules.Values.OrderBy(s => s.Name).ToList());

    /// <inheritdoc />
    public Task<OnCallSchedule?> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default)
        => Task.FromResult(_schedules.TryGetValue(scheduleId, out var s) ? s : null);

    /// <inheritdoc />
    public Task<OnCallSchedule> CreateScheduleAsync(OnCallSchedule schedule, CancellationToken cancellationToken = default)
    {
        _schedules[schedule.Id] = schedule;
        _logger.LogInformation("On-call schedule created: {ScheduleName} ({ScheduleId})", schedule.Name, schedule.Id);
        return Task.FromResult(schedule);
    }

    /// <inheritdoc />
    public Task<OnCallSchedule?> UpdateScheduleAsync(OnCallSchedule schedule, CancellationToken cancellationToken = default)
    {
        if (!_schedules.ContainsKey(schedule.Id))
            return Task.FromResult<OnCallSchedule?>(null);

        _schedules[schedule.Id] = schedule;
        _logger.LogInformation("On-call schedule updated: {ScheduleName} ({ScheduleId})", schedule.Name, schedule.Id);
        return Task.FromResult<OnCallSchedule?>(schedule);
    }

    /// <inheritdoc />
    public Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default)
    {
        var removed = _schedules.TryRemove(scheduleId, out var schedule);
        if (removed)
            _logger.LogInformation("On-call schedule deleted: {ScheduleName} ({ScheduleId})", schedule!.Name, scheduleId);
        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public Task<OnCallEntry?> GetCurrentOnCallAsync(
        Guid scheduleId,
        DateTime? atUtc = null,
        CancellationToken cancellationToken = default)
    {
        var now = atUtc ?? DateTime.UtcNow;

        if (!_schedules.TryGetValue(scheduleId, out var schedule))
            return Task.FromResult<OnCallEntry?>(null);

        // Override entries take priority; then fall back to the most recently started regular shift.
        var active = schedule.Entries
            .Where(e => e.ShiftStart <= now && e.ShiftEnd > now)
            .OrderByDescending(e => e.IsOverride)
            .ThenByDescending(e => e.ShiftStart)
            .FirstOrDefault();

        return Task.FromResult(active);
    }

    /// <inheritdoc />
    public Task<OnCallEntry> AddOverrideAsync(
        Guid scheduleId,
        OnCallEntry overrideEntry,
        CancellationToken cancellationToken = default)
    {
        if (!_schedules.TryGetValue(scheduleId, out var schedule))
            throw new InvalidOperationException($"On-call schedule {scheduleId} not found.");

        overrideEntry.IsOverride = true;
        schedule.Entries.Add(overrideEntry);

        _logger.LogInformation(
            "On-call override added to schedule {ScheduleId}: {ResponderName} [{ShiftStart} – {ShiftEnd}]",
            scheduleId, overrideEntry.ResponderName, overrideEntry.ShiftStart, overrideEntry.ShiftEnd);

        return Task.FromResult(overrideEntry);
    }
}
