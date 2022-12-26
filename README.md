// existing content ...

## AlertRulesEngineExtensions

The `AlertRulesEngineExtensions` class provides utility methods for retrieving alert rules and incidents based on various criteria. It enables flexible querying of alert data, facilitating incident management and monitoring.

### Usage Example

```csharp
using SystemdServiceMonitor.Services;
using SystemdServiceMonitor.Models;

// Retrieve alert rules by severity
var rulesBySeverity = await AlertRulesEngineExtensions.GetRulesBySeverityAsync(AlertSeverity.Critical);
Console.WriteLine($"Critical alert rules: {rulesBySeverity.Count()}");

// Get active incidents by severity
var activeIncidentsBySeverity = await AlertRulesEngineExtensions.GetActiveIncidentsBySeverityAsync(AlertSeverity.Warning);
Console.WriteLine($"Active warning incidents: {activeIncidentsBySeverity.Count()}");

// Get incidents by service
var incidentsByService = await AlertRulesEngineExtensions.GetIncidentsByServiceAsync("service-name");
Console.WriteLine($"Incidents for service 'service-name': {incidentsByService.Count()}");

// Get active incident counts by severity
var activeIncidentCounts = await AlertRulesEngineExtensions.GetActiveIncidentCountsBySeverityAsync();
foreach (var count in activeIncidentCounts)
{
    Console.WriteLine($"{count.Key}: {count.Value}");
}

// Retrieve rules by service pattern
var rulesByServicePattern = await AlertRulesEngineExtensions.GetRulesByServicePatternAsync("service-pattern");
Console.WriteLine($"Rules matching service pattern 'service-pattern': {rulesByServicePattern.Count()}");

// Get latest incident for a service
var latestIncident = await AlertRulesEngineExtensions.GetLatestIncidentForServiceAsync("service-name");
Console.WriteLine($"Latest incident for 'service-name': {latestIncident?.Id}");

// Get unacknowledged active incidents
var unacknowledgedIncidents = await AlertRulesEngineExtensions.GetUnacknowledgedActiveIncidentsAsync();
Console.WriteLine($"Unacknowledged active incidents: {unacknowledgedIncidents.Count()}");

// Get escalated incidents
var escalatedIncidents = await AlertRulesEngineExtensions.GetEscalatedIncidentsAsync();
Console.WriteLine($"Escalated incidents: {escalatedIncidents.Count()}");

// Get incident counts by state
var incidentCountsByState = await AlertRulesEngineExtensions.GetIncidentCountsByStateAsync();
foreach (var count in incidentCountsByState)
{
    Console.WriteLine($"{count.Key}: {count.Value}");
}
```

This example demonstrates how to use the methods of `AlertRulesEngineExtensions` to query alert rules and incidents based on different criteria, aiding in the management and monitoring of alerts within the system.
