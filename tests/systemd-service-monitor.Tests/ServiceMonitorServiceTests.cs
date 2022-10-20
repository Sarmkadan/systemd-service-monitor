#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

/// <summary>
/// Tests for the ServiceMonitorService class.
/// </summary>
public class ServiceMonitorServiceTests
{
    private readonly ILogger<ServiceMonitorService> _logger;
    private readonly ISystemdConnectionService _connectionService;
    private readonly IServiceRepository _repository;
    private readonly ServiceMonitorService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceMonitorServiceTests"/> class.
    /// </summary>
    public ServiceMonitorServiceTests()
    {
        _logger = Substitute.For<ILogger<ServiceMonitorService>>();
        _connectionService = Substitute.For<ISystemdConnectionService>();
        _repository = Substitute.For<IServiceRepository>();
        _service = new ServiceMonitorService(_logger, _connectionService, _repository);
    }

    /// <summary>
    /// Verifies that GetAllServicesAsync returns all services.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetAllServicesAsync logs an error and throws when the repository throws.
    /// </summary>
    /// <param name="ex">The exception thrown by the repository.</param>
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

    /// <summary>
    /// Verifies that GetServiceByNameAsync returns a service with a valid name.
    /// </summary>
    /// <param name="unitName">The name of the service to retrieve.</param>
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

    /// <summary>
    /// Verifies that GetServiceByNameAsync returns null for a non-existent service.
    /// </summary>
    /// <param name="unitName">The name of the service to retrieve.</param>
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

    /// <summary>
    /// Verifies that GetActiveServicesAsync returns only active services.
    /// </summary>
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

    /// <summary>
    /// Verifies that GetServiceByNameAsync logs an error and throws when the repository throws.
    /// </summary>
    /// <param name="unitName">The name of the service to retrieve.</param>
    /// <param name="ex">The exception thrown by the repository.</param>
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

    /// <summary>
    /// Verifies that GetAllServicesAsync returns an empty enumerable when the repository returns an empty result.
    /// </summary>
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
