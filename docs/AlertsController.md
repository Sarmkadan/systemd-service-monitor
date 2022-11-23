# AlertsController

Provides HTTP endpoints for managing alert rules and incidents within the systemd-service-monitor system. Exposes CRUD operations for alert rules and incident lifecycle management including acknowledgment and resolution.

## API

### `GetRules`

Retrieves all configured alert rules.

- **Returns**: `Task<ActionResult<ApiResponse<List<AlertRuleDto>>>>`
  - Success: HTTP 200 with list of `AlertRuleDto` objects.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during database access or serialization errors.

### `GetRuleById`

Retrieves a specific alert rule by its unique identifier.

- **Parameters**: `id` (route) — The unique identifier of the alert rule.
- **Returns**: `Task<ActionResult<ApiResponse<AlertRuleDto>>>>`
  - Success: HTTP 200 with the matching `AlertRuleDto`.
  - Not Found: HTTP 404 if no rule exists for the given `id`.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during database access or serialization errors.

### `CreateRule`

Creates a new alert rule.

- **Parameters**: `ruleDto` (body) — The `AlertRuleDto` containing rule configuration.
- **Returns**: `Task<ActionResult<ApiResponse<AlertRuleDto>>>>`
  - Success: HTTP 201 with the created `AlertRuleDto`.
  - Bad Request: HTTP 400 if `ruleDto` is invalid.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during validation or persistence errors.

### `UpdateRule`

Updates an existing alert rule.

- **Parameters**: `id` (route) — The unique identifier of the alert rule to update. `ruleDto` (body) — The updated `AlertRuleDto`.
- **Returns**: `Task<ActionResult<ApiResponse<AlertRuleDto>>>>`
  - Success: HTTP 200 with the updated `AlertRuleDto`.
  - Not Found: HTTP 404 if no rule exists for the given `id`.
  - Bad Request: HTTP 400 if `ruleDto` is invalid.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during validation or persistence errors.

### `DeleteRule`

Deletes an existing alert rule.

- **Parameters**: `id` (route) — The unique identifier of the alert rule to delete.
- **Returns**: `Task<ActionResult<ApiResponse<bool>>>>`
  - Success: HTTP 200 with `true` if deletion succeeded.
  - Not Found: HTTP 404 if no rule exists for the given `id`.
  - Conflict: HTTP 409 if the rule is referenced by active incidents.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during persistence errors.

### `GetActiveIncidents`

Retrieves all currently active alert incidents.

- **Returns**: `Task<ActionResult<ApiResponse<List<AlertIncidentDto>>>>`
  - Success: HTTP 200 with list of `AlertIncidentDto` objects.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during database access or serialization errors.

### `GetIncidentById`

Retrieves a specific alert incident by its unique identifier.

- **Parameters**: `id` (route) — The unique identifier of the alert incident.
- **Returns**: `Task<ActionResult<ApiResponse<AlertIncidentDto>>>>`
  - Success: HTTP 200 with the matching `AlertIncidentDto`.
  - Not Found: HTTP 404 if no incident exists for the given `id`.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during database access or serialization errors.

### `AcknowledgeIncident`

Marks an alert incident as acknowledged.

- **Parameters**: `id` (route) — The unique identifier of the alert incident to acknowledge.
- **Returns**: `Task<ActionResult<ApiResponse<AlertIncidentDto>>>>`
  - Success: HTTP 200 with the updated `AlertIncidentDto`.
  - Not Found: HTTP 404 if no incident exists for the given `id`.
  - Conflict: HTTP 409 if the incident is already resolved.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during persistence errors.

### `ResolveIncident`

Marks an alert incident as resolved.

- **Parameters**: `id` (route) — The unique identifier of the alert incident to resolve.
- **Returns**: `Task<ActionResult<ApiResponse<AlertIncidentDto>>>>`
  - Success: HTTP 200 with the updated `AlertIncidentDto`.
  - Not Found: HTTP 404 if no incident exists for the given `id`.
  - Conflict: HTTP 409 if the incident is already resolved or not acknowledged.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during persistence errors.

### `GetSummary`

Retrieves a summary of current alerting state including counts of active, acknowledged, and resolved incidents.

- **Returns**: `Task<ActionResult<ApiResponse<AlertSummaryDto>>>>`
  - Success: HTTP 200 with the `AlertSummaryDto`.
  - Failure: HTTP 500 with error details in `ApiResponse`.
- **Throws**: May throw during database access or aggregation errors.

## Usage
