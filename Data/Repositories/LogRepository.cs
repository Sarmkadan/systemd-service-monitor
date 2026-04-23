#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// In-memory implementation of ILogRepository for service log data access.
/// </summary>
public class LogRepository : ILogRepository
{
    private readonly Dictionary<Guid, ServiceLog> _logs = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<ServiceLog?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.TryGetValue(id, out var log) ? log : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetByUnitNameAsync(string unitName, int limit = 100, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.Values
                .Where(l => l.UnitName == unitName)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetByServiceIdAsync(Guid serviceId, int limit = 100, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.Values
                .Where(l => l.ServiceInfoId == serviceId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetByLevelAsync(SyslogLevel level, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.Values
                .Where(l => l.Level == level)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetRecentAsync(TimeSpan timeRange, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var cutoff = DateTime.UtcNow.Subtract(timeRange);
            return _logs.Values
                .Where(l => l.Timestamp >= cutoff)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> GetByProcessIdAsync(int processId, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.Values
                .Where(l => l.ProcessId == processId)
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceLog> CreateAsync(ServiceLog log, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _logs[log.Id] = log;
            return log;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> CreateBatchAsync(IEnumerable<ServiceLog> logs, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            int count = 0;
            foreach (var log in logs)
            {
                _logs[log.Id] = log;
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
            return _logs.Remove(id);
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
            var idsToDelete = _logs.Values
                .Where(l => l.Timestamp < before)
                .Select(l => l.Id)
                .ToList();

            foreach (var id in idsToDelete)
                _logs.Remove(id);

            return idsToDelete.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> GetCountAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _logs.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceLog>> SearchAsync(string searchTerm, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var lower = searchTerm.ToLower();
            return _logs.Values
                .Where(l => l.Message.ToLower().Contains(lower) ||
                           l.UnitName.ToLower().Contains(lower))
                .OrderByDescending(l => l.Timestamp)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}
