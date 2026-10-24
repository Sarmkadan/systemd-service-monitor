using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Extensions;
using SystemdServiceMonitor.Models;

namespace SystemdServiceMonitor.Tests;

public class AlertRulesOptionsValidatorTests
{
    private static readonly AlertRulesOptionsValidator Validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenAllRulesAreValid()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules =
            [
                ValidRule("web-service-failed", condition: "ServiceFailed"),
                ValidRule("high-memory", condition: "MemoryThresholdExceeded", threshold: 512)
            ]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenNoRulesAreConfigured()
    {
        // Arrange
        var options = new AlertRulesOptions();

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenServicePatternIsEmpty(string servicePattern)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("nginx-down", servicePattern: servicePattern)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'nginx-down'").And.Contain("ServicePattern");
    }

    [Fact]
    public void Validate_ShouldFail_WhenServicePatternContainsWhitespace()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("spaced", servicePattern: "nginx web")]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'spaced'").And.Contain("whitespace");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameIsEmpty_AndNamesRuleByPosition()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("ok"), ValidRule(name: "")]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("AlertRules[1]").And.Contain("Name is required");
    }

    [Theory]
    [InlineData("Bogus")]
    [InlineData("ServiceFailedX")]
    [InlineData("99")]
    public void Validate_ShouldFail_WhenConditionIsUnknown(string condition)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("typo-rule", condition: condition)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'typo-rule'").And.Contain($"unknown condition type '{condition}'");
    }

    [Fact]
    public void Validate_ShouldFail_WhenConditionIsEmpty()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("no-condition", condition: "")]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'no-condition'").And.Contain("Condition is required");
    }

    [Theory]
    [InlineData("CpuThresholdExceeded", 0)]
    [InlineData("MemoryThresholdExceeded", -1)]
    [InlineData("RestartCountExceeded", 0)]
    [InlineData("UptimeBelowMinimum", -30)]
    public void Validate_ShouldFail_WhenQuantitativeThresholdIsNotPositive(string condition, decimal threshold)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("threshold-rule", condition: condition, threshold: threshold)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'threshold-rule'").And.Contain("Threshold must be greater than 0");
    }

    [Theory]
    [InlineData("ServiceFailed", 0)]
    [InlineData("HealthCheckUnhealthy", -5)]
    [InlineData("AnyStateChange", 0)]
    public void Validate_ShouldIgnoreThreshold_WhenConditionIsStateBased(string condition, decimal threshold)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("state-rule", condition: condition, threshold: threshold)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ShouldFail_WhenCooldownIsNotPositive(int cooldownMinutes)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("cooldown-rule", cooldownMinutes: cooldownMinutes)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'cooldown-rule'").And.Contain("CooldownMinutes must be greater than 0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Validate_ShouldFail_WhenConsecutiveEvaluationsIsBelowOne(int consecutive)
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("hysteresis-rule", consecutiveEvaluations: consecutive)]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'hysteresis-rule'").And.Contain("ConsecutiveEvaluationsRequired must be at least 1");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSeverityIsUnknown()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [ValidRule("severity-rule", severity: "Apocalyptic")]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'severity-rule'").And.Contain("unknown severity 'Apocalyptic'");
    }

    [Fact]
    public void Validate_ShouldFail_WhenRuleNamesAreDuplicated_IgnoringCase()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules =
            [
                ValidRule("Nginx Down"),
                ValidRule("other"),
                ValidRule("nginx down")
            ]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("'Nginx Down'").And.Contain("positions 1, 3");
    }

    [Fact]
    public void Validate_ShouldReportEveryInvalidRule_InOneResult()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules =
            [
                ValidRule("good-rule"),
                ValidRule("bad-pattern", servicePattern: ""),
                ValidRule("bad-condition", condition: "Nope"),
                ValidRule("bad-threshold", condition: "CpuThresholdExceeded", threshold: 0),
                ValidRule("bad-cooldown", cooldownMinutes: 0),
                ValidRule("good-rule")
            ]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(5);
        foreach (var name in new[] { "bad-pattern", "bad-condition", "bad-threshold", "bad-cooldown" })
        {
            result.Failures.Should().Contain(failure => failure.Contains($"'{name}'"));
        }

        result.Failures.Should().ContainSingle(failure => failure.Contains("'good-rule'"))
            .Which.Should().Contain("must be unique");
    }

    [Fact]
    public void Validate_ShouldNameTheRule_InEveryFailureMessage()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules =
            [
                ValidRule("multi-problem", servicePattern: "", condition: "Nope", cooldownMinutes: -1, consecutiveEvaluations: 0)
            ]
        };

        // Act
        var result = Validator.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(4);
        result.Failures.Should().OnlyContain(failure => failure.Contains("'multi-problem'"));
    }

    [Fact]
    public void Validate_ShouldFailOnStart_WithAggregatedMessageNamingEachInvalidRule()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AlertRules:0:Name"] = "fine-rule",
                ["AlertRules:0:ServicePattern"] = "nginx*",
                ["AlertRules:0:Condition"] = "ServiceFailed",
                ["AlertRules:1:Name"] = "empty-unit",
                ["AlertRules:1:ServicePattern"] = "",
                ["AlertRules:1:Condition"] = "ServiceFailed",
                ["AlertRules:2:Name"] = "mystery-condition",
                ["AlertRules:2:ServicePattern"] = "*",
                ["AlertRules:2:Condition"] = "Telepathy"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlertRulesEngine(configuration);
        using var provider = services.BuildServiceProvider();

        // Act
        var act = () => provider.GetRequiredService<IOptions<AlertRulesOptions>>().Value;

        // Assert
        var exception = act.Should().Throw<OptionsValidationException>().Which;
        exception.Failures.Should().HaveCount(2);
        exception.Message.Should().Contain("'empty-unit'").And.Contain("'mystery-condition'");
    }

    [Fact]
    public void Validate_ShouldBindValidConfiguration_ThroughDependencyInjection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AlertRules:0:Name"] = "high-memory",
                ["AlertRules:0:ServicePattern"] = "*",
                ["AlertRules:0:Condition"] = "memorythresholdexceeded",
                ["AlertRules:0:Threshold"] = "512",
                ["AlertRules:0:Severity"] = "High",
                ["AlertRules:0:ConsecutiveEvaluationsRequired"] = "3",
                ["AlertRules:0:Tags:0"] = "resources"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAlertRulesEngine(configuration);
        using var provider = services.BuildServiceProvider();

        // Act
        var rules = provider.GetRequiredService<IOptions<AlertRulesOptions>>().Value.ToAlertRules();

        // Assert
        rules.Should().ContainSingle();
        var rule = rules[0];
        rule.Name.Should().Be("high-memory");
        rule.ServicePattern.Should().Be("*");
        rule.Condition.Should().Be(AlertCondition.MemoryThresholdExceeded);
        rule.Threshold.Should().Be(512);
        rule.Severity.Should().Be(AlertSeverity.High);
        rule.ConsecutiveEvaluationsRequired.Should().Be(3);
        rule.Tags.Should().ContainSingle().Which.Should().Be("resources");
    }

    [Fact]
    public void ToAlertRules_ShouldApplyDefaults_WhenOptionalFieldsAreOmitted()
    {
        // Arrange
        var options = new AlertRulesOptions
        {
            Rules = [new AlertRuleOptions { Name = "defaults", ServicePattern = "nginx*", Condition = "ServiceInactive" }]
        };

        // Act
        var rule = options.ToAlertRules().Single();

        // Assert
        rule.Severity.Should().Be(AlertSeverity.Medium);
        rule.CooldownMinutes.Should().Be(15);
        rule.ConsecutiveEvaluationsRequired.Should().Be(1);
        rule.IsEnabled.Should().BeTrue();
        rule.Id.Should().NotBe(Guid.Empty);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static AlertRuleOptions ValidRule(
        string name = "valid-rule",
        string servicePattern = "nginx*",
        string condition = "ServiceFailed",
        decimal threshold = 0,
        string severity = "Medium",
        int cooldownMinutes = 15,
        int consecutiveEvaluations = 1)
        => new()
        {
            Name = name,
            ServicePattern = servicePattern,
            Condition = condition,
            Threshold = threshold,
            Severity = severity,
            CooldownMinutes = cooldownMinutes,
            ConsecutiveEvaluationsRequired = consecutiveEvaluations
        };
}
