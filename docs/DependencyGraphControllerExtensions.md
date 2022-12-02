# DependencyGraphControllerExtensions
The `DependencyGraphControllerExtensions` type provides a set of extension methods for working with dependency graphs in the context of systemd service monitoring. It offers various methods to retrieve and analyze the dependency graph, including filtering, retrieving dependents, and summarizing the graph. These methods can be used to gain insights into the dependencies between services and to monitor their status.

## API
### GetFilteredGraph
* Purpose: Retrieves a filtered dependency graph based on the provided criteria.
* Parameters: Not specified in the provided information.
* Return Value: An `ActionResult` containing an `ApiResponse` with a `ServiceDependencyGraph` object.
* Throws: Not specified in the provided information.

### GetDependents
* Purpose: Retrieves a list of dependents for a given service.
* Parameters: Not specified in the provided information.
* Return Value: An `ActionResult` containing an `ApiResponse` with a list of `DependencyNode` objects.
* Throws: Not specified in the provided information.

### GetAllDependencies
* Purpose: Retrieves a list of all dependencies in the graph.
* Parameters: Not specified in the provided information.
* Return Value: An `ActionResult` containing an `ApiResponse` with a list of `DependencyNode` objects.
* Throws: Not specified in the provided information.

### GetGraphSummary
* Purpose: Retrieves a summary of the dependency graph.
* Parameters: Not specified in the provided information.
* Return Value: An `ActionResult` containing an `ApiResponse` with a `DependencyGraphSummary` object.
* Throws: Not specified in the provided information.

### Properties
* `TotalNodes`: The total number of nodes in the graph.
* `TotalEdges`: The total number of edges in the graph.
* `RootNodes`: The number of root nodes in the graph.
* `LeafNodes`: The number of leaf nodes in the graph.
* `ActiveServices`: The number of active services in the graph.
* `InactiveServices`: The number of inactive services in the graph.
* `FailedServices`: The number of failed services in the graph.
* `UnknownStateServices`: The number of services with an unknown state in the graph.
* `GeneratedAt`: The timestamp when the graph was generated.

## Usage
The following examples demonstrate how to use the `DependencyGraphControllerExtensions` type:
```csharp
// Example 1: Retrieving a filtered dependency graph
var graph = await DependencyGraphControllerExtensions.GetFilteredGraph();
if (graph.Result != null)
{
    Console.WriteLine("Filtered graph retrieved successfully.");
}

// Example 2: Retrieving a list of dependents for a service
var dependents = await DependencyGraphControllerExtensions.GetDependents();
if (dependents.Result != null)
{
    Console.WriteLine("Dependents retrieved successfully.");
    foreach (var dependent in dependents.Result)
    {
        Console.WriteLine(dependent);
    }
}
```

## Notes
When using the `DependencyGraphControllerExtensions` type, consider the following:
* The `GetFilteredGraph`, `GetDependents`, `GetAllDependencies`, and `GetGraphSummary` methods are asynchronous, so they should be awaited to ensure the completion of the operation.
* The properties (`TotalNodes`, `TotalEdges`, etc.) provide a snapshot of the graph's state at the time of generation, which may not reflect the current state of the services.
* The `GeneratedAt` property indicates when the graph was generated, which can be used to determine if the graph is up-to-date.
* The thread-safety of the `DependencyGraphControllerExtensions` type depends on the implementation of the underlying dependency graph and the services being monitored. It is recommended to use synchronization mechanisms if accessing the graph from multiple threads.
