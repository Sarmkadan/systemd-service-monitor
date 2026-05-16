#nullable enable

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ApiIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetAllServices_ReturnsOkStatus()
    {
        // Act
        var response = await _client!.GetAsync("/api/services");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllServices_ReturnsValidJsonResponse()
    {
        // Act
        var response = await _client!.GetAsync("/api/services");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();

        var jsonDoc = JsonDocument.Parse(content);
        jsonDoc.RootElement.Should().HaveProperty("success");
        jsonDoc.RootElement.Should().HaveProperty("data");
    }

    [Fact]
    public async Task GetAllServices_WithPagination_ReturnsPaginatedResponse()
    {
        // Act
        var response = await _client!.GetAsync("/api/services?pageNumber=1&pageSize=10");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("\"pageNumber\"");
        content.Should().Contain("\"pageSize\"");
        content.Should().Contain("\"totalCount\"");
    }

    [Fact]
    public async Task GetMetrics_ReturnsOkStatus()
    {
        // Act
        var response = await _client!.GetAsync("/api/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSystem_ReturnsOkStatus()
    {
        // Act
        var response = await _client!.GetAsync("/api/system");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOkStatus()
    {
        // Act
        var response = await _client!.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetServiceWithEmptyName_ReturnsBadRequest()
    {
        // Act
        var response = await _client!.GetAsync("/api/services/");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultipleRequests_ToSameEndpoint_AllSucceed()
    {
        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client!.GetAsync("/api/services"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));
    }

    [Fact]
    public async Task GetMetrics_ReturnsValidJsonWithResourceData()
    {
        // Act
        var response = await _client!.GetAsync("/api/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonDoc = JsonDocument.Parse(content);
        jsonDoc.RootElement.Should().HaveProperty("data");
    }

    [Fact]
    public async Task GetSystem_ReturnsSystemResourceInformation()
    {
        // Act
        var response = await _client!.GetAsync("/api/system");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("/api/services?pageNumber=1&pageSize=10")]
    [InlineData("/api/services?pageNumber=2&pageSize=20")]
    [InlineData("/api/services?pageNumber=1&pageSize=50")]
    public async Task PaginationParametersVariations_AllSucceed(string url)
    {
        // Act
        var response = await _client!.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
