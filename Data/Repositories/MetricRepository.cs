// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// In-memory implementation of IMetricRepository for time-series metric data access.
/// </summary>
public class MetricRepository : IMetricRepository
{
    private readonly Dictionary<Guid, ServiceMetric> _metrics = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<ServiceMetric?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metrics.TryGetValue(id, out var metric) ? metric : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceMetric>> GetByServiceIdAsync(Guid serviceId, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metrics.Values
                .Where(m => m.ServiceInfoId == serviceId)
                .OrderByDescending(m => m.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceMetric>> GetByMetricTypeAsync(MetricType type, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metrics.Values
                .Where(m => m.MetricType == type)
                .OrderByDescending(m => m.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceMetric>> GetTimeSeriesAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var cutoff = DateTime.UtcNow.Subtract(timeRange);
            return _metrics.Values
                .Where(m => m.ServiceInfoId == serviceId &&
                           m.MetricType == type &&
                           m.Timestamp >= cutoff)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceMetric>> GetRecentAsync(int hours, int limit = 1000, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var cutoff = DateTime.UtcNow.AddHours(-hours);
            return _metrics.Values
                .Where(m => m.Timestamp >= cutoff)
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceMetric> CreateAsync(ServiceMetric metric, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _metrics[metric.Id] = metric;
            return metric;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> CreateBatchAsync(IEnumerable<ServiceMetric> metrics, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            int count = 0;
            foreach (var metric in metrics)
            {
                _metrics[metric.Id] = metric;
                count++;
            }
            return count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metrics.Remove(id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> DeleteOlderThanAsync(DateTime before, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var idsToDelete = _metrics.Values
                .Where(m => m.Timestamp < before)
                .Select(m => m.Id)
                .ToList();

            foreach (var id in idsToDelete)
                _metrics.Remove(id);

            return idsToDelete.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceMetric?> GetLatestAsync(Guid serviceId, MetricType type, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _metrics.Values
                .Where(m => m.ServiceInfoId == serviceId && m.MetricType == type)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<decimal> GetAverageAsync(Guid serviceId, MetricType type, TimeSpan timeRange, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var cutoff = DateTime.UtcNow.Subtract(timeRange);
            var metrics = _metrics.Values
                .Where(m => m.ServiceInfoId == serviceId &&
                           m.MetricType == type &&
                           m.Timestamp >= cutoff)
                .ToList();

            return metrics.Any() ? metrics.Average(m => m.Value) : 0;
        }
        finally
        {
            _lock.Release();
        }
    }
}
