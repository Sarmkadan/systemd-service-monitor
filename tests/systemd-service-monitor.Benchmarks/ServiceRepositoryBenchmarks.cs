using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Benchmarks;

[MemoryDiagnoser]
public class ServiceRepositoryBenchmarks
{
    private readonly ServiceRepository _repository = new();
    private readonly Consumer _consumer = new();

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

    [Benchmark]
    public void GetAllServices()
    {
        _repository.GetAllAsync().GetAwaiter().GetResult().Consume(_consumer);
    }

    [Benchmark]
    public ServiceInfo? GetByUnitName()
    {
        return _repository.GetByUnitNameAsync("service500.service").GetAwaiter().GetResult();
    }
}
