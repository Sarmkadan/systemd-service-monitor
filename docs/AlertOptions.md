# AlertOptions

The `AlertOptions` class encapsulates configuration parameters for the alerting subsystem of the systemd-service-monitor. It controls whether alerts are enabled, how incidents are tracked, how escalation levels behave, and how webhook notifications are dispatched. Instances of this class are typically populated from application configuration and passed to the monitoring engine.

## API

### `public bool Enabled`

Gets or sets a value indicating whether alerting is active. When `false`, no alerts are generated regardless of other settings.

### `public bool AutoResolveOnConditionCleared`

Gets or sets a value indicating whether an incident should be automatically resolved when the monitored condition returns to a healthy state. When `true`, the incident is closed without manual intervention.

### `public int MaxIncidentHistorySize`

Gets or sets the maximum number of past incidents retained in memory for reporting or analysis. A value of `0` disables history retention. Negative values are not valid and may cause undefined behavior.

### `public int EscalationCheckIntervalSeconds`

Gets or sets the interval, in seconds, at which the escalation engine evaluates whether an incident should be escalated to the next level. Must be greater than zero; otherwise the escalation loop may behave unpredictably.

### `public int ServiceEvaluationIntervalSeconds`

Gets or sets the interval, in seconds, at which the service health is re-evaluated. This determines how often the monitor checks the underlying systemd service status. Must be greater than zero.

### `public int StartupDelaySeconds`

Gets or sets the delay, in seconds, before the first evaluation occurs after the monitor starts. This allows the service to stabilize before alerts are considered.

### `public WebhookNotificationOptions Webhook`

Gets or sets the configuration for webhook-based notifications. This object defines the endpoint URL, HTTP method, payload template, and authentication details. If `null`, webhook notifications are disabled.

### `public EscalationDefaults EscalationDefaults`

Gets or sets the default escalation parameters applied to incidents that do not have custom escalation rules. This includes default cooldown, delay, and maximum levels. If `null`, the escalation engine may fall back to hard‑coded defaults or throw an exception.

### `public int TimeoutSeconds`

Gets or sets the timeout, in seconds, for HTTP requests made to the webhook endpoint. A value of `0` or less may cause the request to never time out, which is not recommended.

### `public int MaxRetries`

Gets or sets the maximum number of retry attempts for a failed webhook delivery. A value of `0` means no retries.

### `public int RetryDelayMs`

Gets or sets the delay, in milliseconds, between retry attempts. Must be non‑negative.

### `public Dictionary<string, string> DefaultHeaders`

Gets or sets a dictionary of default HTTP headers to include in every webhook request. Headers defined here can be overridden by per‑incident headers. Setting this to `null` is equivalent to an empty dictionary.

### `public int InitialEscalationDelayMinutes`

Gets or sets the delay, in minutes, after an incident is first created before the first escalation attempt occurs. Must be non‑negative.

### `public int SubsequentEscalationDelayMinutes`

Gets or sets the delay, in minutes, between subsequent escalation levels after the initial delay. Must be non‑negative.

### `public int MaxEscalationLevels`

Gets or sets the maximum number of escalation levels that can be reached for a single incident. A value of `0` disables escalation entirely.

### `public int DefaultCooldownMinutes`

Gets or sets the default cooldown period, in minutes, after an incident is resolved before the same service can trigger a new alert. Must be non‑negative.

## Usage

### Example 1: Configuring AlertOptions from application settings

```csharp
var alertOptions = new AlertOptions
{
    Enabled = true,
    AutoResolveOnConditionCleared = true,
    MaxIncidentHistorySize = 100,
    EscalationCheckIntervalSeconds = 30,
    ServiceEvaluationIntervalSeconds = 10,
    StartupDelaySeconds = 5,
    TimeoutSeconds = 15,
    MaxRetries = 3,
    RetryDelayMs = 1000,
    InitialEscalationDelayMinutes = 5,
    SubsequentEscalationDelayMinutes = 10,
    MaxEscalationLevels = 3,
    DefaultCooldownMinutes = 60,
    DefaultHeaders = new Dictionary<string, string>
    {
        ["X-Source"] = "systemd-monitor"
    },
    Webhook = new WebhookNotificationOptions
    {
        Url = "https://hooks.example.com/alerts",
        Method = "POST",
        PayloadTemplate = "{\"service\":\"{{ServiceName}}\",\"status\":\"{{Status}}\"}"
    },
    EscalationDefaults = new EscalationDefaults
    {
        CooldownMinutes = 30,
        DelayMinutes = 5,
        MaxLevels = 5
    }
};

// Pass alertOptions to the monitoring engine
var monitor = new ServiceMonitor(alertOptions);
```

### Example 2: Modifying options at runtime

```csharp
// Assume existing AlertOptions instance is obtained from configuration
AlertOptions options = LoadOptions();

// Temporarily disable alerting during maintenance
options.Enabled = false;

// After maintenance, re-enable and adjust intervals
options.Enabled = true;
options.ServiceEvaluationIntervalSeconds = 15;
options.EscalationCheckIntervalSeconds = 45;

// Add a custom header for all subsequent webhook calls
options.DefaultHeaders["X-Maintenance"] = "true";
```

## Notes

- **Thread safety**: `AlertOptions` is not thread‑safe. If the same instance is accessed or modified concurrently from multiple threads, external synchronization (e.g., a lock) must be used. The monitoring engine typically reads these values once at startup or during a configuration reload; runtime modifications should be performed only when the engine is paused or through a dedicated reconfiguration mechanism.
- **Validation**: The class does not enforce validation on property setters. Setting negative values for interval or delay properties may lead to unexpected behavior (e.g., infinite loops, division by zero, or immediate escalation). It is the caller’s responsibility to ensure values are within sensible ranges before passing the instance to the monitor.
- **Null references**: The `Webhook` and `EscalationDefaults` properties can be set to `null`. The monitoring engine should handle null gracefully by disabling the corresponding feature. The `DefaultHeaders` dictionary, if set to `null`, is treated as empty.
- **Edge cases**:  
  - `MaxIncidentHistorySize = 0` disables history but does not prevent incident creation.  
  - `MaxEscalationLevels = 0` disables escalation; the incident remains at its initial level indefinitely.  
  - `AutoResolveOnConditionCleared = true` combined with a very short `ServiceEvaluationIntervalSeconds` may cause rapid resolution/re‑creation cycles if the service state fluctuates.  
  - `RetryDelayMs = 0` causes immediate retries, which may overwhelm the webhook endpoint.  
  - `StartupDelaySeconds` should be set high enough to allow the monitored service to start, but not so high that alerts are delayed unnecessarily.
