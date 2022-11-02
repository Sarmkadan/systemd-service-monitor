# DependencyNode

Represents a node in a service dependency graph, capturing service relationships, states, and structural metadata for analysis and monitoring purposes.

## API

### `public string ServiceName`
The name of the service this node represents. This value is used as a unique identifier within the dependency graph.

### `public string Description`
A human-readable description of the service, typically derived from its unit file or system metadata.

### `public ServiceState State`
The current operational state of the service (e.g., `Running`, `Stopped`, `Failed`). Used to determine service health and dependency viability.

### `public List<string> Dependencies`
List of service names that this node depends on. Represents direct upstream relationships in the dependency graph.

### `public List<string> Dependents`
List of service names that depend on this node. Represents direct downstream relationships in the dependency graph.

### `public bool IsRootNode`
Indicates whether this node has no dependencies (`true`) or depends on other services (`false`). Used to identify entry points in the graph.

### `public bool IsLeafNode`
Indicates whether this node has no dependents (`true`) or is depended upon by other services (`false`). Used to identify terminal points in the graph.

### `public string FromService`
The source service name for a directed edge in the graph. Used when this node represents the origin of a dependency relationship.

### `public string ToService`
The target service name for a directed edge in the graph. Used when this node represents the destination of a dependency relationship.

### `public string RelationshipType`
The type of dependency relationship (e.g., `Requires`, `Wants`, `BindsTo`) between `FromService` and `ToService`. Describes the nature of the connection.

### `public List<DependencyNode> Nodes`
Collection of child nodes in the dependency graph. Used to represent hierarchical or nested service relationships.

### `public List<DependencyEdge> Edges`
Collection of directed edges representing service dependencies. Each edge connects a `FromService` to a `ToService` with a `RelationshipType`.

### `public int TotalNodes`
The total number of nodes in the dependency graph, including this node and all descendants. Used for graph size metrics.

### `public int TotalEdges`
The total number of edges (dependencies) in the dependency graph. Used for graph complexity metrics.

### `public DateTime GeneratedAt`
Timestamp indicating when the node and its associated graph data were generated. Used for temporal analysis and cache invalidation.

## Usage

### Example 1: Building a Dependency Graph
