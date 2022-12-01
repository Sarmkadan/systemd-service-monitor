# ServiceDependencyGraphServiceExtensions

Provides asynchronous extension methods for querying and analyzing the dependency graph of systemd services. These methods operate on an `IServiceDependencyGraph` instance and support filtering, traversal, and validation of service relationships, including detection of circular dependencies and computation of dependency chains.

## API

### FilterGraphAsync

```csharp
public static async Task<ServiceDependencyGraph> FilterGraphAsync(
    this IServiceDependencyGraph graph,
    Func<DependencyNode, bool> predicate)
```

Returns a new `ServiceDependencyGraph` containing only the nodes that satisfy the given predicate, along with their interconnecting edges. The original graph is not modified.

**Parameters:**
- `graph` — The source dependency graph to filter.
- `predicate` — A delegate that receives each `DependencyNode` and must return `true` for nodes to retain.

**Return value:**
A `Task<ServiceDependencyGraph>` whose result is the filtered subgraph.

**Exceptions:**
- `ArgumentNullException` — if `graph` or `predicate` is `null`.

---

### GetDependentServicesAsync

```csharp
public static async Task<IEnumerable<DependencyNode>> GetDependentServicesAsync(
    this IServiceDependencyGraph graph,
    string serviceName)
```

Retrieves all services that directly or transitively depend on the specified service.

**Parameters:**
- `graph` — The dependency graph to query.
- `serviceName` — The name of the service whose dependents are to be found.

**Return value:**
A `Task<IEnumerable<DependencyNode>>` containing the dependent services.

**Exceptions:**
- `ArgumentNullException` — if `graph` or `serviceName` is `null`.
- `KeyNotFoundException` — if `serviceName` does not exist in the graph.

---

### GetServiceDependenciesAsync

```csharp
public static async Task<IEnumerable<DependencyNode>> GetServiceDependenciesAsync(
    this IServiceDependencyGraph graph,
    string serviceName)
```

Retrieves all services that the specified service directly or transitively depends on.

**Parameters:**
- `graph` — The dependency graph to query.
- `serviceName` — The name of the service whose dependencies are to be found.

**Return value:**
A `Task<IEnumerable<DependencyNode>>` containing the dependency services.

**Exceptions:**
- `ArgumentNullException` — if `graph` or `serviceName` is `null`.
- `KeyNotFoundException` — if `serviceName` does not exist in the graph.

---

### GetAllServicesAsync

```csharp
public static async Task<IEnumerable<DependencyNode>> GetAllServicesAsync(
    this IServiceDependencyGraph graph)
```

Returns every `DependencyNode` present in the graph, without any filtering.

**Parameters:**
- `graph` — The dependency graph to enumerate.

**Return value:**
A `Task<IEnumerable<DependencyNode>>` containing all nodes.

**Exceptions:**
- `ArgumentNullException` — if `graph` is `null`.

---

### HasCircularDependencyAsync

```csharp
public static async Task<bool> HasCircularDependencyAsync(
    this IServiceDependencyGraph graph)
```

Determines whether the graph contains any circular dependency. A circular dependency exists when a service transitively depends on itself.

**Parameters:**
- `graph` — The dependency graph to inspect.

**Return value:**
A `Task<bool>` that yields `true` if at least one cycle is detected; otherwise `false`.

**Exceptions:**
- `ArgumentNullException` — if `graph` is `null`.

---

### GetLongestDependencyChainAsync

```csharp
public static async Task<IEnumerable<string>> GetLongestDependencyChainAsync(
    this IServiceDependencyGraph graph)
```

Computes the longest path of service names formed by following dependency edges anywhere in the graph. If multiple chains share the maximum length, one of them is returned.

**Parameters:**
- `graph` — The dependency graph to analyze.

**Return value:**
A `Task<IEnumerable<string>>` representing the ordered sequence of service names in the longest chain.

**Exceptions:**
- `ArgumentNullException` — if `graph` is `null`.
- `InvalidOperationException` — if the graph contains a cycle, since chain length is unbounded in that case.

---

### GetLongestChainFromServiceAsync

```csharp
public static async Task<IEnumerable<string>> GetLongestChainFromServiceAsync(
    this IServiceDependencyGraph graph,
    string serviceName)
```

Computes the longest dependency chain starting from the specified service. The returned sequence begins with `serviceName` and follows dependency edges as far as possible.

**Parameters:**
- `graph` — The dependency graph to analyze.
- `serviceName` — The starting service name.

**Return value:**
A `Task<IEnumerable<string>>` representing the ordered sequence of service names from the starting service to the farthest reachable node.

**Exceptions:**
- `ArgumentNullException` — if `graph` or `serviceName` is `null`.
- `KeyNotFoundException` — if `serviceName` does not exist in the graph.
- `InvalidOperationException` — if the subgraph reachable from `serviceName` contains a cycle.

## Usage

### Example 1: Detecting and reporting circular dependencies

```csharp
IServiceDependencyGraph graph = await SystemdGraphBuilder.BuildAsync();

bool hasCycle = await graph.HasCircularDependencyAsync();
if (hasCycle)
{
    Console.WriteLine("Warning: circular dependency detected in systemd services.");
}
else
{
    IEnumerable<string> longestChain = await graph.GetLongestDependencyChainAsync();
    Console.WriteLine($"Longest dependency chain: {string.Join(" -> ", longestChain)}");
}
```

### Example 2: Filtering and analyzing a subset of services

```csharp
IServiceDependencyGraph graph = await SystemdGraphBuilder.BuildAsync();

// Consider only services whose names start with "network-"
ServiceDependencyGraph filtered = await graph.FilterGraphAsync(
    node => node.ServiceName.StartsWith("network-"));

IEnumerable<DependencyNode> allFiltered = await filtered.GetAllServicesAsync();
Console.WriteLine($"Filtered graph contains {allFiltered.Count()} services.");

foreach (DependencyNode node in allFiltered)
{
    IEnumerable<DependencyNode> deps = await filtered.GetServiceDependenciesAsync(node.ServiceName);
    Console.WriteLine($"{node.ServiceName} depends on {deps.Count()} other services.");
}
```

## Notes

- All methods are asynchronous and should be awaited. The underlying implementation may involve I/O when resolving service states, though the graph structure itself is typically held in memory.
- Methods that accept a `serviceName` argument throw `KeyNotFoundException` if the name is not present in the graph. Callers should validate service names or catch this exception when querying arbitrary input.
- `GetLongestDependencyChainAsync` and `GetLongestChainFromServiceAsync` throw `InvalidOperationException` when a cycle is encountered. Use `HasCircularDependencyAsync` beforehand to guard against this case if cycles are possible.
- `FilterGraphAsync` produces a new graph instance; the original graph remains unchanged and can be reused independently.
- These extension methods are not guaranteed to be thread-safe on the same `IServiceDependencyGraph` instance. If multiple threads must access a graph concurrently, external synchronization is required.
- The order of elements in returned `IEnumerable<DependencyNode>` collections is deterministic within a single graph snapshot but may vary across different builds of the graph if the underlying systemd state has changed.
