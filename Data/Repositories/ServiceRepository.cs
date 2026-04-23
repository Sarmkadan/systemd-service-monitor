#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Data.Repositories;

/// <summary>
/// In-memory implementation of IServiceRepository for service unit data access.
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly Dictionary<Guid, ServiceInfo> _services = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<ServiceInfo?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.TryGetValue(id, out var service) ? service : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceInfo?> GetByUnitNameAsync(string unitName, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values.FirstOrDefault(s => s.UnitName == unitName);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values.OrderBy(s => s.UnitName).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetActiveServicesAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values
                .Where(s => s.State == Enums.ServiceState.Active)
                .OrderBy(s => s.UnitName)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetFailedServicesAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values
                .Where(s => s.State == Enums.ServiceState.Failed)
                .OrderBy(s => s.UnitName)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetByUserAsync(string username, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values
                .Where(s => s.RunAsUser == username)
                .OrderBy(s => s.UnitName)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceInfo> CreateAsync(ServiceInfo service, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_services.ContainsKey(service.Id))
                throw new InvalidOperationException($"Service with ID {service.Id} already exists");

            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            _services[service.Id] = service;
            return service;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceInfo> UpdateAsync(ServiceInfo service, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (!_services.ContainsKey(service.Id))
                throw new KeyNotFoundException($"Service with ID {service.Id} not found");

            service.UpdatedAt = DateTime.UtcNow;
            _services[service.Id] = service;
            return service;
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
            return _services.Remove(id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Count;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _services.Values
                .OrderBy(s => s.UnitName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<ServiceInfo>> SearchAsync(string query, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var lowerQuery = query.ToLower();
            return _services.Values
                .Where(s => s.UnitName.ToLower().Contains(lowerQuery) ||
                           s.Description.ToLower().Contains(lowerQuery))
                .OrderBy(s => s.UnitName)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }
}
