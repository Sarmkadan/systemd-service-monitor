# AlertRulesEngineExtensions
The `AlertRulesEngineExtensions` class provides a set of static extension methods for interacting with alert rules and incidents in the context of a systemd service monitor. These methods enable retrieval of alert rules and incidents based on various criteria, such as severity, service patterns, and incident states, allowing for more efficient and targeted monitoring and management of system services.

## API
The following members are part of the `AlertRulesEngineExtensions` class:
* `GetRulesBySeverityAsync`: Retrieves a list of alert rules based on the specified severity. Returns an `IEnumerable<AlertRule>`. Throws if the severity is invalid or if an error occurs during retrieval.
* `GetActiveIncidentsBySeverityAsync`: Retrieves a list of active incidents based on the specified severity. Returns an `IEnumerable<AlertIncident>`. Throws if the severity is invalid or if an error occurs during retrieval.
* `GetIncidentsByServiceAsync`: Retrieves a list of incidents related to the specified service. Returns an `IEnumerable<AlertIncident>`. Throws if the service is not found or if an error occurs during retrieval.
* `GetActiveIncidentCountsBySeverityAsync`: Retrieves a dictionary containing the count of active incidents by severity. Returns a `Dictionary<AlertSeverity, int>`. Throws if an error occurs during retrieval.
* `GetRulesByServicePatternAsync`: Retrieves a list of alert rules based on the specified service pattern. Returns an `IEnumerable<AlertRule>`. Throws if the service pattern is invalid or if an error occurs during retrieval.
* `GetLatestIncidentForServiceAsync`: Retrieves the latest incident for the specified service. Returns an `AlertIncident?`. Throws if the service is not found or if an error occurs during retrieval.
* `GetUnacknowledgedActiveIncidentsAsync`: Retrieves a list of unacknowledged active incidents. Returns an `IEnumerable<AlertIncident>`. Throws if an error occurs during retrieval.
* `GetEscalatedIncidentsAsync`: Retrieves a list of escalated incidents. Returns an `IEnumerable<AlertIncident>`. Throws if an error occurs during retrieval.
* `GetIncidentCountsByStateAsync`: Retrieves a dictionary containing the count of incidents by state. Returns a `Dictionary<AlertIncidentState, int>`. Throws if an error occurs during retrieval.

## Usage
The following examples demonstrate how to use the `AlertRulesEngineExtensions` class:
```csharp
// Retrieve all active incidents with a severity of Critical
var criticalIncidents = await AlertRulesEngineExtensions.GetActiveIncidentsBySeverityAsync(AlertSeverity.Critical);

// Retrieve the count of active incidents by severity
var incidentCounts = await AlertRulesEngineExtensions.GetActiveIncidentCountsBySeverityAsync();
```

## Notes
When using the `AlertRulesEngineExtensions` class, consider the following:
* The methods are asynchronous, so ensure that the calling code is also asynchronous to avoid blocking.
* The methods may throw exceptions if errors occur during retrieval, so consider implementing error handling mechanisms.
* The class is designed to be thread-safe, but it is still important to follow standard threading best practices when using the methods in a multi-threaded environment.
* The `GetLatestIncidentForServiceAsync` method returns a nullable `AlertIncident`, so be sure to check for null before attempting to access the incident's properties.
