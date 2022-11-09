# AlertRulesEngine

Central service for managing alert rules, evaluating service states, and handling alert incidents within the systemd-service-monitor system. Integrates with an in-memory on-call schedule service to coordinate incident escalations and acknowledgments.

## API

### `AlertRulesEngine`

Constructor. Initializes the engine with required dependencies for rule management and incident handling.

### `Task<IEnumerable<AlertRule>> GetRulesAsync()`

Retrieves all configured alert rules asynchronously.

- **Returns**: An enumerable collection of `AlertRule` objects.
- **Throws**: `InvalidOperationException` if the underlying store is unavailable.

### `Task<AlertRule?> GetRuleByIdAsync(Guid id)`

Fetches a single alert rule by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the rule to retrieve.
- **Returns**: The matching `AlertRule` if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on storage failure.

### `Task<AlertRule> AddRuleAsync(AlertRule rule)`

Adds a new alert rule to the system.

- **Parameters**:
  - `rule` (AlertRule): The rule to add.
- **Returns**: The added `AlertRule` with updated metadata (e.g., ID).
- **Throws**: `ArgumentNullException` if `rule` is `null`; `InvalidOperationException` if a rule with the same ID already exists or on persistence failure.

### `Task<AlertRule?> UpdateRuleAsync(AlertRule rule)`

Updates an existing alert rule.

- **Parameters**:
  - `rule` (AlertRule): The rule containing updated properties and the original ID.
- **Returns**: The updated `AlertRule` if successful; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `rule` is `null`; `ArgumentException` if `rule.Id` is empty; `InvalidOperationException` on failure to locate or persist the rule.

### `Task<bool> RemoveRuleAsync(Guid id)`

Removes an alert rule by its identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the rule to remove.
- **Returns**: `true` if the rule was found and removed; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on storage failure.

### `async Task EvaluateServiceAsync(string serviceName)`

Evaluates the current state of a systemd service and triggers alerts based on configured rules.

- **Parameters**:
  - `serviceName` (string): The name of the systemd service to evaluate.
- **Throws**: `ArgumentException` if `serviceName` is null or whitespace; `InvalidOperationException` if rule evaluation or incident creation fails.

### `Task<IEnumerable<AlertIncident>> GetActiveIncidentsAsync()`

Retrieves all currently active (unresolved and unsilenced) alert incidents.

- **Returns**: An enumerable collection of `AlertIncident` objects representing active incidents.
- **Throws**: `InvalidOperationException` if the incident store is unavailable.

### `Task<IEnumerable<AlertIncident>> GetIncidentHistoryAsync()`

Retrieves the full history of all resolved and silenced alert incidents.

- **Returns**: An enumerable collection of `AlertIncident` objects representing historical incidents.
- **Throws**: `InvalidOperationException` if the incident store is unavailable.

### `Task<AlertIncident?> GetIncidentByIdAsync(Guid id)`

Fetches a single alert incident by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the incident to retrieve.
- **Returns**: The matching `AlertIncident` if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on storage failure.

### `Task<bool> AcknowledgeIncidentAsync(Guid id)`

Acknowledges an active alert incident, suppressing further notifications for the incident.

- **Parameters**:
  - `id` (Guid): The unique identifier of the incident to acknowledge.
- **Returns**: `true` if the incident was found and acknowledged; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on persistence failure.

### `Task<bool> ResolveIncidentAsync(Guid id)`

Marks an active alert incident as resolved.

- **Parameters**:
  - `id` (Guid): The unique identifier of the incident to resolve.
- **Returns**: `true` if the incident was found and resolved; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on persistence failure.

### `Task<bool> SilenceIncidentAsync(Guid id)`

Silences an active alert incident, preventing escalations and notifications until the silence period expires.

- **Parameters**:
  - `id` (Guid): The unique identifier of the incident to silence.
- **Returns**: `true` if the incident was found and silenced; otherwise, `false`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on persistence failure.

### `async Task EscalateIncidentAsync(Guid id)`

Initiates escalation procedures for an active alert incident, notifying on-call personnel according to the configured schedule.

- **Parameters**:
  - `id` (Guid): The unique identifier of the incident to escalate.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` if escalation cannot be initiated (e.g., no on-call personnel available).

### `Task<AlertSummaryDto> GetSummaryAsync()`

Generates a summary of current system health, including active rules, active incidents, and summary statistics.

- **Returns**: A `AlertSummaryDto` containing counts and status summaries.
- **Throws**: `InvalidOperationException` if data aggregation fails.

### `InMemoryOnCallScheduleService`

Provides in-memory storage and management of on-call schedules used for incident escalation. Accessible for dependency injection and integration testing.

### `Task<IEnumerable<OnCallSchedule>> GetSchedulesAsync()`

Retrieves all configured on-call schedules.

- **Returns**: An enumerable collection of `OnCallSchedule` objects.
- **Throws**: `InvalidOperationException` if the schedule store is unavailable.

### `Task<OnCallSchedule?> GetScheduleByIdAsync(Guid id)`

Fetches a single on-call schedule by its unique identifier.

- **Parameters**:
  - `id` (Guid): The unique identifier of the schedule to retrieve.
- **Returns**: The matching `OnCallSchedule` if found; otherwise, `null`.
- **Throws**: `ArgumentException` if `id` is empty; `InvalidOperationException` on storage failure.

### `Task<OnCallSchedule> CreateScheduleAsync(OnCallSchedule schedule)`

Creates a new on-call schedule.

- **Parameters**:
  - `schedule` (OnCallSchedule): The schedule to create.
- **Returns**: The created `OnCallSchedule` with updated metadata.
- **Throws**: `ArgumentNullException` if `schedule` is `null`; `InvalidOperationException` if a schedule with the same ID already exists or on persistence failure.

### `Task<OnCallSchedule?> UpdateScheduleAsync(OnCallSchedule schedule)`

Updates an existing on-call schedule.

- **Parameters**:
  - `schedule` (OnCallSchedule): The schedule containing updated properties and the original ID.
- **Returns**: The updated `OnCallSchedule` if successful; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `schedule` is `null`; `ArgumentException` if `schedule.Id` is empty; `InvalidOperationException` on failure to locate or persist the schedule.

## Usage

### Example 1: Adding and Evaluating a Rule
