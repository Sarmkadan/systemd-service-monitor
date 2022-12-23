#nullable enable

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SystemdServiceMonitor.Utilities;
using Xunit;

/// <summary>
/// Tests for the PerformanceMonitor class.
/// </summary>
namespace SystemdServiceMonitor.Tests;

public class PerformanceMonitorTests
{
    private readonly ILogger<PerformanceMonitor> _logger = Substitute.For<ILogger<PerformanceMonitor>>();

    /// <summary>
    /// Tests that the constructor initializes with default warning threshold.
    /// </summary>
    [Fact]
    public void Constructor_WithDefaultThreshold_InitializesCorrectly()
    {
        // Arrange & Act
        using var monitor = new PerformanceMonitor("TestOperation");

        // Assert - using reflection to check private fields
        var stopwatchField = typeof(PerformanceMonitor).GetField("_stopwatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        stopwatchField.Should().NotBeNull();
        var stopwatch = (System.Diagnostics.Stopwatch)stopwatchField!.GetValue(monitor)!;
        stopwatch.IsRunning.Should().BeTrue();
    }

    /// <summary>
    /// Tests that the constructor initializes with custom warning threshold.
    /// </summary>
    [Fact]
    public void Constructor_WithCustomThreshold_InitializesCorrectly()
    {
        // Arrange & Act
        using var monitor = new PerformanceMonitor("TestOperation", _logger, warningThresholdMs: 500);

        // Assert
        var warningThresholdField = typeof(PerformanceMonitor).GetField("_warningThresholdMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        warningThresholdField.Should().NotBeNull();
        var threshold = (long)warningThresholdField!.GetValue(monitor)!;
        threshold.Should().Be(500);
    }

    /// <summary>
    /// Tests that ElapsedMilliseconds returns increasing values over time.
    /// </summary>
    [Fact]
    public void ElapsedMilliseconds_ReturnsIncreasingValues()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation");
        var initialValue = monitor.ElapsedMilliseconds;

        // Act - wait a bit
        Thread.Sleep(50);

        // Assert
        monitor.ElapsedMilliseconds.Should().BeGreaterThan(initialValue);
    }

    /// <summary>
    /// Tests that Elapsed returns a valid TimeSpan.
    /// </summary>
    [Fact]
    public void Elapsed_ReturnsValidTimeSpan()
    {
        // Arrange & Act
        using var monitor = new PerformanceMonitor("TestOperation");
        Thread.Sleep(10);

        // Assert
        var elapsed = monitor.Elapsed;
        elapsed.Should().BePositive();
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that RecordCheckpoint records the checkpoint time.
    /// </summary>
    [Fact]
    public void RecordCheckpoint_RecordsCheckpoint()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        Thread.Sleep(10);

        // Act
        monitor.RecordCheckpoint("Checkpoint1");

        // Assert - using reflection to check checkpoints
        var checkpointsField = typeof(PerformanceMonitor).GetField("_checkpoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        checkpointsField.Should().NotBeNull();
        var checkpoints = (Dictionary<string, long>)checkpointsField!.GetValue(monitor)!;
        checkpoints.Should().ContainKey("Checkpoint1");
        checkpoints["Checkpoint1"].Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that GetCheckpoints returns all recorded checkpoints.
    /// </summary>
    [Fact]
    public void GetCheckpoints_ReturnsAllRecordedCheckpoints()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        monitor.RecordCheckpoint("Checkpoint1");
        Thread.Sleep(10);
        monitor.RecordCheckpoint("Checkpoint2");

        // Act
        var checkpoints = monitor.GetCheckpoints();

        // Assert
        checkpoints.Should().HaveCount(2);
        checkpoints.Should().ContainKeys("Checkpoint1", "Checkpoint2");
    }

    /// <summary>
    /// Tests that GetElapsedBetween calculates time between two checkpoints.
    /// </summary>
    [Fact]
    public void GetElapsedBetween_CalculatesCorrectElapsedTime()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        monitor.RecordCheckpoint("Start");
        Thread.Sleep(50);
        monitor.RecordCheckpoint("End");

        // Act
        var elapsed = monitor.GetElapsedBetween("Start", "End");

        // Assert
        elapsed.Should().BeGreaterThan(40); // Should be at least 40ms
        elapsed.Should().BeLessThan(150); // But less than 150ms
    }

    /// <summary>
    /// Tests that GetElapsedBetween returns -1 for non-existent checkpoints.
    /// </summary>
    [Fact]
    public void GetElapsedBetween_NonExistentCheckpoints_ReturnsMinusOne()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);

        // Act
        var elapsed = monitor.GetElapsedBetween("NonExistent1", "NonExistent2");

        // Assert
        elapsed.Should().Be(-1);
    }

    /// <summary>
    /// Tests that GetSummary returns a non-empty string with operation name.
    /// </summary>
    [Fact]
    public void GetSummary_ReturnsNonEmptyString()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        Thread.Sleep(10);

        // Act
        var summary = monitor.GetSummary();

        // Assert
        summary.Should().NotBeNullOrEmpty();
        summary.Should().Contain("TestOperation");
    }

    /// <summary>
    /// Tests that Dispose logs debug message for fast operations.
    /// </summary>
    [Fact]
    public void Dispose_ForFastOperation_LogsDebug()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PerformanceMonitor>>();
        var monitor = new PerformanceMonitor("FastOperation", logger, warningThresholdMs: 1000);
        Thread.Sleep(10);

        // Act
        monitor.Dispose();

        // Assert
        logger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()!);
    }

    /// <summary>
    /// Tests that Dispose logs warning message for slow operations exceeding threshold.
    /// </summary>
    [Fact]
    public void Dispose_ForSlowOperation_LogsWarning()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PerformanceMonitor>>();
        var monitor = new PerformanceMonitor("SlowOperation", logger, warningThresholdMs: 10);
        Thread.Sleep(50); // Exceed the threshold

        // Act
        monitor.Dispose();

        // Assert
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>()!);
    }

    /// <summary>
    /// Tests that Dispose can be called multiple times safely.
    /// </summary>
    [Fact]
    public void Dispose_MultipleTimes_Safe()
    {
        // Arrange
        var monitor = new PerformanceMonitor("TestOperation");

        // Act
        monitor.Dispose();
        monitor.Dispose(); // Should not throw

        // Assert - no exception thrown
        Assert.True(true);
    }

    /// <summary>
    /// Tests that GetCheckpoints returns a copy of the checkpoints dictionary.
    /// </summary>
    [Fact]
    public void GetCheckpoints_ReturnsCopy_NotOriginal()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        monitor.RecordCheckpoint("Test");

        // Act
        var checkpoints1 = monitor.GetCheckpoints();
        var checkpoints2 = monitor.GetCheckpoints();

        // Assert - they should be different instances
        checkpoints1.Should().NotBeSameAs(checkpoints2);
        checkpoints1.Should().BeEquivalentTo(checkpoints2);
    }

    /// <summary>
    /// Tests that GetSummary includes checkpoint information when checkpoints exist.
    /// </summary>
    [Fact]
    public void GetSummary_WithCheckpoints_IncludesCheckpointInfo()
    {
        // Arrange
        using var monitor = new PerformanceMonitor("TestOperation", _logger);
        monitor.RecordCheckpoint("Checkpoint1");
        Thread.Sleep(10);
        monitor.RecordCheckpoint("Checkpoint2");

        // Act
        var summary = monitor.GetSummary();

        // Assert
        summary.Should().Contain("Checkpoint1");
        summary.Should().Contain("Checkpoint2");
    }
}
