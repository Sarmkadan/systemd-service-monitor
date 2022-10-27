# AlertRule

The `AlertRule` type defines the configuration and behavioral criteria for monitoring system services and triggering alerts based on specific performance or state conditions. It acts as a blueprint for the system to match running services against defined patterns, evaluate metrics against thresholds, and orchestrate incident responses through defined escalation policies.

## API

*   `Guid Id`: Unique identifier for the alert rule.
*   `string Name`: The display name of the alert rule. Required.
*   `string Description`: A detailed description of the alert rule's purpose.
*   `string ServicePattern`: A regex or wildcard pattern used to match the system services that this rule applies to. Required.
*   `AlertCondition Condition`: The logical operator or metric condition to evaluate (e.g., GreaterThan, Equals).
*   `decimal Threshold`: The numerical value to compare against the monitored service metrics.
*   `AlertSeverity Severity`: The severity level assigned to alerts triggered by this rule (e.g., Info, Warning, Critical).
*   `Guid? EscalationPolicyId`: Optional identifier linking the rule to an `EscalationPolicy`.
*   `bool IsEnabled`: Indicates whether the rule is currently active and processing.
*   `int CooldownMinutes`: The duration in minutes to suppress further alerts after an incident has been triggered.
*   `int ConsecutiveEvaluationsRequired`: The number of consecutive evaluation cycles the condition must be met before an alert is considered active.
*   `List<string> Tags`: A list of strings used for categorizing and filtering alerts.
*   `DateTime CreatedAt`: The timestamp indicating when the rule was first created.
*   `DateTime UpdatedAt`: The timestamp indicating the most recent modification to the rule.

*Note: The provided definition includes members from an `EscalationPolicy` structure (`Levels`, `RepeatEscalation`, `MaxRepeatCycles`) which are typically associated with an escalation configuration rather than the `AlertRule` itself.*

## Usage

### Basic Alert Rule Initialization
```csharp
var cpuAlert = new AlertRule
{
    Name = "High CPU Usage",
    ServicePattern = "web-server-.*",
    Condition = AlertCondition.GreaterThan,
    Threshold = 90.0m,
    Severity = AlertSeverity.Critical,
    IsEnabled = true,
    CooldownMinutes = 15,
    ConsecutiveEvaluationsRequired = 3
};
```

### Linking to an Escalation Policy
```csharp
var memoryAlert = new AlertRule
{
    Name = "Memory Leak Detection",
    ServicePattern = "data-processor",
    Condition = AlertCondition.GreaterThan,
    Threshold = 85.0m,
    Severity = AlertSeverity.Warning,
    EscalationPolicyId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    IsEnabled = true,
    Tags = new List<string> { "production", "memory" }
};
```

## Notes

*   **Thread Safety**: The `AlertRule` class is intended to be used as a data transfer object or configuration snapshot. Instances should be treated as immutable after initialization within the monitoring engine to ensure thread safety; if modification is necessary, a new instance should be created and swapped.
*   **Validation**: While `Name` and `ServicePattern` are marked as `required`, consumer code should validate that `ServicePattern` is a well-formed regular expression before enabling the rule to prevent runtime evaluation errors.
*   **Performance**: Extremely complex `ServicePattern` regexes may impact the performance of the monitoring loop. Keep patterns optimized for speed.
*   **Data Consistency**: The `CreatedAt` and `UpdatedAt` timestamps should be managed by the data persistence layer to ensure accuracy across service restarts.
