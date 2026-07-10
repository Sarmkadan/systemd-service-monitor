#nullable enable

using Microsoft.AspNetCore.Mvc;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;

namespace SystemdServiceMonitor.Controllers;

/// <summary>
/// Extension methods for <see cref="DependencyGraphController"/> providing additional utility functionality
/// for dependency graph analysis and manipulation.
/// </summary>
public static class DependencyGraphControllerExtensions
{
    /// <summary>
    /// Gets a filtered dependency graph containing only services matching the specified criteria.
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="predicate">Filter function to select which services to include</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtered dependency graph with only matching services</returns>
    public static async Task<ActionResult<ApiResponse<ServiceDependencyGraph>>> GetFilteredGraph(
        this DependencyGraphController controller,
        Func<DependencyNode, bool> predicate,
        CancellationToken ct = default)
    {
        var graph = await controller.DependencyGraphService.BuildGraphAsync(ct);

        var filteredNodes = graph.Nodes
            .Where(predicate)
            .ToList();

        var filteredServiceNames = filteredNodes
            .SelectMany(node => node.Dependencies.Concat(node.Dependents))
            .Concat(filteredNodes.Select(node => node.ServiceName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredGraph = new ServiceDependencyGraph
        {
            Nodes = filteredNodes,
            Edges = graph.Edges
                .Where(edge => filteredServiceNames.Contains(edge.FromService) &&
                               filteredServiceNames.Contains(edge.ToService))
                .ToList(),
            GeneratedAt = graph.GeneratedAt
        };

        return controller.Ok(new ApiResponse<ServiceDependencyGraph>
        {
            Data = filteredGraph,
            Success = true,
            Message = $"Retrieved filtered dependency graph with {filteredGraph.TotalNodes} nodes"
        });
    }

    /// <summary>
    /// Gets all services that depend on the specified service (direct and indirect dependents).
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="serviceName">Name of the service to find dependents for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of services that depend on the specified service</returns>
    public static async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetDependents(
        this DependencyGraphController controller,
        string serviceName,
        CancellationToken ct = default)
    {
        var graph = await controller.DependencyGraphService.BuildGraphAsync(ct);
        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);

        if (!nodeLookup.TryGetValue(serviceName, out var targetNode))
        {
            return controller.NotFound(new ApiResponse<List<DependencyNode>>
            {
                Success = false,
                Message = $"Service '{serviceName}' not found in dependency graph"
            });
        }

        // Find all nodes that have this service in their Dependencies list (direct dependents)
        var dependents = graph.Nodes
            .Where(node => node.Dependencies.Contains(serviceName, StringComparer.OrdinalIgnoreCase))
            .OrderBy(node => node.ServiceName)
            .ToList();

        return controller.Ok(new ApiResponse<List<DependencyNode>>
        {
            Data = dependents,
            Success = true,
            Message = $"Retrieved {dependents.Count} services that depend on '{serviceName}'"
        });
    }

    /// <summary>
    /// Gets all services that the specified service depends on (direct and indirect dependencies).
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="serviceName">Name of the service to find dependencies for</param>
    /// <param name="maxDepth">Maximum depth to traverse dependencies (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of services that the specified service depends on</returns>
    public static async Task<ActionResult<ApiResponse<List<DependencyNode>>>> GetAllDependencies(
        this DependencyGraphController controller,
        string serviceName,
        int maxDepth = 10,
        CancellationToken ct = default)
    {
        var graph = await controller.DependencyGraphService.BuildGraphAsync(ct);
        var nodeLookup = graph.Nodes.ToDictionary(node => node.ServiceName, StringComparer.OrdinalIgnoreCase);

        if (!nodeLookup.TryGetValue(serviceName, out var targetNode))
        {
            return controller.NotFound(new ApiResponse<List<DependencyNode>>
            {
                Success = false,
                Message = $"Service '{serviceName}' not found in dependency graph"
            });
        }

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var dependencies = new List<DependencyNode>();
        var queue = new Queue<(string serviceName, int depth)>();

        queue.Enqueue((serviceName, 0));
        visited.Add(serviceName);

        while (queue.Count > 0)
        {
            var (currentService, currentDepth) = queue.Dequeue();

            if (currentDepth >= maxDepth)
            {
                continue;
            }

            var node = graph.Nodes.FirstOrDefault(n => string.Equals(n.ServiceName, currentService, StringComparison.OrdinalIgnoreCase));
            if (node != null && node.Dependencies.Any())
            {
                foreach (var dep in node.Dependencies.OrderBy(d => d))
                {
                    if (!visited.Contains(dep))
                    {
                        visited.Add(dep);
                        queue.Enqueue(new ValueTuple<string, int>(dep, currentDepth + 1));
                        var depNode = graph.Nodes.FirstOrDefault(n => string.Equals(n.ServiceName, dep, StringComparison.OrdinalIgnoreCase));
                        if (depNode != null)
                        {
                            dependencies.Add(depNode);
                        }
                    }
                }
            }
        }

        return controller.Ok(new ApiResponse<List<DependencyNode>>
        {
            Data = dependencies,
            Success = true,
            Message = $"Retrieved {dependencies.Count} dependencies for '{serviceName}' (depth: {maxDepth})"
        });
    }

    /// <summary>
    /// Gets a summary of the dependency graph including counts by service state.
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Summary statistics about the dependency graph</returns>
    public static async Task<ActionResult<ApiResponse<DependencyGraphSummary>>> GetGraphSummary(
        this DependencyGraphController controller,
        CancellationToken ct = default)
    {
        var graph = await controller.DependencyGraphService.BuildGraphAsync(ct);

        var summary = new DependencyGraphSummary
        {
            TotalNodes = graph.TotalNodes,
            TotalEdges = graph.TotalEdges,
            RootNodes = graph.Nodes.Count(n => n.IsRootNode),
            LeafNodes = graph.Nodes.Count(n => n.IsLeafNode),
            ActiveServices = graph.Nodes.Count(n => n.State == Enums.ServiceState.Active),
            InactiveServices = graph.Nodes.Count(n => n.State == Enums.ServiceState.Inactive),
            FailedServices = graph.Nodes.Count(n => n.State == Enums.ServiceState.Failed),
            UnknownStateServices = graph.Nodes.Count(n => n.State == Enums.ServiceState.Unknown),
            GeneratedAt = graph.GeneratedAt
        };

        return controller.Ok(new ApiResponse<DependencyGraphSummary>
        {
            Data = summary,
            Success = true,
            Message = "Retrieved dependency graph summary"
        });
    }
}

/// <summary>
/// Summary statistics for a dependency graph.
/// </summary>
public class DependencyGraphSummary
{
    /// <summary>Total number of nodes in the graph</summary>
    public int TotalNodes { get; set; }

    /// <summary>Total number of edges in the graph</summary>
    public int TotalEdges { get; set; }

    /// <summary>Number of root nodes (services with no dependencies)</summary>
    public int RootNodes { get; set; }

    /// <summary>Number of leaf nodes (services with no dependents)</summary>
    public int LeafNodes { get; set; }

    /// <summary>Number of active services</summary>
    public int ActiveServices { get; set; }

    /// <summary>Number of inactive services</summary>
    public int InactiveServices { get; set; }

    /// <summary>Number of failed services</summary>
    public int FailedServices { get; set; }

    /// <summary>Number of services with unknown state</summary>
    public int UnknownStateServices { get; set; }

    /// <summary>When the graph was generated</summary>
    public DateTime GeneratedAt { get; set; }
}