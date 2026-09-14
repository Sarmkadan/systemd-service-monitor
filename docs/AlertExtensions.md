# AlertExtensions

This document describes the `AlertExtensions` class and the `AlertEscalationWorker` background service in the `SystemdServiceMonitor.Extensions` namespace.

## AlertExtensions

The `AlertExtensions` class provides extension methods for registering alert-related services into the ASP.NET Core dependency injection (DI) container.

### AddAlertRulesEngine

Registers alert rules engine services and configuration.

#### Signature

```csharp
public static IServiceCollection AddAlertRulesEngine(
    this IServiceCollection services,
    IConfiguration configuration)
```

#### Parameters

- `services`: The `IServiceCollection` to add services to.
- `configuration`: The `IConfiguration` providing application configuration.

#### Returns

The same `IServiceCollection` instance for method chaining.

#### Exceptions

- `ArgumentNullException`: Thrown if `services` or `configuration` is `null`.

#### Services Registered

The method registers the following services:

| Service | Lifetime | Description |
|---------|----------|-------------|
| `IAlertRulesEngine` → `AlertRulesEngine` | Singleton | Singleton alert evaluation engine |
| `IOnCallScheduleService` → `InMemoryOnCallScheduleService` | Singleton | Singleton on-call schedule service |
| `AlertEscalationWorker` | Singleton | Hosted background service for periodic alert evaluation and escalation |
| `AlertOptions` | Singleton | Configuration options bound from the `Alerts` section |
| `HttpClient` (named `AlertRulesEngine`) | Singleton | Named `HttpClient` for outbound webhook delivery with configured timeout and default headers |

#### Configuration

The method binds configuration from the `Alerts` section to `AlertOptions`. The `AlertOptions` class configures:

- Whether the alert rules engine is enabled (`Enabled`)
- Startup delay before the escalation worker begins (`StartupDelaySeconds`)
- Intervals for service evaluation and escalation checks (`ServiceEvaluationIntervalSeconds`, `EscalationCheckIntervalSeconds`)
- Webhook settings for outbound alerts (`Webhook.TimeoutSeconds`, `Webhook.DefaultHeaders`)
- Escalation policy defaults (`EscalationDefaults`)

#### Usage

In `Program.cs` or `Startup.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add alert rules engine services
builder.Services.AddAlertRulesEngine(builder.Configuration);

// ... other service registrations

var app = builder.Build();
```

## AlertEscalationWorker

The `AlertEscalationWorker` is a long-running background service that drives two periodic responsibilities:

1. **Service evaluation** — resolves the current status of all monitored services via `IServiceMonitorService` and feeds each snapshot into `IAlertRulesEngine.EvaluateServiceAsync`.
2. **Escalation promotion** — inspects open incidents whose last escalation timestamp is older than the policy's configured delay and advances them to the next escalation level.

### Constructor

```csharp
public AlertEscalationWorker(
    ILogger<AlertEscalationWorker> logger,
    IServiceScopeFactory scopeFactory,
    IAlertRulesEngine alertEngine,
    IOptions<AlertOptions> options)
```

#### Parameters

- `logger`: The `ILogger` instance for logging.
- `scopeFactory`: The `IServiceScopeFactory` for creating scoped services.
- `alertEngine`: The `IAlertRulesEngine` instance for evaluating alert rules.
- `options`: The `AlertOptions` configuration.

#### Exceptions

- `ArgumentNullException`: Thrown if any parameter is `null`.

### Execution Flow

When the application starts, the worker:

1. Waits for the configured startup delay (`StartupDelaySeconds`).
2. Logs the evaluation and escalation check intervals.
3. Enters a loop that continues until a cancellation token is triggered:
   - Calls `EvaluateServicesAsync` to evaluate all monitored services.
   - Checks if it's time to perform an escalation check (based on `EscalationCheckIntervalSeconds`).
   - If so, calls `PromoteDueEscalationsAsync` to advance eligible incidents.
   - Waits for the service evaluation interval before repeating.

#### Error Handling

If an exception occurs during service evaluation or escalation promotion:
- The error is logged.
- The worker waits for 10 seconds before resuming (back-off period).

### Methods

#### EvaluateServicesAsync

Evaluates all monitored services for alert conditions:

1. Creates a scope to resolve `IServiceMonitorService`.
2. Retrieves all services via `GetAllServicesAsync`.
3. For each service:
   - Gets the current status via `GetServiceStatusAsync`.
   - If status is available, evaluates alert rules via `IAlertRulesEngine.EvaluateServiceAsync`.
   - Logs warnings for any errors during evaluation.

#### PromoteDueEscalationsAsync

Promotes incidents that are due for escalation:

1. Retrieves active incidents via `IAlertRulesEngine.GetActiveIncidentsAsync`.
2. For each incident:
   - Skips if the incident is acknowledged or silenced.
   - Determines the last escalation action time (from escalation history or creation time).
   - Calculates the delay based on the current escalation level:
     - Level 0 uses `InitialEscalationDelayMinutes`.
     - Higher levels use `SubsequentEscalationDelayMinutes`.
   - If the incident has reached the maximum escalation level, logs a debug message and skips.
   - If the time since the last action exceeds the delay:
     - Logs an information message about the due escalation.
     - Attempts to escalate the incident via `IAlertRulesEngine.EscalateIncidentAsync`.
     - Logs warnings for any errors during escalation.

### Dependencies

The worker depends on the following services being registered in the DI container:
- `ILogger<AlertEscalationWorker>`
- `IServiceScopeFactory`
- `IAlertRulesEngine`
- `IOptions<AlertOptions>`

These are registered by the `AddAlertRulesEngine` extension method.

## Example Configuration

In `appsettings.json`:

```json
{
  "Alerts": {
    "Enabled": true,
    "StartupDelaySeconds": 10,
    "ServiceEvaluationIntervalSeconds": 30,
    "EscalationCheckIntervalSeconds": 60,
    "Webhook": {
      "TimeoutSeconds": 15,
      "DefaultHeaders": {
        "Content-Type": "application/json"
      }
    },
    "EscalationDefaults": {
      "InitialEscalationDelayMinutes": 5,
      "SubsequentEscalationDelayMinutes": 15,
      "MaxEscalationLevels": 3
    }
  }
}
```