# DependencyGraphController

Provides HTTP API endpoints for querying and traversing the dependency graph of monitored systemd services. This controller exposes operations to retrieve the full graph, identify root and leaf nodes, compute dependency paths between services, and extract subgraphs centered on a specific service.

## API

### GetGraph

```csharp
public async Task<ActionResult<ApiResponse<ServiceDependencyGraph>>> GetGraph()
```

Returns the complete service dependency graph for all monitored services. The graph includes all nodes and their directed edges representing `Requires`, `Wants`, `After`, and `Before` relationships.

**Parameters:** None.

**Return Value:**  
An `ApiResponse<ServiceDependencyGraph>` containing the full dependency graph structure. The `ServiceDependencyGraph` object holds a collection of nodes and their adjacency mappings.

**Throws:**  
May throw `InvalidOperationException` if the underlying graph builder has not completed initialization. The async operation can propagate `OperationCanceledException` if the request is aborted.

---

### GetRoots

```csharp
public async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetRoots()
```

Retrieves all root nodes in the dependency graph. A root node is defined as a service that has no incoming dependency edges from other monitored services (it is not depended upon by any other service in the graph).

**Parameters:** None.

**Return Value:**  
An `ApiResponse<List<DependencyNode>>` containing the list of root `DependencyNode` objects, each representing a service with zero inbound dependencies.

**Throws:**  
Same initialization and cancellation exceptions as `GetGraph`.

---

### GetLeaves

```csharp
public async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetLeaves()
```

Retrieves all leaf nodes in the dependency graph. A leaf node is defined as a service that has no outgoing dependency edges to other monitored services (it does not depend on any other service in the graph).

**Parameters:** None.

**Return Value:**  
An `ApiResponse<List<DependencyNode>>` containing the list of leaf `DependencyNode` objects, each representing a service with zero outbound dependencies.

**Throws:**  
Same initialization and cancellation exceptions as `GetGraph`.

---

### GetPath

```csharp
public async Task<ActionResult<ApiResponse<List<string>>>> GetPath(
    string fromService,
    string toService)
```

Computes a dependency path between two specified services. The path follows directed edges in the dependency graph and represents a chain of dependencies from the source service to the target service.

**Parameters:**
- `fromService` (`string`): The name of the source service unit (e.g., `"nginx.service"`).
- `toService` (`string`): The name of the target service unit (e.g., `"postgresql.service"`).

**Return Value:**  
An `ApiResponse<List<string>>` containing an ordered list of service names representing the dependency path. The first element is `fromService` and the last is `toService`. Returns an empty list if no path exists.

**Throws:**  
- `ArgumentException` if either service name is null, empty, or whitespace.
- `KeyNotFoundException` if either service is not present in the monitored graph.
- Same initialization and cancellation exceptions as `GetGraph`.

---

### GetSubgraph

```csharp
public async Task<ActionResult<ApiResponse<ServiceDependencyGraph>>> GetSubgraph(
    string serviceName,
    int depth = 1)
```

Extracts a subgraph centered on a specified service, including all dependencies up to the given traversal depth. The subgraph includes both inbound dependencies (services that depend on the target) and outbound dependencies (services the target depends on).

**Parameters:**
- `serviceName` (`string`): The name of the central service unit for the subgraph.
- `depth` (`int`, optional): The maximum number of dependency hops to traverse in each direction. Defaults to `1`.

**Return Value:**  
An `ApiResponse<ServiceDependencyGraph>` containing the subgraph with the specified service as the focal point and all reachable nodes within the given depth.

**Throws:**  
- `ArgumentException` if `serviceName` is null, empty, or whitespace, or if `depth` is negative.
- `KeyNotFoundException` if the specified service is not present in the monitored graph.
- Same initialization and cancellation exceptions as `GetGraph`.

## Usage

### Example 1: Retrieving and Displaying the Full Dependency Graph

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

var response = await client.GetAsync("/api/dependencygraph/graph");
response.EnsureSuccessStatusCode();

var apiResponse = await response.Content
    .ReadFromJsonAsync<ApiResponse<ServiceDependencyGraph>>();

if (apiResponse?.Success == true)
{
    var graph = apiResponse.Data;
    Console.WriteLine($"Total services: {graph.Nodes.Count}");
    foreach (var node in graph.Nodes)
    {
        var deps = graph.Adjacency.TryGetValue(node.Name, out var list) ? list : new();
        Console.WriteLine($"{node.Name} depends on: {string.Join(", ", deps)}");
    }
}
```

### Example 2: Finding a Path Between Two Services and Extracting a Subgraph

```csharp
using var client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };

// Find a dependency path from nginx to postgresql
var pathResponse = await client.GetAsync(
    "/api/dependencygraph/path?fromService=nginx.service&toService=postgresql.service");
pathResponse.EnsureSuccessStatusCode();

var pathResult = await pathResponse.Content
    .ReadFromJsonAsync<ApiResponse<List<string>>>();

if (pathResult?.Success == true && pathResult.Data.Count > 0)
{
    Console.WriteLine($"Path: {string.Join(" -> ", pathResult.Data)}");
}
else
{
    Console.WriteLine("No dependency path found between the specified services.");
}

// Extract a subgraph around postgresql with depth 2
var subgraphResponse = await client.GetAsync(
    "/api/dependencygraph/subgraph?serviceName=postgresql.service&depth=2");
subgraphResponse.EnsureSuccessStatusCode();

var subgraphResult = await subgraphResponse.Content
    .ReadFromJsonAsync<ApiResponse<ServiceDependencyGraph>>();

if (subgraphResult?.Success == true)
{
    Console.WriteLine(
        $"Subgraph around postgresql.service contains {subgraphResult.Data.Nodes.Count} services.");
}
```

## Notes

- **Graph Consistency:** All endpoints operate on a snapshot of the dependency graph taken at request time. If the underlying systemd service state changes between successive calls, results may reflect different snapshots.
- **Path Ambiguity:** `GetPath` returns the first discovered path using a breadth-first traversal. Multiple valid paths may exist between two services; this method does not enumerate all possibilities.
- **Depth Semantics:** In `GetSubgraph`, a depth of `0` returns only the specified service node with no edges. Negative depth values cause an `ArgumentException`.
- **Thread Safety:** The controller itself is stateless and safe for concurrent requests. Thread safety of the underlying graph data source depends on the implementation of the injected dependency graph service.
- **Cancellation:** All async methods respect `CancellationToken` propagation via the ASP.NET Core request pipeline. Clients disconnecting mid-request will trigger `OperationCanceledException`.
- **Service Naming:** Service names must match the exact unit names as tracked by the monitor (e.g., `"sshd.service"`, not `"ssh"`). Case sensitivity follows systemd conventions.
- **Empty Graphs:** If no services are monitored, `GetGraph` returns a valid but empty graph structure, `GetRoots` and `GetLeaves` return empty lists, and `GetPath` and `GetSubgraph` throw `KeyNotFoundException` for any requested service.
