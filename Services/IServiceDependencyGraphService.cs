#nullable enable

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

/// <summary>
/// Service interface for building and analyzing service dependency graphs.
/// </summary>
public interface IServiceDependencyGraphService
{
    /// <summary>
    /// Builds a complete dependency graph of all systemd services.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The complete service dependency graph.</returns>
    Task<ServiceDependencyGraph> BuildGraphAsync(CancellationToken ct = default);

    /// <summary>
    /// Builds a dependency graph for a specific service with configurable depth.
    /// </summary>
    /// <param name="unitName">The name of the systemd unit.</param>
    /// <param name="depth">The maximum depth to traverse dependencies.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The service dependency graph.</returns>
    Task<ServiceDependencyGraph> BuildGraphForServiceAsync(string unitName, int depth = 3, CancellationToken ct = default);

    /// <summary>
    /// Finds the dependency chain between two services.
    /// </summary>
    /// <param name="fromService">The starting service name.</param>
    /// <param name="toService">The target service name.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The dependency chain as a sequence of service names.</returns>
    Task<IEnumerable<string>> GetDependencyChainAsync(string fromService, string toService, CancellationToken ct = default);

    /// <summary>
    /// Gets services with no dependencies (root services).
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Collection of root service nodes.</returns>
    Task<IEnumerable<DependencyNode>> GetRootServicesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets services that are not depended upon by any other service (leaf services).
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Collection of leaf service nodes.</returns>
    Task<IEnumerable<DependencyNode>> GetLeafServicesAsync(CancellationToken ct = default);
}
