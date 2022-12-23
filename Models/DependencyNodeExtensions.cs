using System;
using System.Collections.Generic;
using System.Globalization;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="DependencyNode"/>.
/// </summary>
public static class DependencyNodeExtensions
{
    /// <summary>
    /// Checks if the node has any dependencies.
    /// </summary>
    /// <param name="node">The <see cref="DependencyNode"/>.</param>
    /// <returns>True if the node has dependencies, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool HasDependencies(this DependencyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Dependencies.Count > 0;
    }

    /// <summary>
    /// Checks if the node has any dependents.
    /// </summary>
    /// <param name="node">The <see cref="DependencyNode"/>.</param>
    /// <returns>True if the node has dependents, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool HasDependents(this DependencyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Dependents.Count > 0;
    }

    /// <summary>
    /// Determines if the node is isolated (no dependencies and no dependents).
    /// </summary>
    /// <param name="node">The <see cref="DependencyNode"/>.</param>
    /// <returns>True if the node is isolated, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static bool IsIsolated(this DependencyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.Dependencies.Count == 0 && node.Dependents.Count == 0;
    }

    /// <summary>
    /// Gets a descriptive string summary of the node.
    /// </summary>
    /// <param name="node">The <see cref="DependencyNode"/>.</param>
    /// <returns>A string summary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static string GetSummary(this DependencyNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return string.Format(CultureInfo.InvariantCulture, "{0} [{1}] (Deps: {2}, Dependents: {3})", 
            node.ServiceName, node.State, node.Dependencies.Count, node.Dependents.Count);
    }
}
