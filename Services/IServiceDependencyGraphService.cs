#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Services;

public interface IServiceDependencyGraphService
{
    Task<ServiceDependencyGraph> BuildGraphAsync(CancellationToken ct = default);
    Task<ServiceDependencyGraph> BuildGraphForServiceAsync(string unitName, int depth = 3, CancellationToken ct = default);
    Task<IEnumerable<string>> GetDependencyChainAsync(string fromService, string toService, CancellationToken ct = default);
    Task<IEnumerable<DependencyNode>> GetRootServicesAsync(CancellationToken ct = default);
    Task<IEnumerable<DependencyNode>> GetLeafServicesAsync(CancellationToken ct = default);
}
