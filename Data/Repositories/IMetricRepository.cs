// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// Repository interface for ServiceMetric CRUD operations and time-series queries.
/// </summary>
public interface IMetricRepository
{
    Task<ServiceMetric?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ServiceMetric>> GetByServiceIdAsync(Guid serviceId, CancellationToken ct = default);
    Task<IEnumerable<ServiceMetric>> GetByMetricTypeAsync(MetricType type, CancellationToken ct = default);
    Task<IEnumerable<ServiceMetric>> GetTimeSeriesAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default);
    Task<IEnumerable<ServiceMetric>> GetRecentAsync(int hours, int limit = 1000, CancellationToken ct = default);
    Task<ServiceMetric> CreateAsync(ServiceMetric metric, CancellationToken ct = default);
    Task<int> CreateBatchAsync(IEnumerable<ServiceMetric> metrics, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime before, CancellationToken ct = default);
    Task<ServiceMetric?> GetLatestAsync(Guid serviceId, MetricType type, CancellationToken ct = default);
    Task<decimal> GetAverageAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default);
}
