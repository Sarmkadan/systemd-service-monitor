// existing content ...

## ServiceDependencyGraphServiceExtensions

The `ServiceDependencyGraphServiceExtensions` class provides utility methods for querying and analyzing service dependency graphs. It enables you to filter graphs, retrieve dependent services, and detect circular dependencies.

### Usage Example

```csharp
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

// Assuming a ServiceDependencyGraph named 'graph'
var graph = new ServiceDependencyGraph();
// Initialize graph with services and dependencies...

var filteredGraph = await ServiceDependencyGraphServiceExtensions.FilterGraphAsync(graph, "filter-criteria");
Console.WriteLine($"Filtered graph: {filteredGraph}");

var dependentServices = await ServiceDependencyGraphServiceExtensions.GetDependentServicesAsync(graph, "service-name");
Console.WriteLine($"Dependent services of 'service-name': {string.Join(", ", dependentServices.Select(d => d.Id))}");

var serviceDependencies = await ServiceDependencyGraphServiceExtensions.GetServiceDependenciesAsync(graph, "service-name");
Console.WriteLine($"Dependencies of 'service-name': {string.Join(", ", serviceDependencies.Select(d => d.Id))}");

var allServices = await ServiceDependencyGraphServiceExtensions.GetAllServicesAsync(graph);
Console.WriteLine($"All services: {string.Join(", ", allServices.Select(s => s.Id))}");

var hasCircularDependency = await ServiceDependencyGraphServiceExtensions.HasCircularDependencyAsync(graph);
Console.WriteLine($"Has circular dependency: {hasCircularDependency}");

var longestDependencyChain = await ServiceDependencyGraphServiceExtensions.GetLongestDependencyChainAsync(graph);
Console.WriteLine($"Longest dependency chain: {string.Join(" -> ", longestDependencyChain)}");

var longestChainFromService = await ServiceDependencyGraphServiceExtensions.GetLongestChainFromServiceAsync(graph, "service-name");
Console.WriteLine($"Longest chain from 'service-name': {string.Join(" -> ", longestChainFromService)}");
```
