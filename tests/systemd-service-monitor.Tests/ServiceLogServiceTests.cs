// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SystemdServiceMonitor.Configuration;
using SystemdServiceMonitor.Data.Repositories;
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Services;
using Xunit;

namespace SystemdServiceMonitor.Tests;

public class ServiceLogServiceTests
{
    private readonly Mock<ILogRepository> _repositoryMock;
    private readonly Mock<ILogger<ServiceLogService>> _loggerMock;
    private readonly SystemdOptions _options;
    private readonly ServiceLogService _service;

    public ServiceLogServiceTests()
    {
        _repositoryMock = new Mock<ILogRepository>();
        _loggerMock = new Mock<ILogger<ServiceLogService>>();
        _options = new SystemdOptions { MaxLogEntriesPerRequest = 1000 };
        _service = new ServiceLogService(_loggerMock.Object, _repositoryMock.Object, _options);
    }

    [Fact]
    public async Task GetLogStatisticsAsync_WithMixedLevelLogs_CountsEachLevelCorrectly()
    {
        // Arrange
        const string unitName = "nginx.service";
        var logs = new List<ServiceLog>
        {
            new() { UnitName = unitName, Level = SyslogLevel.Error,   Timestamp = DateTime.UtcNow },
            new() { UnitName = unitName, Level = SyslogLevel.Warning,  Timestamp = DateTime.UtcNow },
            new() { UnitName = unitName, Level = SyslogLevel.Warning,  Timestamp = DateTime.UtcNow },
            new() { UnitName = unitName, Level = SyslogLevel.Info,     Timestamp = DateTime.UtcNow },
        };
        _repositoryMock
            .Setup(r => r.GetByUnitNameAsync(unitName, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var stats = await _service.GetLogStatisticsAsync(unitName);

        // Assert
        stats.TotalLogEntries.Should().Be(4);
        stats.ErrorCount.Should().Be(1);
        stats.WarningCount.Should().Be(2);
        stats.InfoCount.Should().Be(1);
    }

    [Fact]
    public async Task GetLogsByLevelAsync_FiltersByRequestedLevel_ReturnsOnlyMatchingLogs()
    {
        // Arrange
        const string unitName = "sshd.service";
        var logs = new List<ServiceLog>
        {
            new() { UnitName = unitName, Level = SyslogLevel.Error },
            new() { UnitName = unitName, Level = SyslogLevel.Info },
            new() { UnitName = unitName, Level = SyslogLevel.Info },
        };
        _repositoryMock
            .Setup(r => r.GetByUnitNameAsync(unitName, _options.MaxLogEntriesPerRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = (await _service.GetLogsByLevelAsync(unitName, SyslogLevel.Info)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(l => l.Level == SyslogLevel.Info);
    }

    [Fact]
    public async Task GetLogStatisticsAsync_NoLogs_ReturnsAllZeroCounts()
    {
        // Arrange
        const string unitName = "empty.service";
        _repositoryMock
            .Setup(r => r.GetByUnitNameAsync(unitName, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceLog>());

        // Act
        var stats = await _service.GetLogStatisticsAsync(unitName);

        // Assert
        stats.TotalLogEntries.Should().Be(0);
        stats.ErrorCount.Should().Be(0);
        stats.WarningCount.Should().Be(0);
        stats.InfoCount.Should().Be(0);
        stats.UnitName.Should().Be(unitName);
    }

    [Fact]
    public async Task GetLogsInTimeRangeAsync_LogsOutsideWindow_ExcludesThemFromResult()
    {
        // Arrange
        const string unitName = "cron.service";
        var now = DateTime.UtcNow;
        var from = now.AddHours(-2);
        var to   = now.AddHours(-1);

        var logs = new List<ServiceLog>
        {
            new() { UnitName = unitName, Level = SyslogLevel.Info, Timestamp = now.AddMinutes(-90) }, // inside window
            new() { UnitName = unitName, Level = SyslogLevel.Info, Timestamp = now.AddHours(-3) },    // before range
            new() { UnitName = unitName, Level = SyslogLevel.Info, Timestamp = now },                 // after range
        };
        _repositoryMock
            .Setup(r => r.GetByUnitNameAsync(unitName, _options.MaxLogEntriesPerRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = (await _service.GetLogsInTimeRangeAsync(unitName, from, to)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Single().Timestamp.Should().Be(now.AddMinutes(-90));
    }
}
