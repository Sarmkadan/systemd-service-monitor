# AlertRuleExtensions

Extension methods for evaluating alert rule conditions and metadata in the `systemd-service-monitor` project. These methods provide utility functions for checking alert severity thresholds, tag presence, cooldown periods, activation states, and evaluation requirements against alert rule definitions.

## API

### `IsSeverityAtLeast(AlertRule rule, AlertSeverity threshold)`
Determines whether the alert rule's severity meets or exceeds the specified threshold.

- **Parameters**
  - `rule`: The alert rule to evaluate.
  - `threshold`: The minimum severity level to compare against.
- **Returns**
  - `true` if the rule's severity is greater than or equal to `threshold`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `IsSeverityGreaterThan(AlertRule rule, AlertSeverity threshold)`
Determines whether the alert rule's severity is strictly greater than the specified threshold.

- **Parameters**
  - `rule`: The alert rule to evaluate.
  - `threshold`: The severity level to compare against.
- **Returns**
  - `true` if the rule's severity is greater than `threshold`; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `HasAnyTag(AlertRule rule, params string[] tags)`
Checks if the alert rule has at least one of the specified tags.

- **Parameters**
  - `rule`: The alert rule to evaluate.
  - `tags`: One or more tag strings to check for presence.
- **Returns**
  - `true` if the rule contains any of the provided tags; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.
  - `ArgumentNullException` if `tags` is `null`.

### `HasAllTags(AlertRule rule, params string[] tags)`
Checks if the alert rule contains all of the specified tags.

- **Parameters**
  - `rule`: The alert rule to evaluate.
  - `tags`: One or more tag strings that must all be present.
- **Returns**
  - `true` if the rule contains every provided tag; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.
  - `ArgumentNullException` if `tags` is `null`.

### `GetCooldownSeconds(AlertRule rule)`
Retrieves the cooldown period in seconds for the alert rule.

- **Parameters**
  - `rule`: The alert rule whose cooldown period is to be retrieved.
- **Returns**
  - The cooldown duration in seconds. Returns `0` if no cooldown is configured.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `IsActive(AlertRule rule)`
Determines whether the alert rule is currently active based on its configuration.

- **Parameters**
  - `rule`: The alert rule to evaluate.
- **Returns**
  - `true` if the rule is active; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `GetSummary(AlertRule rule)`
Generates a human-readable summary of the alert rule.

- **Parameters**
  - `rule`: The alert rule for which to generate a summary.
- **Returns**
  - A string containing a concise description of the rule, including severity, tags, and other relevant metadata.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `RequiresConsecutiveEvaluations(AlertRule rule)`
Determines whether the alert rule requires multiple consecutive evaluations before triggering.

- **Parameters**
  - `rule`: The alert rule to evaluate.
- **Returns**
  - `true` if the rule requires consecutive evaluations; otherwise, `false`.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

### `GetRequiredEvaluationCount(AlertRule rule)`
Retrieves the number of consecutive evaluations required for the alert rule to trigger.

- **Parameters**
  - `rule`: The alert rule whose required evaluation count is to be retrieved.
- **Returns**
  - The number of required evaluations. Returns `1` if no specific count is configured.
- **Throws**
  - `ArgumentNullException` if `rule` is `null`.

## Usage
