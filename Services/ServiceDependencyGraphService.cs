#nullable enable

using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

public class ServiceDependencyGraphService(IServiceRepository serviceRepository) : IServiceDependencyGraphService
{
    private readonly ILogger<ServiceDependencyGraphService>? _logger = null;

    public async Task<ServiceDependencyGraph> BuildGraphAsync(CancellationToken ct = default)
    {
        _logger?.LogInformation("Building dependency graph for all services");
        var services = await serviceRepository.GetAllAsync(ct);
        var graph = BuildGraph(services);
        _logger?.LogInformation("Dependency graph built successfully: {NodeCount} nodes, {EdgeCount} edges", graph.TotalNodes, graph.TotalEdges);
        return graph;
    }

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

    private static ServiceDependencyGraph CreateEmptyGraph() => new()
    {
        Nodes = [],
        Edges = [],
        TotalNodes = 0,
        TotalEdges = 0,
        GeneratedAt = DateTime.UtcNow
    };

    private static NodeBuilder GetOrAddBuilder(IDictionary<string, NodeBuilder> builders, string serviceName)
    {
        if (!builders.TryGetValue(serviceName, out var builder))
        {
            builder = new NodeBuilder(serviceName);
            builders[serviceName] = builder;
        }

        return builder;
    }

    private sealed class NodeBuilder(string serviceName)
    {
        public string ServiceName { get; } = serviceName;
        public string Description { get; set; } = string.Empty;
        public ServiceState State { get; set; } = ServiceState.Unknown;
        public HashSet<string> Dependencies { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Dependents { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
