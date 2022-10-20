#nullable enable

using FluentAssertions;
using NSubstitute;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Tests;

/// <summary>
/// Tests for the ServiceDependencyGraphService class.
/// </summary>
public class ServiceDependencyGraphTests
{
    /// <summary>
    /// Tests that the BuildGraphAsync method creates nodes and edges from dependencies.
    /// </summary>
    [Fact]
    public async Task BuildGraphAsync_CreatesNodesAndEdgesFromDependencies()
    {
        var repository = Substitute.For<IServiceRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[]
        {
            new ServiceInfo
            {
                UnitName = "api.service",
                Description = "API",
                State = ServiceState.Active,
                Dependencies = ["database.service", "cache.service"]
            },
            new ServiceInfo
            {
                UnitName = "database.service",
                Description = "Database",
                State = ServiceState.Active,
                Dependents = ["api.service"]
            },
            new ServiceInfo
            {
                UnitName = "cache.service",
                Description = "Cache",
                State = ServiceState.Inactive,
                Dependents = ["api.service"]
            }
        });

        var service = new ServiceDependencyGraphService(repository);

        var graph = await service.BuildGraphAsync();

        graph.TotalNodes.Should().Be(3);
        graph.TotalEdges.Should().Be(2);
        graph.Nodes.Should().Contain(node => node.ServiceName == "api.service" && !node.IsLeafNode && node.IsRootNode);
        graph.Nodes.Should().Contain(node => node.ServiceName == "database.service" && node.IsLeafNode && !node.IsRootNode);
        graph.Edges.Should().Contain(edge => edge.FromService == "api.service" && edge.ToService == "database.service" && edge.RelationshipType == "DependsOn");
    }

    /// <summary>
    /// Tests that the GetDependencyChainAsync method returns the shortest path when a chain exists.
    /// </summary>
    [Fact]
    public async Task GetDependencyChainAsync_WhenChainExists_ReturnsShortestPath()
    {
        var repository = Substitute.For<IServiceRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[]
        {
            new ServiceInfo
            {
                UnitName = "frontend.service",
                Dependencies = ["api.service"]
            },
            new ServiceInfo
            {
                UnitName = "api.service",
                Dependencies = ["database.service"]
            },
            new ServiceInfo
            {
                UnitName = "database.service"
            }
        });

        var service = new ServiceDependencyGraphService(repository);

        var path = (await service.GetDependencyChainAsync("frontend.service", "database.service")).ToList();

        path.Should().Equal("frontend.service", "api.service", "database.service");
    }

    /// <summary>
    /// Tests that the BuildGraphForServiceAsync method returns a local subgraph with a depth limit.
    /// </summary>
    [Fact]
    public async Task BuildGraphForServiceAsync_WithDepthLimit_ReturnsLocalSubgraph()
    {
        var repository = Substitute.For<IServiceRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new[]
        {
            new ServiceInfo
            {
                UnitName = "frontend.service",
                Dependencies = ["api.service"]
            },
            new ServiceInfo
            {
                UnitName = "api.service",
                Dependencies = ["database.service"],
                Dependents = ["frontend.service"]
            },
            new ServiceInfo
            {
                UnitName = "database.service",
                Dependents = ["api.service"]
            }
        });

        var service = new ServiceDependencyGraphService(repository);

        var graph = await service.BuildGraphForServiceAsync("api.service", depth: 1);

        graph.Nodes.Select(node => node.ServiceName).Should().BeEquivalentTo(["api.service", "frontend.service", "database.service"]);
    }
}
