#nullable enable

using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Core contract for the alert rules engine.  The engine evaluates <see cref="AlertRule"/>
/// conditions against incoming <see cref="ServiceStatus"/> snapshots, manages the complete
/// <see cref="AlertIncident"/> lifecycle, and drives multi-level escalation policies with
/// on-call rotation support.
/// </summary>
public interface IAlertRulesEngine
{
    // -------------------------------------------------------------------------
    // Rule management
    // -------------------------------------------------------------------------

    /// <summary>Returns all configured alert rules in name order.</summary>
    Task<IEnumerable<AlertRule>> GetRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single rule by its unique identifier, or <c>null</c> if not found.</summary>
    Task<AlertRule?> GetRuleByIdAsync(Guid ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new alert rule so it participates in subsequent evaluations.
    /// Returns the persisted rule (including any server-assigned defaults).
    /// </summary>
    Task<AlertRule> AddRuleAsync(AlertRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to an existing rule using the non-null fields from
    /// <paramref name="dto"/>.  Returns the updated rule, or <c>null</c> if not found.
    /// </summary>
    Task<AlertRule?> UpdateRuleAsync(Guid ruleId, UpdateAlertRuleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a rule from the engine.  Active incidents opened by this rule are not
    /// affected and continue their normal lifecycle.  Returns <c>true</c> if removed.
    /// </summary>
    Task<bool> RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Evaluation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Evaluates all enabled rules whose <see cref="AlertRule.ServicePattern"/> matches
    /// <paramref name="status"/>.  Opens new incidents for matching conditions and
    /// auto-resolves existing incidents when the underlying condition clears.
    /// </summary>
    Task EvaluateServiceAsync(ServiceStatus status, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Incident queries
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns all incidents currently in <see cref="AlertIncidentState.Open"/>,
    /// <see cref="AlertIncidentState.Acknowledged"/>, or <see cref="AlertIncidentState.Escalated"/>
    /// state, ordered by severity descending then creation time ascending.
    /// </summary>
    Task<IEnumerable<AlertIncident>> GetActiveIncidentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the full incident history, optionally filtered to a specific service name.
    /// Results are ordered by creation time descending and capped at <paramref name="maxResults"/>.
    /// </summary>
    Task<IEnumerable<AlertIncident>> GetIncidentHistoryAsync(
        string? serviceName = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a single incident by its unique identifier, or <c>null</c> if not found.</summary>
    Task<AlertIncident?> GetIncidentByIdAsync(Guid incidentId, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Incident lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Acknowledges an open incident on behalf of <paramref name="acknowledgedBy"/>,
    /// pausing automatic escalation.  Returns <c>false</c> if the incident does not exist
    /// or is already resolved.
    /// </summary>
    Task<bool> AcknowledgeIncidentAsync(Guid incidentId, string acknowledgedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an incident as resolved with optional root-cause notes.
    /// Returns <c>false</c> if the incident does not exist.
    /// </summary>
    Task<bool> ResolveIncidentAsync(Guid incidentId, string resolvedBy, string notes = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Silences an incident without formally resolving it.  Silenced incidents are
    /// excluded from active-incident queries and do not trigger further escalation.
    /// Returns <c>false</c> if the incident does not exist.
    /// </summary>
    Task<bool> SilenceIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Immediately advances an incident to the next escalation level, regardless of
    /// the policy's configured delay.  Useful for manual escalation by a responder.
    /// </summary>
    Task EscalateIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default);

    // -------------------------------------------------------------------------
    // Summary
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns an aggregated <see cref="AlertSummaryDto"/> suitable for dashboard widgets,
    /// capturing rule counts and incident state counts at the current moment.
    /// </summary>
    Task<AlertSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Contract for managing on-call rotation schedules and resolving the current active
/// responder at any point in time.  Used by the alert engine to route notifications
/// to the right person during escalation.
/// </summary>
public interface IOnCallScheduleService
{
    /// <summary>Returns all defined on-call schedules in name order.</summary>
    Task<IEnumerable<OnCallSchedule>> GetSchedulesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a schedule by its unique identifier, or <c>null</c> if not found.</summary>
    Task<OnCallSchedule?> GetScheduleByIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and persists a new on-call schedule.
    /// Returns the persisted schedule (including any server-assigned defaults).
    /// </summary>
    Task<OnCallSchedule> CreateScheduleAsync(OnCallSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing schedule in full.
    /// Returns the updated schedule, or <c>null</c> if not found.
    /// </summary>
    Task<OnCallSchedule?> UpdateScheduleAsync(OnCallSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>Deletes a schedule by its unique identifier.  Returns <c>true</c> if deleted.</summary>
    Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the <see cref="OnCallEntry"/> whose shift window covers <paramref name="atUtc"/>
    /// (defaults to <see cref="DateTime.UtcNow"/>).  Override entries take priority over
    /// regular rotation entries.  Returns <c>null</c> when no entry covers the requested time.
    /// </summary>
    Task<OnCallEntry?> GetCurrentOnCallAsync(
        Guid scheduleId,
        DateTime? atUtc = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a one-time override entry that supersedes any overlapping regular entries
    /// for the specified time window.  Returns the persisted override entry.
    /// </summary>
    Task<OnCallEntry> AddOverrideAsync(
        Guid scheduleId,
        OnCallEntry overrideEntry,
        CancellationToken cancellationToken = default);
}
