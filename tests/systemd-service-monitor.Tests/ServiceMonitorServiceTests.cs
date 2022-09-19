#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ServiceMonitorServiceTests
{
    private readonly ILogger<ServiceMonitorService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly IServiceRepository _repository;
    private readonly ServiceMonitorService _service;

    public ServiceMonitorServiceTests()
    {
        _logger = Substitute.For<ILogger<ServiceMonitorService>>();
        _connectionService = Substitute.For<ISystemdConnectionService>();
        _repository = Substitute.For<IServiceRepository>();
        _service = new ServiceMonitorService(_logger, _connectionService, _repository);
    }

    [Fact]
    public async Task GetAllServicesAsync_ReturnsAllServices()
    {
        // Arrange
        var services = new List<ServiceInfo>
        {
            new() { UnitName = "nginx.service", State = ServiceState.Active },
            new() { UnitName = "postgresql.service", State = ServiceState.Active },
            new() { UnitName = "redis.service", State = ServiceState.Inactive }
        };
        _repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(services));

        // Act
        var result = await _service.GetAllServicesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(s => s.UnitName == "nginx.service");
    }

    [Fact]
    public async Task GetAllServicesAsync_WhenRepositoryThrows_LogsErrorAndThrows()
    {
        // Arrange
        var ex = new Exception("DB connection failed");
        _repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromException<IEnumerable<ServiceInfo>>(ex));

        // Act & Assert
        await _service.Invoking(s => s.GetAllServicesAsync())
            .Should().ThrowAsync<Exception>()
            .WithMessage("DB connection failed");
    }

    [Fact]
    public async Task GetServiceByNameAsync_WithValidName_ReturnsService()
    {
        // Arrange
        const string unitName = "nginx.service";
        var service = new ServiceInfo { UnitName = unitName, State = ServiceState.Active };
        _repository.GetByUnitNameAsync(unitName, default).ReturnsForAnyArgs(Task.FromResult<ServiceInfo?>(service));

        // Act
        var result = await _service.GetServiceByNameAsync(unitName);

        // Assert
        result.Should().NotBeNull();
        result?.UnitName.Should().Be(unitName);
    }

    [Fact]
    public async Task GetServiceByNameAsync_WithNonExistentName_ReturnsNull()
    {
        // Arrange
        const string unitName = "nonexistent.service";
        _repository.GetByUnitNameAsync(unitName, default).ReturnsForAnyArgs(Task.FromResult<ServiceInfo?>(null));

        // Act
        var result = await _service.GetServiceByNameAsync(unitName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveServicesAsync_ReturnsOnlyActiveServices()
    {
        // Arrange
        var activeServices = new List<ServiceInfo>
        {
            new() { UnitName = "nginx.service", State = ServiceState.Active },
            new() { UnitName = "postgresql.service", State = ServiceState.Active }
        };
        _repository.GetActiveServicesAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(activeServices));

        // Act
        var result = await _service.GetActiveServicesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.State.Should().Be(ServiceState.Active));
    }

    [Fact]
    public async Task GetServiceByNameAsync_WhenRepositoryThrows_LogsErrorAndThrows()
    {
        // Arrange
        const string unitName = "test.service";
        var ex = new Exception("Network error");
        _repository.GetByUnitNameAsync(unitName, default).ReturnsForAnyArgs(Task.FromException<ServiceInfo?>(ex));

        // Act & Assert
        await _service.Invoking(s => s.GetServiceByNameAsync(unitName))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Network error");
    }

    [Fact]
    public async Task GetAllServicesAsync_EmptyResult_ReturnsEmptyEnumerable()
    {
        // Arrange
        _repository.GetAllAsync(default).ReturnsForAnyArgs(Task.FromResult<IEnumerable<ServiceInfo>>(new List<ServiceInfo>()));

        // Act
        var result = await _service.GetAllServicesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
