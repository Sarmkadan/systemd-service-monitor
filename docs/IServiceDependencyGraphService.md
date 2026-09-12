# `IServiceDependencyGraphService`

`IServiceDependencyGraphService` is the asynchronous contract for building and querying directed graphs of systemd service relationships. It returns `ServiceDependencyGraph` and `DependencyNode` models, where an edge from one service to another means that the first service depends on the second.

The interface is defined in `Services/IServiceDependencyGraphService.cs`. Its default implementation is `ServiceDependencyGraphService` in `Services/ServiceDependencyGraphService.cs`.

## Registration and data source

The application registers `IServiceDependencyGraphService` as a scoped service implemented by `ServiceDependencyGraphService`. The default implementation requires an `IServiceRepository`; passing a null repository to its constructor throws `ArgumentNullException`.

Every public operation requests the current complete service collection from the repository and builds a new in-memory graph. Results are therefore snapshots rather than a live view or a cache. Cancellation tokens are passed to the repository query.

Graph construction combines both the `Dependencies` and `Dependents` reported by each `ServiceInfo`. Referenced service names that do not have their own `ServiceInfo` still become nodes, with an empty description and `ServiceState.Unknown`. Service-name matching and relationship de-duplication are case-insensitive. Returned nodes, their relationship lists, and graph edges are ordered by service name using the default string ordering.

## Contract

### `BuildGraphAsync(CancellationToken ct = default)`

Builds and returns the complete dependency graph from all services supplied by the repository. The result includes nodes, `DependsOn` edges, node and edge totals, and a UTC generation timestamp.

A node is marked as a root when no other node depends on it and as a leaf when it has no dependencies. An empty repository result produces a valid graph with empty collections and zero totals.

### `BuildGraphForServiceAsync(string unitName, int depth = 3, CancellationToken ct = default)`

Builds a subgraph centered on `unitName`. Traversal uses both dependencies and dependents, so the result describes the service's local neighborhood rather than only its downstream dependencies. `depth` is the maximum number of relationships traversed from the requested service:

- A depth of `0` includes only the requested node.
- The default depth of `3` includes nodes up to three relationships away.
- A negative depth is treated as `0`.

The service name lookup is case-insensitive. A null `unitName` throws `ArgumentNullException`; an empty, whitespace-only, or unknown name returns an empty graph. Root and leaf flags are recalculated from relationships contained in the returned subgraph, so they can differ from the same nodes' flags in the complete graph.

### `GetDependencyChainAsync(string fromService, string toService, CancellationToken ct = default)`

Finds the shortest directed dependency path from `fromService` to `toService` using breadth-first search. Unlike scoped graph traversal, this method follows dependencies only; it does not traverse dependents. The returned sequence contains both endpoints in traversal order.

Names are matched case-insensitively. If both arguments identify the same existing service, the result contains that one service name. Null arguments throw `ArgumentNullException`. Empty or whitespace-only names, unknown services, and pairs with no dependency path produce an empty sequence.

When more than one equally short path exists, the selected path follows the dependency ordering in the constructed graph.

### `GetRootServicesAsync(CancellationToken ct = default)`

Returns nodes with no dependents (`IsRootNode == true`), ordered by service name. In this graph model, a root can depend on other services but is not itself depended upon by another service.

### `GetLeafServicesAsync(CancellationToken ct = default)`

Returns nodes with no dependencies (`IsLeafNode == true`), ordered by service name. A leaf can have dependents but does not itself depend on another service.

## Result ownership

Each call creates a new graph and new mutable model objects. The interface returns `IEnumerable<T>` for chains, roots, and leaves, but the default implementation materializes these results in lists. Callers may modify returned objects without changing repository data; those modifications also do not affect later calls.

## Example

```csharp
var graph = await dependencyGraphService.BuildGraphForServiceAsync(
    "web.service",
    depth: 2,
    cancellationToken);

var chain = await dependencyGraphService.GetDependencyChainAsync(
    "web.service",
    "network.service",
    cancellationToken);

if (chain.Any())
{
    Console.WriteLine(string.Join(" -> ", chain));
}

var roots = await dependencyGraphService.GetRootServicesAsync(cancellationToken);
var leaves = await dependencyGraphService.GetLeafServicesAsync(cancellationToken);
```

The models returned by this interface are documented in `docs/ServiceDependencyGraph.md`.
