# ServiceDependencyGraphService

A service that analyzes and visualizes the dependency relationships between systemd services by constructing a directed graph of service dependencies and dependents. It provides methods to traverse the graph, identify root and leaf services, and retrieve dependency chains for specific services.

## API

### `BuildGraphAsync`
Constructs a complete dependency graph for all systemd services on the host.

- **Returns**: `Task<ServiceDependencyGraph>` – A graph where nodes represent services and edges represent dependency relationships.
- **Exceptions**: Throws `InvalidOperationException` if the systemd interface is unavailable or if service parsing fails.

### `BuildGraphForServiceAsync`
Constructs a dependency graph rooted at a specific service, including only its transitive dependencies.

- **Parameters**:
  - `serviceName` (`string`) – The name of the service to build the graph from.
- **Returns**: `Task<ServiceDependencyGraph>` – A subgraph containing the specified service and its dependencies.
- **Exceptions**:
  - Throws `ArgumentNullException` if `serviceName` is `null`.
  - Throws `InvalidOperationException` if the service does not exist or if dependency resolution fails.

### `GetDependencyChainAsync`
Retrieves the full chain of dependencies for a given service, ordered from the service itself to its ultimate dependencies.

- **Parameters**:
  - `serviceName` (`string`) – The name of the service to analyze.
- **Returns**: `Task<IEnumerable<string>>` – An ordered sequence of service names starting with `serviceName` followed by its direct dependencies, then their dependencies, and so on.
- **Exceptions**:
  - Throws `ArgumentNullException` if `serviceName` is `null`.
  - Throws `InvalidOperationException` if the service does not exist or if the dependency chain cannot be resolved.

### `GetRootServicesAsync`
Identifies services that have no dependents (i.e., services that are not required by any other service).

- **Returns**: `Task<IEnumerable<DependencyNode>>` – A collection of `DependencyNode` instances representing root services.
- **Exceptions**: Throws `InvalidOperationException` if the graph cannot be built or traversed.

### `GetLeafServicesAsync`
Identifies services that have no dependencies (i.e., services that do not depend on any other service).

- **Returns**: `Task<IEnumerable<DependencyNode>>` – A collection of `DependencyNode` instances representing leaf services.
- **Exceptions**: Throws `InvalidOperationException` if the graph cannot be built or traversed.

### `ServiceName`
Gets the name of the service this instance represents.

- **Type**: `string`
- **Access**: Read-only

### `Description`
Gets the human-readable description of the service.

- **Type**: `string`
- **Access**: Read-only

### `State`
Gets the current state of the service.

- **Type**: `ServiceState`
- **Access**: Read-only

### `Dependencies`
Gets the set of service names that this service directly depends on.

- **Type**: `HashSet<string>`
- **Access**: Read-only

### `Dependents`
Gets the set of service names that directly depend on this service.

- **Type**: `HashSet<string>`
- **Access**: Read-only

## Usage

### Example 1: Building and traversing the full dependency graph
