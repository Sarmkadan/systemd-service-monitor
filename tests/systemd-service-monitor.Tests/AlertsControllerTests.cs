#nullable enable

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Controllers;
using SystemdServiceMonitor.Dtos;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;
using SystemdServiceMonitor.Services;

namespace SystemdServiceMonitor.Tests;

/// <summary>
/// Tests for the AlertsController class.
/// </summary>
public class AlertsControllerTests
{
    private readonly IAlertRulesEngine _alertRulesEngine = Substitute.For<IAlertRulesEngine>();
    private readonly ILogger<AlertsController> _logger = Substitute.For<ILogger<AlertsController>>();

    /// <summary>
    /// Tests that the GetRules method returns an OK response with mapped rules.
    /// </summary>
    [Fact]
    public async Task GetRules_ReturnsOkResponseWithMappedRules()
    {
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var updatedAt = DateTime.UtcNow;
        _alertRulesEngine.GetRulesAsync(Arg.Any<CancellationToken>()).Returns(new[]
        {
            new AlertRule
            {
                Id = Guid.NewGuid(),
                Name = "CPU threshold",
                Description = "Tracks CPU spikes",
                ServicePattern = "api.service",
                Condition = AlertCondition.CpuThresholdExceeded,
                Threshold = 85,
                Severity = AlertSeverity.High,
                IsEnabled = true,
                CooldownMinutes = 10,
                ConsecutiveEvaluationsRequired = 2,
                Tags = ["cpu", "api"],
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        });

        var controller = new AlertsController(_alertRulesEngine, _logger);

        var actionResult = await controller.GetRules(CancellationToken.None);

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<AlertRuleDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().ContainSingle();
        response.Data![0].Name.Should().Be("CPU threshold");
        response.Data[0].Tags.Should().BeEquivalentTo(["cpu", "api"]);
    }

    /// <summary>
    /// Tests that the CreateRule method returns a Created response with a mapped rule.
    /// </summary>
    [Fact]
    public async Task CreateRule_ReturnsCreatedResponseWithMappedRule()
    {
        var ruleId = Guid.NewGuid();
        _alertRulesEngine
            .AddRuleAsync(Arg.Any<AlertRule>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var rule = callInfo.Arg<AlertRule>();
                rule.Id = ruleId;
                return rule;
            });

        var controller = new AlertsController(_alertRulesEngine, _logger);
        var request = new CreateAlertRuleDto(
            Name: "Memory threshold",
            Description: "Tracks memory growth",
            ServicePattern: "worker.service",
            Condition: AlertCondition.MemoryThresholdExceeded,
            Threshold: 1024,
            Severity: AlertSeverity.Critical,
            IsEnabled: true,
            CooldownMinutes: 20,
            ConsecutiveEvaluationsRequired: 3,
            Tags: ["memory"]);

        var actionResult = await controller.CreateRule(request, CancellationToken.None);

        var createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(AlertsController.GetRuleById));
        var response = createdResult.Value.Should().BeOfType<ApiResponse<AlertRuleDto>>().Subject;
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(ruleId);
        response.Data.Name.Should().Be("Memory threshold");
        await _alertRulesEngine.Received(1).AddRuleAsync(Arg.Is<AlertRule>(rule =>
            rule.ServicePattern == "worker.service" &&
            rule.Condition == AlertCondition.MemoryThresholdExceeded &&
            rule.Tags.SequenceEqual(new[] { "memory" })), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests that the AcknowledgeIncident method returns a NotFound response when the incident is missing.
    /// </summary>
    [Fact]
    public async Task AcknowledgeIncident_WhenIncidentIsMissing_ReturnsNotFound()
    {
        var incidentId = Guid.NewGuid();
        _alertRulesEngine.GetIncidentByIdAsync(incidentId, Arg.Any<CancellationToken>()).Returns((AlertIncident?)null);
        var controller = new AlertsController(_alertRulesEngine, _logger);

        var actionResult = await controller.AcknowledgeIncident(
            incidentId,
            new AcknowledgeIncidentDto("operator-1"),
            CancellationToken.None);

        var notFoundResult = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<AlertIncidentDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain(incidentId.ToString());
    }
}
