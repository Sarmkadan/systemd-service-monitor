// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// Repository interface for ServiceInfo CRUD operations and queries.
/// </summary>
public interface IServiceRepository
{
    Task<ServiceInfo?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceInfo?> GetByUnitNameAsync(string unitName, CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> GetByUserAsync(string username, CancellationToken ct = default);
    Task<ServiceInfo> CreateAsync(ServiceInfo service, CancellationToken ct = default);
    Task<ServiceInfo> UpdateAsync(ServiceInfo service, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<IEnumerable<ServiceInfo>> SearchAsync(string query, CancellationToken ct = default);
}
