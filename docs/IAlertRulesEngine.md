# `IAlertRulesEngine`

`IAlertRulesEngine` is the asynchronous contract for storing alert rules, evaluating `ServiceStatus` snapshots, managing alert incidents, and producing alert summary counts. The default `AlertRulesEngine` implementation is a singleton, thread-safe, in-process engine backed by concurrent dictionaries.

The interface is defined in `Services/IAlertRulesEngine.cs`; its default implementation is in `Services/AlertRulesEngine.cs`.

## Registration and lifetime

`AddAlertRulesEngine(configuration)` registers `IAlertRulesEngine` as a singleton implemented by `AlertRulesEngine`. It also registers the in-memory on-call schedule service, a named HTTP client, alert options, and the hosted escalation worker.

Because rules, incidents, cooldown timestamps, and consecutive-hit counters are held only in memory, they are lost when the process exits and are not shared between application instances. A durable or distributed deployment should provide another `IAlertRulesEngine` implementation.

`AlertRulesEngine` requires:

- `ILogger<AlertRulesEngine>` for rule, incident, and notification events.
- `IOnCallScheduleService` for the alerting architecture's on-call integration. The current engine retains this dependency but does not yet resolve escalation policies or schedules during delivery.
- `IOptions<AlertOptions>` for the engine switch and auto-resolution behavior.
- `IHttpClientFactory` for webhook delivery support.

The constructor throws `ArgumentNullException` when any dependency, or the options value, is null.

## Rule management contract

### `GetRulesAsync(CancellationToken cancellationToken = default)`

Returns a snapshot of all stored rules ordered by `Name` using the default string ordering. The returned collection contains the stored mutable `AlertRule` instances, not copies. The in-memory implementation completes synchronously and does not inspect the cancellation token.

### `GetRuleByIdAsync(Guid ruleId, CancellationToken cancellationToken = default)`

Returns the rule with the supplied ID, or `null` when no rule exists. `Guid.Empty` has no special validation. The cancellation token is not inspected by the default implementation.

### `AddRuleAsync(AlertRule rule, CancellationToken cancellationToken = default)`

Stores and returns `rule`. A null rule causes `ArgumentNullException`. If its ID already exists, the current implementation replaces the existing value; it does not report a duplicate. It does not assign a new ID, clone the rule, or inspect the cancellation token.

### `UpdateRuleAsync(Guid ruleId, UpdateAlertRuleDto dto, CancellationToken cancellationToken = default)`

Applies a partial update to the stored rule. Only non-null DTO fields are applied, and `UpdatedAt` is set to `DateTime.UtcNow`. The method returns the same mutable rule instance, or `null` when the ID is unknown. A null DTO causes `ArgumentNullException`; the cancellation token is not inspected.

The updateable fields are `Name`, `Description`, `ServicePattern`, `Condition`, `Threshold`, `Severity`, `EscalationPolicyId`, `IsEnabled`, `CooldownMinutes`, `ConsecutiveEvaluationsRequired`, and `Tags`. Because nullable DTO fields mean "not supplied," this method cannot clear an existing `EscalationPolicyId` to `null`.

### `RemoveRuleAsync(Guid ruleId, CancellationToken cancellationToken = default)`

Removes the rule and returns `true`, or returns `false` when it is absent. Removing a rule does not remove its existing incidents or related cooldown and hit-tracking entries. The cancellation token is not inspected.

## Evaluation contract

### `EvaluateServiceAsync(ServiceStatus status, CancellationToken cancellationToken = default)`

Evaluates one service snapshot against every enabled matching rule. A null status causes `ArgumentNullException`. When `AlertOptions.Enabled` is false, the call returns without evaluating rules.

Service patterns are case-insensitive and support only these forms:

- `*` matches every service.
- A trailing `*`, such as `nginx*`, performs a prefix match.
- Any other value must equal the complete unit name.

Wildcards in other positions are treated as literal characters.

For each matching rule, the implementation checks cancellation before evaluation, then applies the condition as follows:

| Condition | Match |
| --- | --- |
| `ServiceFailed` | `HasFailed` is true or `State` is `Failed`. |
| `ServiceInactive` | `IsRunning` is false and `State` is `Inactive`. |
| `CpuThresholdExceeded` | CPU percentage is strictly greater than `Threshold`. |
| `MemoryThresholdExceeded` | memory MB is strictly greater than `Threshold` after the threshold is converted to `long`. |
| `RestartCountExceeded` | Not implemented by the default engine; it never matches. |
| `HealthCheckUnhealthy` | health status equals `Unhealthy`. |
| `HealthCheckDegraded` | health status is `Degraded` or a higher enum value. |
| `UptimeBelowMinimum` | the service is running and uptime seconds are strictly below `Threshold` after the threshold is converted to `long`. |
| `AnyStateChange` | Always matches each evaluated snapshot; the engine does not retain a prior state for comparison. |

A matching condition increments a counter scoped to the rule ID and service name. The engine opens an incident only after `ConsecutiveEvaluationsRequired` matches, then resets the counter. It suppresses another incident for the same rule/service pair until `CooldownMinutes` has elapsed. This cooldown is based on the last opened incident and applies even if that incident has since been resolved.

A non-matching condition resets the consecutive-hit counter. If `AutoResolveOnConditionCleared` is enabled, it changes every matching `Open` or `Escalated` incident to `AutoResolved`. Acknowledged and silenced incidents are not auto-resolved.

Opening an incident records the rule ID, service name, condition, severity, generated summary, and any observed numeric value. It then records an initial escalation-history entry. The current implementation delivers log notifications only: even when `EscalationPolicyId` is present, it does not load a policy or consult the on-call service. Webhook delivery code exists internally but is not selected by the current escalation path.

## Incident query contract

### `GetActiveIncidentsAsync(CancellationToken cancellationToken = default)`

Returns incidents in `Open`, `Acknowledged`, or `Escalated` state. Results are ordered by severity descending, then creation time ascending. The collection is a snapshot, but its incident objects are the stored mutable instances. The cancellation token is not inspected.

### `GetIncidentHistoryAsync(string? serviceName = null, int maxResults = 100, CancellationToken cancellationToken = default)`

Returns incidents of every state, newest first. A nonblank `serviceName` filters by case-insensitive exact service name. Null, empty, or whitespace means no filter. `maxResults` is passed directly to LINQ `Take`: zero or a negative value yields an empty result. The configured `MaxIncidentHistorySize` is not enforced by the current implementation. The cancellation token is not inspected.

### `GetIncidentByIdAsync(Guid incidentId, CancellationToken cancellationToken = default)`

Returns the incident with the supplied ID, or `null` when absent. The returned object is the stored mutable instance. The cancellation token is not inspected.

## Incident lifecycle contract

### `AcknowledgeIncidentAsync(Guid incidentId, string acknowledgedBy, CancellationToken cancellationToken = default)`

Changes an existing incident to `Acknowledged`, records the responder and UTC acknowledgement/update timestamps, and returns `true`. It returns `false` for an unknown, `Resolved`, or `AutoResolved` incident. The default implementation permits acknowledging a silenced incident and does not validate `acknowledgedBy`.

### `ResolveIncidentAsync(Guid incidentId, string resolvedBy, string notes = "", CancellationToken cancellationToken = default)`

Changes any existing incident to `Resolved`, records the resolver, notes, and UTC resolution/update timestamps, and returns `true`. It returns `false` only when the ID is unknown; resolving an already resolved, auto-resolved, or silenced incident is allowed. Inputs are not validated.

### `SilenceIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default)`

Changes any existing incident to `Silenced`, updates `UpdatedAt`, and returns `true`; an unknown ID returns `false`. Silenced incidents are absent from active queries and are ignored by escalation. No expiry time is recorded.

### `EscalateIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default)`

For an active or acknowledged incident, increments `CurrentEscalationLevel`, changes the state to `Escalated`, updates the timestamp, and attempts notification. An unknown ID or an incident in `Resolved`, `AutoResolved`, or `Silenced` state is a logged/no-op result rather than an exception. If the incident's rule has been removed, the state and level are still updated, but notification history is not added.

All lifecycle methods except escalation complete synchronously and do not inspect their cancellation token. Escalation passes its token to asynchronous notification delivery.

## Summary contract

### `GetSummaryAsync(CancellationToken cancellationToken = default)`

Returns an `AlertSummaryDto` computed from the current in-memory contents:

- `TotalRules` counts every rule; `EnabledRules` counts enabled rules.
- `OpenIncidents` counts `Open` and `Escalated`, but not `Acknowledged`.
- `CriticalIncidents` counts critical incidents in `Open` or `Escalated` state.
- `AcknowledgedIncidents` counts acknowledged incidents separately.
- `ResolvedLast24Hours` counts any incident with a `ResolvedAt` less than 24 hours before the current UTC time, including auto-resolved incidents.
- `AsOf` is the UTC time at which the summary was built.

The method does not inspect its cancellation token.

## Concurrency and ownership notes

The default implementation uses `ConcurrentDictionary` for top-level storage and tracking, so dictionary access is safe across callers. The `AlertRule` and `AlertIncident` objects stored inside those dictionaries remain mutable, however. Returned objects are not defensive copies, and multi-property updates are not transactional. Consumers should treat returned models as read-only and use the engine's lifecycle and update methods for changes.

## Example

```csharp
var rule = new AlertRule
{
    Name = "High CPU",
    ServicePattern = "api-*",
    Condition = AlertCondition.CpuThresholdExceeded,
    Threshold = 90m,
    Severity = AlertSeverity.High,
    ConsecutiveEvaluationsRequired = 3,
    CooldownMinutes = 15
};

await alertRulesEngine.AddRuleAsync(rule, cancellationToken);

await alertRulesEngine.EvaluateServiceAsync(
    new ServiceStatus
    {
        UnitName = "api-worker.service",
        CpuUsagePercent = 94.5m
    },
    cancellationToken);

var activeIncidents = await alertRulesEngine.GetActiveIncidentsAsync(cancellationToken);
```

The example requires three consecutive matching snapshots before opening an incident; one call alone only advances the hit counter.
