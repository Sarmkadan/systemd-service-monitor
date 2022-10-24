using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Benchmarks;

/// <summary>
/// Benchmark class for ServiceRepository.
/// </summary>
[MemoryDiagnoser]
public class ServiceRepositoryBenchmarks
{
    private readonly ServiceRepository _repository = new();
    private readonly Consumer _consumer = new();

    /// <summary>
    /// Sets up the benchmark by populating the repository with 1000 services.
    /// </summary>
    [GlobalSetup]
    public async Task Setup()
    {
        // Populate with some data
        for (int i = 0; i < 1000; i++)
        {
            var service = new ServiceInfo 
            { 
                Id = Guid.NewGuid(), 
                UnitName = $"service{i}.service" 
            };
            await _repository.CreateAsync(service);
        }
    }

    /// <summary>
    /// Benchmark for retrieving all services from the repository.
    /// </summary>
    [Benchmark]
    public void GetAllServices()
    {
        _repository.GetAllAsync().GetAwaiter().GetResult().Consume(_consumer);
    }

    /// <summary>
    /// Benchmark for retrieving a service by unit name from the repository.
    /// </summary>
    /// <param name="unitName">The unit name to retrieve.</param>
    /// <returns>The service with the specified unit name, or null if not found.</returns>
    [Benchmark]
    public ServiceInfo? GetByUnitName()
    {
        return _repository.GetByUnitNameAsync("service500.service").GetAwaiter().GetResult();
    }
}
