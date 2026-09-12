# ServiceDependencyGraph

`ServiceDependencyGraph` is the data model for a directed graph of systemd service relationships. The model is declared in `Models/ServiceDependencyGraph.cs` together with the `DependencyNode` and `DependencyEdge` types used by its collections.

An edge points from a service to one of its dependencies. For example, an edge from `web.service` to `network.service` means that `web.service` depends on `network.service`.

## ServiceDependencyGraph fields

### `List<DependencyNode> Nodes`

The services represented in the graph. The list is initialized as empty.

### `List<DependencyEdge> Edges`

The directed dependency relationships between services in `Nodes`. The list is initialized as empty.

### `int TotalNodes`

The recorded number of nodes in the graph. This is stored separately from `Nodes.Count`; callers that construct or modify a graph directly are responsible for keeping the values consistent.

### `int TotalEdges`

The recorded number of edges in the graph. This is stored separately from `Edges.Count`; callers that construct or modify a graph directly are responsible for keeping the values consistent.

### `DateTime GeneratedAt`

The date and time at which the graph was generated. A newly constructed instance defaults this field to `DateTime.UtcNow`.

## DependencyNode fields

### `string ServiceName`

The systemd unit name represented by the node, such as `web.service`. It defaults to an empty string.

### `string Description`

A human-readable description of the service. It defaults to an empty string.

### `ServiceState State`

The service's current state, represented by the `SystemdServiceMonitor.Enums.ServiceState` enum. It defaults to `ServiceState.Unknown`.

### `List<string> Dependencies`

The names of services that this service directly depends on. The list is initialized as empty.

### `List<string> Dependents`

The names of services that directly depend on this service. The list is initialized as empty.

### `bool IsRootNode`

Indicates that the node has no dependents in the graph. In a subgraph, this value describes the relationships that remain inside that subgraph.

### `bool IsLeafNode`

Indicates that the node has no dependencies in the graph. In a subgraph, this value describes the relationships that remain inside that subgraph.

## DependencyEdge fields

### `string FromService`

The name of the service that has the dependency. It defaults to an empty string.

### `string ToService`

The name of the service depended upon by `FromService`. It defaults to an empty string.

### `string RelationshipType`

A label describing the relationship between the services. It defaults to an empty string. Graphs produced by `ServiceDependencyGraphService` use `DependsOn`.

## String representation

`ServiceDependencyGraph.ToString()` returns a summary containing `Nodes.Count`, `Edges.Count`, and `GeneratedAt` formatted as a universal sortable date and time. It uses the collection counts rather than the stored `TotalNodes` and `TotalEdges` fields.

## Example

```csharp
var graph = new ServiceDependencyGraph
{
    Nodes =
    [
        new DependencyNode
        {
            ServiceName = "web.service",
            Description = "Web application",
            State = ServiceState.Active,
            Dependencies = ["network.service"],
            IsRootNode = true
        },
        new DependencyNode
        {
            ServiceName = "network.service",
            Description = "Network availability",
            State = ServiceState.Active,
            Dependents = ["web.service"],
            IsLeafNode = true
        }
    ],
    Edges =
    [
        new DependencyEdge
        {
            FromService = "web.service",
            ToService = "network.service",
            RelationshipType = "DependsOn"
        }
    ],
    TotalNodes = 2,
    TotalEdges = 1,
    GeneratedAt = DateTime.UtcNow
};
```

All fields have public getters and setters. The collection properties are mutable and are not automatically synchronized with each other or with the total fields.
