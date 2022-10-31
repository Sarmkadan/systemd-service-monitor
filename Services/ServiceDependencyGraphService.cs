#nullable enable

using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service that builds and analyzes dependency graphs for systemd services.
/// </summary>
public class ServiceDependencyGraphService(IServiceRepository serviceRepository) : IServiceDependencyGraphService
{
    private readonly ILogger<ServiceDependencyGraphService>? _logger = null;

    /// <summary>
    /// Builds a complete dependency graph for all systemd services.
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>ServiceDependencyGraph containing all services and their dependencies.</returns>
    public async Task<ServiceDependencyGraph> BuildGraphAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Building dependency graph for all services");
        var services = await serviceRepository.GetAllAsync(ct);
        var graph = BuildGraph(services);
        _logger?.LogInformation("Dependency graph built successfully: {NodeCount} nodes, {EdgeCount} edges", graph.TotalNodes, graph.TotalEdges);
        return graph;
    }

    /// <summary>
    /// Builds a dependency graph for a specific service with configurable depth.
    /// </summary>
    /// <param name="unitName">Name of the service to build graph for.</param>
    /// <param name="depth">Maximum depth of dependencies to include (default: 3).</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>ServiceDependencyGraph containing the service and its dependencies up to specified depth.</returns>
    public async Task<ServiceDependencyGraph> BuildGraphForServiceAsync(string unitName, int depth = 3, CancellationToken ct = default)
    {
        _logger?.LogInformation("Building dependency graph for service: {ServiceName} (depth: {Depth})", unitName, depth);
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        if (string.IsNullOrWhiteSpace(unitName))
        {
            _logger?.LogWarning("Service name is empty, returning empty graph");
            return CreateEmptyGraph();
        }

        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);
        if (!nodeLookup.ContainsKey(unitName))
        {
            _logger?.LogWarning("Service {ServiceName} not found in graph", unitName);
            return CreateEmptyGraph();
        }

        depth = Math.Max(0, depth);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string ServiceName, int Depth)>();
        queue.Enqueue((unitName, 0));

        while (queue.Count > 0)
        {
            var (current, currentDepth) = queue.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            if (!nodeLookup.TryGetValue(current, out var node) || currentDepth >= depth)
            {
                continue;
            }

            foreach (var related in node.Dependencies.Concat(node.Dependents).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!visited.Contains(related))
                {
                    queue.Enqueue((related, currentDepth + 1));
                }
            }
        }

        var subgraph = BuildSubgraph(graph, visited);
        _logger?.LogInformation("Dependency graph for {ServiceName} built: {NodeCount} nodes, {EdgeCount} edges", unitName, subgraph.TotalNodes, subgraph.TotalEdges);
        return subgraph;
    }

    /// <summary>
    /// Finds a dependency chain between two services using breadth-first search.
    /// </summary>
    /// <param name="fromService">Starting service name.</param>
    /// <param name="toService">Target service name.</param>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>Sequence of service names representing the dependency chain, or empty if no path exists.</returns>
    public async Task<IEnumerable<string>> GetDependencyChainAsync(string fromService, string toService, CancellationToken ct = default)
    {
        _logger?.LogInformation("Finding dependency chain from {FromService} to {ToService}", fromService, toService);
        if (string.IsNullOrWhiteSpace(fromService) || string.IsNullOrWhiteSpace(toService))
        {
            _logger?.LogWarning("Invalid service names provided: from='{FromService}', to='{ToService}'", fromService, toService);
            return [];
        }

        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);
        if (!nodeLookup.ContainsKey(fromService) || !nodeLookup.ContainsKey(toService))
        {
            _logger?.LogWarning("One or both services not found in graph: from='{FromService}', to='{ToService}'", fromService, toService);
            return [];
        }

        if (string.Equals(fromService, toService, StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogDebug("Services are the same, returning single service path");
            return [nodeLookup.Keys.First(name => string.Equals(name, fromService, StringComparison.OrdinalIgnoreCase))];
        }

        var queue = new Queue<List<string>>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { fromService };
        queue.Enqueue([fromService]);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var current = path[^1];

            foreach (var dependency in nodeLookup[current].Dependencies)
            {
                if (visited.Contains(dependency))
                {
                    continue;
                }

                var nextPath = new List<string>(path) { dependency };
                if (string.Equals(dependency, toService, StringComparison.OrdinalIgnoreCase))
                {
                    _logger?.LogInformation("Dependency chain found: {Chain}", string.Join(" -> ", nextPath));
                    return nextPath;
                }

                visited.Add(dependency);
                queue.Enqueue(nextPath);
            }
        }

        _logger?.LogWarning("No dependency chain found from {FromService} to {ToService}", fromService, toService);
        return [];
    }

    /// <summary>
    /// Retrieves all root services (services with no dependents).
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>Collection of DependencyNode objects representing root services, ordered by service name.</returns>
    public async Task<IEnumerable<DependencyNode>> GetRootServicesAsync(CancellationToken ct = default)
    {
        _logger?.LogDebug("Retrieving root services (services with no dependents)");
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        var rootServices = graph.Nodes
            .Where(node => node.IsRootNode)
            .OrderBy(node => node.ServiceName)
            .ToList();
        _logger?.LogInformation("Found {Count} root services", rootServices.Count);
        return rootServices;
    }

    /// <summary>
    /// Retrieves all leaf services (services with no dependencies).
    /// </summary>
    /// <param name="ct">Cancellation token for async operation.</param>
    /// <returns>Collection of DependencyNode objects representing leaf services, ordered by service name.</returns>
    public async Task<IEnumerable<DependencyNode>> GetLeafServicesAsync(CancellationToken ct = default)
    {
        _logger?.LogDebug("Retrieving leaf services (services with no dependencies)");
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        var leafServices = graph.Nodes
            .Where(node => node.IsLeafNode)
            .OrderBy(node => node.ServiceName)
            .ToList();
        _logger?.LogInformation("Found {Count} leaf services", leafServices.Count);
        return leafServices;
    }

    /// <summary>
    /// Builds a complete dependency graph from a collection of service information.
    /// </summary>
    /// <param name="services">Collection of ServiceInfo objects representing systemd services.</param>
    /// <returns>ServiceDependencyGraph containing all services, dependencies, and edges.</returns>
    private static ServiceDependencyGraph BuildGraph(IEnumerable<ServiceInfo> services)
    {
        var serviceList = services.ToList();
        var nodeData = new Dictionary<string, NodeBuilder>(StringComparer.OrdinalIgnoreCase);

        foreach (var service in serviceList)
        {
            var builder = GetOrAddBuilder(nodeData, service.UnitName);
            builder.Description = service.Description;
            builder.State = service.State;

            foreach (var dependency in service.Dependencies.Where(static dependency => !string.IsNullOrWhiteSpace(dependency)))
            {
                builder.Dependencies.Add(dependency);
                GetOrAddBuilder(nodeData, dependency).Dependents.Add(service.UnitName);
            }

            foreach (var dependent in service.Dependents.Where(static dependent => !string.IsNullOrWhiteSpace(dependent)))
            {
                builder.Dependents.Add(dependent);
                GetOrAddBuilder(nodeData, dependent).Dependencies.Add(service.UnitName);
            }
        }

        var nodes = nodeData.Values
            .Select(builder => new DependencyNode
            {
                ServiceName = builder.ServiceName,
                Description = builder.Description,
                State = builder.State,
                Dependencies = builder.Dependencies.OrderBy(name => name).ToList(),
                Dependents = builder.Dependents.OrderBy(name => name).ToList(),
                IsRootNode = builder.Dependents.Count == 0,
                IsLeafNode = builder.Dependencies.Count == 0
            })
            .OrderBy(node => node.ServiceName)
            .ToList();

        var edges = nodes
            .SelectMany(node => node.Dependencies.Select(dependency => new DependencyEdge
            {
                FromService = node.ServiceName,
                ToService = dependency,
                RelationshipType = "DependsOn"
            }))
            .OrderBy(edge => edge.FromService)
            .ThenBy(edge => edge.ToService)
            .ToList();

        return new ServiceDependencyGraph
        {
            Nodes = nodes,
            Edges = edges,
            TotalNodes = nodes.Count,
            TotalEdges = edges.Count,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Builds a subgraph containing only the specified services and their dependencies.
    /// </summary>
    /// <param name="sourceGraph">The complete source dependency graph.</param>
    /// <param name="includedServices">Set of service names to include in the subgraph.</param>
    /// <returns>ServiceDependencyGraph containing only the specified services and their relationships.</returns>
    private static ServiceDependencyGraph BuildSubgraph(ServiceDependencyGraph sourceGraph, HashSet<string> includedServices)
    {
        var nodes = sourceGraph.Nodes
            .Where(node => includedServices.Contains(node.ServiceName))
            .Select(node => new DependencyNode
            {
                ServiceName = node.ServiceName,
                Description = node.Description,
                State = node.State,
                Dependencies = node.Dependencies.Where(includedServices.Contains).ToList(),
                Dependents = node.Dependents.Where(includedServices.Contains).ToList(),
                IsRootNode = node.Dependents.Where(includedServices.Contains).Any() == false,
                IsLeafNode = node.Dependencies.Where(includedServices.Contains).Any() == false
            })
            .OrderBy(node => node.ServiceName)
            .ToList();

        var edges = sourceGraph.Edges
            .Where(edge => includedServices.Contains(edge.FromService) && includedServices.Contains(edge.ToService))
            .OrderBy(edge => edge.FromService)
            .ThenBy(edge => edge.ToService)
            .ToList();

        return new ServiceDependencyGraph
        {
            Nodes = nodes,
            Edges = edges,
            TotalNodes = nodes.Count,
            TotalEdges = edges.Count,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an empty dependency graph.
    /// </summary>
    /// <returns>ServiceDependencyGraph with zero nodes and edges.</returns>
    private static ServiceDependencyGraph CreateEmptyGraph() => new()
    {
        Nodes = [],
        Edges = [],
        TotalNodes = 0,
        TotalEdges = 0,
        GeneratedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Gets or creates a NodeBuilder for the specified service name.
    /// </summary>
    /// <param name="builders">Dictionary of existing builders.</param>
    /// <param name="serviceName">Name of the service to get or create a builder for.</param>
    /// <returns>NodeBuilder instance for the specified service.</returns>
    private static NodeBuilder GetOrAddBuilder(IDictionary<string, NodeBuilder> builders, string serviceName)
    {
        if (!builders.TryGetValue(serviceName, out var builder))
        {
            builder = new NodeBuilder(serviceName);
            builders[serviceName] = builder;
        }

        return builder;
    }

    /// <summary>
    /// Builder class for creating dependency nodes with their relationships.
    /// </summary>
    private sealed class NodeBuilder(string serviceName)
    {
        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        public string ServiceName { get; } = serviceName;
        /// <summary>
        /// Gets or sets the description of the service.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the state of the service.
        /// </summary>
        public ServiceState State { get; set; } = ServiceState.Unknown;
        /// <summary>
        /// Gets the collection of services this service depends on.
        /// </summary>
        public HashSet<string> Dependencies { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Gets the collection of services that depend on this service.
        /// </summary>
        public HashSet<string> Dependents { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
    }
