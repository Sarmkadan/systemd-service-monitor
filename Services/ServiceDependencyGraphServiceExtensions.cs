#nullable enable

using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

public static class ServiceDependencyGraphServiceExtensions
{
    /// <summary>
    /// Filters the dependency graph to only include services that match the given predicate.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="predicate">Predicate to filter services by name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Filtered dependency graph containing only matching services</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/></exception>
    public static async Task<ServiceDependencyGraph> FilterGraphAsync(
        this ServiceDependencyGraphService service,
        Func<string, bool> predicate,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var graph = await service.BuildGraphAsync(ct);
        var filteredNodes = graph.Nodes
            .Where(node => predicate(node.ServiceName))
            .ToList();

        var filteredServiceNames = filteredNodes
            .SelectMany(node => node.Dependencies.Concat(node.Dependents))
            .Concat(filteredNodes.Select(node => node.ServiceName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredEdges = graph.Edges
            .Where(edge => filteredServiceNames.Contains(edge.FromService) && filteredServiceNames.Contains(edge.ToService))
            .ToList();

        return new ServiceDependencyGraph
        {
            Nodes = filteredNodes,
            Edges = filteredEdges,
            TotalNodes = filteredNodes.Count,
            TotalEdges = filteredEdges.Count,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets all services that directly depend on the specified service.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="serviceName">Name of the service to find dependents for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of services that depend on the specified service</returns>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<IEnumerable<DependencyNode>> GetDependentServicesAsync(
        this ServiceDependencyGraphService service,
        string serviceName,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var graph = await service.BuildGraphAsync(ct);

        return graph.Nodes
            .Where(node => node.Dependencies.Contains(serviceName, StringComparer.OrdinalIgnoreCase))
            .OrderBy(node => node.ServiceName)
            .ToList();
    }

    /// <summary>
    /// Gets all services that the specified service directly depends on.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="serviceName">Name of the service to find dependencies for</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of services that the specified service depends on</returns>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<IEnumerable<DependencyNode>> GetServiceDependenciesAsync(
        this ServiceDependencyGraphService service,
        string serviceName,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var graph = await service.BuildGraphAsync(ct);

        return graph.Nodes
            .Where(node => node.Dependents.Contains(serviceName, StringComparer.OrdinalIgnoreCase))
            .OrderBy(node => node.ServiceName)
            .ToList();
    }

    /// <summary>
    /// Gets a flattened list of all services in the dependency graph.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All services in the graph, ordered by name</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    public static async Task<IEnumerable<DependencyNode>> GetAllServicesAsync(
        this ServiceDependencyGraphService service,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var graph = await service.BuildGraphAsync(ct);
        return graph.Nodes.OrderBy(node => node.ServiceName);
    }

    /// <summary>
    /// Checks if there is a circular dependency involving the specified service.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="serviceName">Name of the service to check for circular dependencies</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if circular dependency exists, false otherwise</returns>
    /// <exception cref="ArgumentException"><paramref name="serviceName"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<bool> HasCircularDependencyAsync(
        this ServiceDependencyGraphService service,
        string serviceName,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        var graph = await service.BuildGraphAsync(ct);

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recursionStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return CheckCircularDependency(serviceName, graph, visited, recursionStack);
    }

    private static bool CheckCircularDependency(
        string currentService,
        ServiceDependencyGraph graph,
        HashSet<string> visited,
        HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(currentService))
        {
            return true; // Circular dependency found
        }

        if (visited.Contains(currentService))
        {
            return false; // Already checked, no circular dependency
        }

        visited.Add(currentService);
        recursionStack.Add(currentService);

        var currentNode = graph.Nodes.FirstOrDefault(node => string.Equals(node.ServiceName, currentService, StringComparison.OrdinalIgnoreCase));
        if (currentNode is not null)
        {
            foreach (var dependency in currentNode.Dependencies)
            {
                if (CheckCircularDependency(dependency, graph, visited, recursionStack))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(currentService);
        return false;
    }

    /// <summary>
    /// Gets the longest dependency chain in the graph.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Longest dependency chain found, or empty if no chains exist</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/></exception>
    public static async Task<IEnumerable<string>> GetLongestDependencyChainAsync(
        this ServiceDependencyGraphService service,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        var graph = await service.BuildGraphAsync(ct);

        var longestChain = new List<string>();

        foreach (var node in graph.Nodes)
        {
            var chain = await service.GetLongestChainFromServiceAsync(node.ServiceName, ct);
            if (chain.Count() > longestChain.Count)
            {
                longestChain = chain.ToList();
            }
        }

        return longestChain;
    }

    /// <summary>
    /// Gets the longest dependency chain starting from a specific service.
    /// </summary>
    /// <param name="service">The service dependency graph service</param>
    /// <param name="fromService">Starting service name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Longest dependency chain from the specified service</returns>
    /// <exception cref="ArgumentException"><paramref name="fromService"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static async Task<IEnumerable<string>> GetLongestChainFromServiceAsync(
        this ServiceDependencyGraphService service,
        string fromService,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromService);

        var graph = await service.BuildGraphAsync(ct);

        var longestChain = new List<string>();
        FindLongestChain(fromService, graph, new List<string>(), new HashSet<string>(), longestChain);

        return longestChain;
    }

    private static void FindLongestChain(
        string currentService,
        ServiceDependencyGraph graph,
        List<string> currentChain,
        HashSet<string> visited,
        List<string> longestChain)
    {
        if (visited.Contains(currentService))
        {
            return;
        }

        visited.Add(currentService);
        currentChain.Add(currentService);

        var currentNode = graph.Nodes.FirstOrDefault(node => string.Equals(node.ServiceName, currentService, StringComparison.OrdinalIgnoreCase));
        if (currentNode is not null)
        {
            foreach (var dependency in currentNode.Dependencies.Order())
            {
                FindLongestChain(dependency, graph, currentChain, new HashSet<string>(visited), longestChain);
            }
        }

        if (currentChain.Count > longestChain.Count)
        {
            longestChain.Clear();
            longestChain.AddRange(currentChain);
        }

        currentChain.RemoveAt(currentChain.Count - 1);
    }
}