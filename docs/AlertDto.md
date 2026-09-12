# Alert DTOs

`Dtos/AlertDto.cs` defines the request and response records used by the alerts API. The records are grouped around alert rules, incidents, escalation policies, on-call schedules, and dashboard summary data. All types are in the `SystemdServiceMonitor.Dtos` namespace.

Because these are positional C# records, their constructor parameters also define their public init-only properties and participate in value equality.

## Alert rules

### `AlertRuleDto`

A read-only API projection of an `AlertRule`.

| Property | Type | Meaning |
| --- | --- | --- |
| `Id` | `Guid` | Unique rule identifier. |
| `Name` | `string` | Human-readable rule name. |
| `Description` | `string` | Description of the rule's purpose. |
| `ServicePattern` | `string` | Pattern used to select systemd services. |
| `Condition` | `AlertCondition` | Condition evaluated by the alert engine. |
| `Threshold` | `decimal` | Condition-specific threshold. |
| `Severity` | `AlertSeverity` | Severity assigned to incidents raised by the rule. |
| `EscalationPolicyId` | `Guid?` | Associated escalation policy, or `null` when none is configured. |
| `IsEnabled` | `bool` | Whether the engine evaluates the rule. |
| `CooldownMinutes` | `int` | Minimum interval between incidents for the same rule and service. |
| `ConsecutiveEvaluationsRequired` | `int` | Number of consecutive matches required before an incident is created. |
| `Tags` | `IReadOnlyList<string>` | Labels attached to the rule. |
| `CreatedAt` | `DateTime` | Rule creation time. |
| `UpdatedAt` | `DateTime` | Most recent rule update time. |

### `CreateAlertRuleDto`

Request payload for creating a rule. `EscalationPolicyId` defaults to `null`, `IsEnabled` to `true`, `CooldownMinutes` to `15`, `ConsecutiveEvaluationsRequired` to `1`, and `Tags` to `null`. The controller converts a null tag list to an empty list.

The required constructor arguments are `Name`, `Description`, `ServicePattern`, `Condition`, `Threshold`, and `Severity`; the remaining properties have the defaults described above.

```csharp
var request = new CreateAlertRuleDto(
    Name: "High nginx CPU",
    Description: "Page when nginx CPU remains high",
    ServicePattern: "nginx.service",
    Condition: AlertCondition.CpuThresholdExceeded,
    Threshold: 90m,
    Severity: AlertSeverity.High,
    ConsecutiveEvaluationsRequired: 3,
    Tags: ["web", "production"]);
```

### `UpdateAlertRuleDto`

Partial-update payload for an existing rule. Every property is nullable and defaults to `null`: `Name`, `Description`, `ServicePattern`, `Condition`, `Threshold`, `Severity`, `EscalationPolicyId`, `IsEnabled`, `CooldownMinutes`, `ConsecutiveEvaluationsRequired`, and `Tags`.

Only non-null values are applied. A null value therefore means “leave unchanged”; in particular, this DTO cannot clear an existing `EscalationPolicyId` or replace `Tags` with `null`. Supplying an empty tag list does clear all tags. The engine updates `UpdatedAt` even if every property is null.

```csharp
var update = new UpdateAlertRuleDto(
    Threshold: 95m,
    Severity: AlertSeverity.Critical,
    IsEnabled: true);
```

## Incidents

### `AlertIncidentDto`

A read-only projection of an alert incident.

| Property | Type | Meaning |
| --- | --- | --- |
| `Id` | `Guid` | Unique incident identifier. |
| `AlertRuleId` | `Guid` | Identifier of the rule that raised the incident. |
| `RuleName` | `string` | Display name of the originating rule. The current controller mapping supplies an empty string. |
| `ServiceName` | `string` | Service associated with the incident. |
| `TriggerCondition` | `AlertCondition` | Condition that triggered the incident. |
| `Severity` | `AlertSeverity` | Incident severity. |
| `State` | `AlertIncidentState` | Current lifecycle state. |
| `CurrentEscalationLevel` | `int` | Current zero-based escalation level. |
| `Summary` | `string` | Human-readable incident summary. |
| `ObservedValue` | `decimal?` | Measured value, or `null` when the condition has no numeric observation. |
| `AcknowledgedBy` | `string?` | Responder who acknowledged the incident. |
| `AcknowledgedAt` | `DateTime?` | Acknowledgement time, when acknowledged. |
| `ResolvedBy` | `string?` | Responder or process that resolved the incident. |
| `ResolvedAt` | `DateTime?` | Resolution time, when resolved. |
| `ResolutionNotes` | `string` | Notes recorded for the resolution. |
| `EscalationHistory` | `IReadOnlyList<EscalationHistoryDto>` | Ordered escalation and notification history. |
| `CreatedAt` | `DateTime` | Incident creation time. |
| `UpdatedAt` | `DateTime` | Most recent incident update time. |

### `EscalationHistoryDto`

Represents one escalation attempt. It contains `Id` (`Guid`), `LevelReached` (`int`), `LevelName` (`string`), `Channel` (`NotificationChannel`), `NotificationTarget` (`string`), `NotificationDelivered` (`bool`), `DeliveryError` (`string?`), and `OccurredAt` (`DateTime`). `DeliveryError` is null when no delivery error was recorded.

### Incident action requests

`AcknowledgeIncidentDto` contains the required `AcknowledgedBy` string and an optional `Notes` string that defaults to `null`. The current alerts controller passes `AcknowledgedBy` to the engine but does not consume `Notes`.

`ResolveIncidentDto` contains the required `ResolvedBy` string and `ResolutionNotes`, which defaults to an empty string. Both values are passed to the engine.

```csharp
var acknowledgement = new AcknowledgeIncidentDto("operator-1", "Investigating");
var resolution = new ResolveIncidentDto("operator-1", "Restarted the failed dependency");
```

## Escalation policies

### `EscalationPolicyDto`

A read-only projection containing `Id` (`Guid`), `Name` (`string`), `Description` (`string`), `Levels` (`IReadOnlyList<EscalationLevelDto>`), `RepeatEscalation` (`bool`), `MaxRepeatCycles` (`int`), and `CreatedAt` (`DateTime`).

### `EscalationLevelDto`

Describes one escalation level:

| Property | Type | Meaning |
| --- | --- | --- |
| `Order` | `int` | Position of the level in the escalation sequence. |
| `Name` | `string` | Human-readable level name. |
| `EscalateAfterMinutes` | `int` | Delay before this level is reached. |
| `Channels` | `IReadOnlyList<NotificationChannel>` | Delivery channels used at this level. |
| `NotificationTargets` | `IReadOnlyDictionary<string, string>` | Channel or target keys mapped to delivery destinations. |
| `OnCallScheduleId` | `Guid?` | On-call schedule used to resolve a recipient, or `null`. |

### `CreateEscalationPolicyDto`

Creation payload requiring `Name`, `Description`, and a mutable `List<EscalationLevelDto>`. `RepeatEscalation` defaults to `false`, and `MaxRepeatCycles` defaults to `3`.

## On-call schedules

### `OnCallScheduleDto`

A read-only schedule projection containing `Id` (`Guid`), `Name` (`string`), `TimeZone` (`string`), `Entries` (`IReadOnlyList<OnCallEntryDto>`), `CurrentOnCall` (`OnCallEntryDto?`), and `CreatedAt` (`DateTime`). `CurrentOnCall` is null when no entry covers the time at which the schedule was resolved.

### `OnCallEntryDto`

One on-call shift with `Id` (`Guid`), `ResponderName` (`string`), `ContactTarget` (`string`), `PreferredChannel` (`NotificationChannel`), `ShiftStart` (`DateTime`), `ShiftEnd` (`DateTime`), `IsOverride` (`bool`), and `Notes` (`string`).

### Schedule request DTOs

`CreateOnCallScheduleDto` requires a schedule `Name`, an IANA or platform-supported `TimeZone` identifier, and a mutable `List<OnCallEntryDto>` of `Entries`.

`AddOnCallOverrideDto` requires `ResponderName`, `ContactTarget`, `PreferredChannel`, `ShiftStart`, and `ShiftEnd`. Its `Notes` value defaults to an empty string.

## `AlertSummaryDto`

A compact read-only snapshot for alert dashboards.

| Property | Type | Meaning |
| --- | --- | --- |
| `TotalRules` | `int` | Total configured alert rules. |
| `EnabledRules` | `int` | Rules currently enabled. |
| `OpenIncidents` | `int` | Incidents in the `Open` or `Escalated` state. |
| `CriticalIncidents` | `int` | Open incidents with `Critical` severity. |
| `AcknowledgedIncidents` | `int` | Incidents in the `Acknowledged` state. |
| `ResolvedLast24Hours` | `int` | Incidents resolved during the preceding 24 hours. |
| `AsOf` | `DateTime` | UTC time at which the snapshot was computed. |

## Related enums and serialization

The DTOs reuse `AlertCondition`, `AlertSeverity`, `AlertIncidentState`, and `NotificationChannel` from `SystemdServiceMonitor.Models`. The application's configured JSON options serialize enums as names rather than numeric values. Date/time values are serialized using the configured `System.Text.Json` date representation; fields documented as UTC should be populated with UTC values by producers.

API responses are wrapped in `ApiResponse<T>` by `AlertsController`; these DTOs describe the value carried in that wrapper's `Data` property or the body accepted by an alert endpoint.
