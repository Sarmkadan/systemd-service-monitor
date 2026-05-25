#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Enums;

namespace SystemdServiceMonitor.Models;

public class DependencyNode
{
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceState State { get; set; } = ServiceState.Unknown;
    public List<string> Dependencies { get; set; } = [];
    public List<string> Dependents { get; set; } = [];
    public bool IsRootNode { get; set; }
    public bool IsLeafNode { get; set; }
}

public class DependencyEdge
{
    public string FromService { get; set; } = string.Empty;
    public string ToService { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
}

public class ServiceDependencyGraph
{
    public List<DependencyNode> Nodes { get; set; } = [];
    public List<DependencyEdge> Edges { get; set; } = [];
    public int TotalNodes { get; set; }
    public int TotalEdges { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
