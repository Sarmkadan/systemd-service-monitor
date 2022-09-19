#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Enums;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class AlertRulesEngineTests
{
    private readonly ILogger<AlertRulesEngine> _logger;
    private readonly IOnCallScheduleService _onCallService;
    private readonly AlertOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AlertRulesEngine _engine;

    public AlertRulesEngineTests()
    {
        _logger = Substitute.For<ILogger<AlertRulesEngine>>();
        _onCallService = Substitute.For<IOnCallScheduleService>();
        _options = new AlertOptions
        {
            Enabled = true,
            MaxRules = 100,
            MaxIncidents = 1000
        };
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _engine = new AlertRulesEngine(_logger, _onCallService, Options.Create(_options), _httpClientFactory);
    }

    [Fact]
    public async Task GetRulesAsync_InitiallyEmpty_ReturnsEmptyCollection()
    {
        // Act
        var rules = await _engine.GetRulesAsync();

        // Assert
        rules.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRuleAsync_WithValidRule_ReturnsRule()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            Name = "High CPU Alert",
            ServicePattern = "nginx*",
            Severity = AlertSeverity.Critical,
            Condition = "cpu > 80",
            Enabled = true
        };

        // Act
        var result = await _engine.AddRuleAsync(rule);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(rule.Id);
        result.Name.Should().Be(rule.Name);
    }

    [Fact]
    public async Task GetRulesAsync_AfterAddingRule_ReturnsAddedRule()
    {
        // Arrange
        var rule = new AlertRule
        {
            Id = Guid.NewGuid(),
            Name = "Test Rule",
            ServicePattern = "test*",
            Severity = AlertSeverity.Warning,
            Condition = "memory > 70"
        };

        // Act
        await _engine.AddRuleAsync(rule);
        var rules = await _engine.GetRulesAsync();

        // Assert
        rules.Should().ContainSingle(r => r.Id == rule.Id);
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithExistingRule_ReturnsRule()
    {
        // Arrange
        var ruleId = Guid.NewGuid();
        var rule = new AlertRule
        {
            Id = ruleId,
            Name = "CPU Alert",
            ServicePattern = "app*",
            Severity = AlertSeverity.Critical,
            Condition = "cpu > 90"
        };
        await _engine.AddRuleAsync(rule);

        // Act
        var result = await _engine.GetRuleByIdAsync(ruleId);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(ruleId);
    }

    [Fact]
    public async Task GetRuleByIdAsync_WithNonExistentRule_ReturnsNull()
    {
        // Act
        var result = await _engine.GetRuleByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddMultipleRules_ReturnsAllRules()
    {
        // Arrange
        var rules = new[]
        {
            new AlertRule { Id = Guid.NewGuid(), Name = "Rule 1", ServicePattern = "svc1*" },
            new AlertRule { Id = Guid.NewGuid(), Name = "Rule 2", ServicePattern = "svc2*" },
            new AlertRule { Id = Guid.NewGuid(), Name = "Rule 3", ServicePattern = "svc3*" }
        };

        // Act
        foreach (var rule in rules)
            await _engine.AddRuleAsync(rule);

        var allRules = await _engine.GetRulesAsync();

        // Assert
        allRules.Should().HaveCount(3);
        allRules.Should().Contain(r => r.Name == "Rule 1");
        allRules.Should().Contain(r => r.Name == "Rule 2");
        allRules.Should().Contain(r => r.Name == "Rule 3");
    }

    [Fact]
    public async Task GetRulesAsync_ReturnsSortedByName()
    {
        // Arrange
        await _engine.AddRuleAsync(new AlertRule { Id = Guid.NewGuid(), Name = "Zebra Rule", ServicePattern = "z*" });
        await _engine.AddRuleAsync(new AlertRule { Id = Guid.NewGuid(), Name = "Apple Rule", ServicePattern = "a*" });
        await _engine.AddRuleAsync(new AlertRule { Id = Guid.NewGuid(), Name = "Monkey Rule", ServicePattern = "m*" });

        // Act
        var rules = await _engine.GetRulesAsync();

        // Assert
        rules.Should().BeInAscendingOrder(r => r.Name);
    }

    [Fact]
    public async Task AddRuleAsync_WithNullRule_ThrowsArgumentNullException()
    {
        // Act & Assert
        await _engine.Invoking(e => e.AddRuleAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRuleAsync_MultipleTimesWithSameId_ReplacesRule()
    {
        // Arrange
        var ruleId = Guid.NewGuid();
        var rule1 = new AlertRule { Id = ruleId, Name = "Original", ServicePattern = "test*" };
        var rule2 = new AlertRule { Id = ruleId, Name = "Updated", ServicePattern = "test*" };

        // Act
        await _engine.AddRuleAsync(rule1);
        await _engine.AddRuleAsync(rule2);
        var result = await _engine.GetRuleByIdAsync(ruleId);

        // Assert
        result?.Name.Should().Be("Updated");
    }
}
