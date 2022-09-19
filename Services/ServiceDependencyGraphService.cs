#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

public class ServiceDependencyGraphService(IServiceRepository serviceRepository) : IServiceDependencyGraphService
{
    public async Task<ServiceDependencyGraph> BuildGraphAsync(CancellationToken ct = default)
    {
        var services = await serviceRepository.GetAllAsync(ct);
        return BuildGraph(services);
    }

    public async Task<ServiceDependencyGraph> BuildGraphForServiceAsync(string unitName, int depth = 3, CancellationToken ct = default)
    {
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return CreateEmptyGraph();
        }

        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);
        if (!nodeLookup.ContainsKey(unitName))
        {
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

        return BuildSubgraph(graph, visited);
    }

    public async Task<IEnumerable<string>> GetDependencyChainAsync(string fromService, string toService, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fromService) || string.IsNullOrWhiteSpace(toService))
        {
            return [];
        }

        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);
        if (!nodeLookup.ContainsKey(fromService) || !nodeLookup.ContainsKey(toService))
        {
            return [];
        }

        if (string.Equals(fromService, toService, StringComparison.OrdinalIgnoreCase))
        {
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
                    return nextPath;
                }

                visited.Add(dependency);
                queue.Enqueue(nextPath);
            }
        }

        return [];
    }

    public async Task<IEnumerable<DependencyNode>> GetRootServicesAsync(CancellationToken ct = default)
    {
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        return graph.Nodes
            .Where(node => node.IsRootNode)
            .OrderBy(node => node.ServiceName)
            .ToList();
    }

    public async Task<IEnumerable<DependencyNode>> GetLeafServicesAsync(CancellationToken ct = default)
    {
        var graph = BuildGraph(await serviceRepository.GetAllAsync(ct));
        return graph.Nodes
            .Where(node => node.IsLeafNode)
            .OrderBy(node => node.ServiceName)
            .ToList();
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
