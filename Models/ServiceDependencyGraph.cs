#nullable enable

using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Represents a node in the service dependency graph.
/// </summary>
public class DependencyNode
{
    /// <summary>
    /// Gets or sets the name of the service.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief description of the service.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current state of the service.
    /// </summary>
    public ServiceState State { get; set; } = ServiceState.Unknown;

    /// <summary>
    /// Gets or sets a list of services that this service depends on.
    /// </summary>
    public List<string> Dependencies { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of services that depend on this service.
    /// </summary>
    public List<string> Dependents { get; set; } = [];

    /// <summary>
    /// Gets a value indicating whether this node is a root node in the graph.
    /// </summary>
    public bool IsRootNode { get; set; }

    /// <summary>
    /// Gets a value indicating whether this node is a leaf node in the graph.
    /// </summary>
    public bool IsLeafNode { get; set; }
}

/// <summary>
/// Represents an edge in the service dependency graph.
/// </summary>
public class DependencyEdge
{
    /// <summary>
    /// Gets or sets the name of the service that this edge originates from.
    /// </summary>
    public string FromService { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the service that this edge points to.
    /// </summary>
    public string ToService { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of relationship between the two services.
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;
}

/// <summary>
/// Represents the service dependency graph.
/// </summary>
public class ServiceDependencyGraph
{
    /// <summary>
    /// Gets or sets a list of nodes in the graph.
    /// </summary>
    public List<DependencyNode> Nodes { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of edges in the graph.
    /// </summary>
    public List<DependencyEdge> Edges { get; set; } = [];

    /// <summary>
    /// Gets the total number of nodes in the graph.
    /// </summary>
    public int TotalNodes { get; set; }

    /// <summary>
    /// Gets the total number of edges in the graph.
    /// </summary>
    public int TotalEdges { get; set; }

    /// <summary>
    /// Gets the date and time when the graph was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
