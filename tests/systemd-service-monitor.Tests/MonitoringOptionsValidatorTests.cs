using FluentAssertions;
using SystemdServiceMonitor.Configuration;

namespace SystemdServiceMonitor.Tests;

public class MonitoringOptionsValidatorTests
{
    private static readonly PrometheusExporterOptionsValidator ExporterValidator = new();
    private static readonly ServiceMonitorOptionsValidator MonitorValidator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenFullConfigurationIsValid()
    {
        // Arrange
        var exporter = new PrometheusExporterOptions
        {
            Port = 9100,
            MetricsPath = "/metrics",
            MetricPrefix = "systemd_"
        };
        var monitor = new ServiceMonitorOptions
        {
            PollingIntervalSeconds = 5,
            MonitoredUnits = ["nginx.service", "sshd.service"]
        };

        // Act
        var exporterResult = ExporterValidator.Validate(null, exporter);
        var monitorResult = MonitorValidator.Validate(null, monitor);

        // Assert
        exporterResult.Succeeded.Should().BeTrue();
        monitorResult.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Validate_ShouldFail_WhenPortIsOutOfRange(int port)
    {
        // Arrange
        var options = new PrometheusExporterOptions { Port = port };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("PrometheusExporter:Port");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    public void Validate_ShouldSucceed_WhenPortIsAtBoundary(int port)
    {
        // Arrange
        var options = new PrometheusExporterOptions { Port = port };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("metrics")]
    [InlineData("")]
    [InlineData(" /metrics")]
    public void Validate_ShouldFail_WhenMetricsPathDoesNotStartWithSlash(string metricsPath)
    {
        // Arrange
        var options = new PrometheusExporterOptions { MetricsPath = metricsPath };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("PrometheusExporter:MetricsPath");
    }

    [Theory]
    [InlineData("systemd_")]
    [InlineData("_systemd")]
    [InlineData(":systemd")]
    [InlineData("Systemd2")]
    public void Validate_ShouldSucceed_WhenMetricPrefixFollowsPrometheusRules(string metricPrefix)
    {
        // Arrange
        var options = new PrometheusExporterOptions { MetricPrefix = metricPrefix };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("1systemd")]
    [InlineData("systemd-")]
    [InlineData("sys tem")]
    [InlineData("systemd.")]
    [InlineData("systemd\n")]
    public void Validate_ShouldFail_WhenMetricPrefixBreaksPrometheusRules(string metricPrefix)
    {
        // Arrange
        var options = new PrometheusExporterOptions { MetricPrefix = metricPrefix };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("PrometheusExporter:MetricPrefix");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ShouldFail_WhenPollingIntervalIsBelowOneSecond(int pollingIntervalSeconds)
    {
        // Arrange
        var options = new ServiceMonitorOptions
        {
            PollingIntervalSeconds = pollingIntervalSeconds,
            MonitoredUnits = ["nginx.service"]
        };

        // Act
        var result = MonitorValidator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("ServiceMonitor:PollingIntervalSeconds");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenPollingIntervalIsOneSecond()
    {
        // Arrange
        var options = new ServiceMonitorOptions
        {
            PollingIntervalSeconds = 1,
            MonitoredUnits = ["nginx.service"]
        };

        // Act
        var result = MonitorValidator.Validate(null, options);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenMonitoredUnitsIsEmpty()
    {
        // Arrange
        var options = new ServiceMonitorOptions { MonitoredUnits = [] };

        // Act
        var result = MonitorValidator.Validate(null, options);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle()
            .Which.Should().Contain("ServiceMonitor:MonitoredUnits");
    }

    [Fact]
    public void Validate_ShouldReportEveryProblem_WhenExporterHasSeveralInvalidProperties()
    {
        // Arrange
        var options = new PrometheusExporterOptions
        {
            Port = 0,
            MetricsPath = "metrics",
            MetricPrefix = "bad-prefix"
        };

        // Act
        var result = ExporterValidator.Validate(null, options);

        // Assert
        result.Failures.Should().HaveCount(3);
        result.FailureMessage.Should().Contain("PrometheusExporter:Port")
            .And.Contain("PrometheusExporter:MetricsPath")
            .And.Contain("PrometheusExporter:MetricPrefix");
    }
}
