#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// Repository interface for ServiceLog CRUD operations and queries.
/// </summary>
public interface ILogRepository
{
    Task<ServiceLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> GetByUnitNameAsync(string unitName, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> GetByServiceIdAsync(Guid serviceId, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> GetByLevelAsync(SyslogLevel level, CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> GetRecentAsync(TimeSpan timeRange, CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> GetByProcessIdAsync(int processId, CancellationToken ct = default);
    Task<ServiceLog> CreateAsync(ServiceLog log, CancellationToken ct = default);
    Task<int> CreateBatchAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime before, CancellationToken ct = default);
    Task<int> GetCountAsync(CancellationToken ct = default);
    Task<IEnumerable<ServiceLog>> SearchAsync(string searchTerm, CancellationToken ct = default);
}
