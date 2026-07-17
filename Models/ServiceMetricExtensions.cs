#nullable enable

using System.Globalization;

namespace SystemdServiceMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="ServiceMetric"/> to enable common operations
/// on service metrics such as filtering, aggregation, and conversion.
/// </summary>
public static class ServiceMetricExtensions
{
    /// <summary>
    /// Filters metrics by the specified metric type.
    /// </summary>
    /// <param name="metrics">The collection of metrics to filter.</param>
    /// <param name="metricType">The metric type to filter by.</param>
    /// <returns>An enumerable containing only metrics matching the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IEnumerable<ServiceMetric> WhereMetricType(this IEnumerable<ServiceMetric> metrics, MetricType metricType)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.Where(m => m.MetricType == metricType);
    }

    /// <summary>
    /// Filters metrics by service name from the Tags dictionary.
    /// </summary>
    /// <param name="metrics">The collection of metrics to filter.</param>
    /// <param name="serviceName">The service name to filter by.</param>
    /// <returns>An enumerable containing only metrics for the specified service.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="serviceName"/> is null or empty.</exception>
    public static IEnumerable<ServiceMetric> WhereServiceName(this IEnumerable<ServiceMetric> metrics, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);

        return metrics.Where(m => string.Equals(m.Tags.GetValueOrDefault("ServiceName"), serviceName, StringComparison.Ordinal));
    }

    /// <summary>
    /// Groups metrics by their metric type and calculates basic statistics for each group.
    /// </summary>
    /// <param name="metrics">The collection of metrics to group and aggregate.</param>
    /// <returns>A dictionary mapping metric types to their aggregated statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IReadOnlyDictionary<MetricType, MetricStatistics> GroupByMetricType(this IEnumerable<ServiceMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var grouped = metrics
            .GroupBy(m => m.MetricType)
            .Select(g => new {
                MetricType = g.Key,
                Count = g.Count(),
                AvgValue = g.Average(m => (double)m.Value),
                MinValue = g.Min(m => m.MinValue is { } minValue ? (double)minValue : (double)m.Value),
                MaxValue = g.Max(m => m.MaxValue is { } maxValue ? (double)maxValue : (double)m.Value),
                TotalValue = g.Sum(m => (double)m.Value)
            })
            .ToDictionary(
                static x => x.MetricType,
                static x => new MetricStatistics(
                    x.Count,
                    (decimal)x.AvgValue,
                    (decimal)x.MinValue,
                    (decimal)x.MaxValue,
                    (decimal)x.TotalValue
                )
            );

        return grouped;
    }

    /// <summary>
    /// Converts a collection of metrics to a CSV-formatted string.
    /// </summary>
    /// <param name="metrics">The collection of metrics to convert.</param>
    /// <param name="includeHeader">Whether to include a header row.</param>
    /// <returns>A CSV-formatted string containing all metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string ToCsv(this IEnumerable<ServiceMetric> metrics, bool includeHeader = true)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var csvLines = new List<string>();

        if (includeHeader)
        {
            csvLines.Add("Timestamp,ServiceName,UnitName,MetricType,Value,Unit,ProcessId,SampleCount,CpuPercentage,MemoryUsageMb,NetworkBytesIn,NetworkBytesOut,DurationSeconds");
        }

        foreach (var metric in metrics.OrderBy(m => m.Timestamp))
        {
            csvLines.Add(string.Join(",",
                metric.Timestamp.ToString("o", CultureInfo.InvariantCulture),
                EscapeCsvField(metric.Tags.GetValueOrDefault("ServiceName")),
                EscapeCsvField(metric.UnitName),
                EscapeCsvField(metric.MetricType.ToString()),
                metric.Value.ToString(CultureInfo.InvariantCulture),
                EscapeCsvField(metric.Unit),
                metric.ProcessId.ToString(CultureInfo.InvariantCulture),
                metric.SampleCount.ToString(CultureInfo.InvariantCulture),
                metric.CpuPercentage.ToString(CultureInfo.InvariantCulture),
                metric.MemoryUsageMb.ToString(CultureInfo.InvariantCulture),
                metric.NetworkBytesIn.ToString(CultureInfo.InvariantCulture),
                metric.NetworkBytesOut.ToString(CultureInfo.InvariantCulture),
                metric.DurationSeconds.ToString(CultureInfo.InvariantCulture)
            ));
        }

        return string.Join(Environment.NewLine, csvLines);
    }

    /// <summary>
    /// Calculates the total resource usage across all metrics in the collection.
    /// </summary>
    /// <param name="metrics">The collection of metrics to aggregate.</param>
    /// <returns>A <see cref="ResourceUsageSummary"/> containing aggregated resource metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static ResourceUsageSummary CalculateResourceUsage(this IEnumerable<ServiceMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var resourceMetrics = metrics.Where(m => m.MetricType == MetricType.CpuUsage || m.MetricType == MetricType.MemoryUsage);

        return new ResourceUsageSummary(
            CpuPercentage: resourceMetrics.Sum(m => m.CpuPercentage),
            MemoryUsageMb: resourceMetrics.Sum(m => m.MemoryUsageMb),
            NetworkBytesIn: metrics.Sum(m => m.NetworkBytesIn),
            NetworkBytesOut: metrics.Sum(m => m.NetworkBytesOut),
            DiskReadBytesPerSec: metrics.Sum(m => m.DiskReadBytesPerSec),
            DiskWriteBytesPerSec: metrics.Sum(m => m.DiskWriteBytesPerSec),
            MetricCount: metrics.Count(),
            Timestamp: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Filters metrics by timestamp range.
    /// </summary>
    /// <param name="metrics">The collection of metrics to filter.</param>
    /// <param name="startTime">The start of the time range (inclusive).</param>
    /// <param name="endTime">The end of the time range (inclusive).</param>
    /// <returns>An enumerable containing only metrics within the specified time range.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IEnumerable<ServiceMetric> WhereTimestampBetween(this IEnumerable<ServiceMetric> metrics, DateTime startTime, DateTime endTime)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime);
    }

    /// <summary>
    /// Gets the most recent metric for each unique service.
    /// </summary>
    /// <param name="metrics">The collection of metrics to process.</param>
    /// <returns>A dictionary mapping service names to their most recent metrics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static IReadOnlyDictionary<string, ServiceMetric> GetLatestPerService(this IEnumerable<ServiceMetric> metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics
            .Where(m => m.Tags.TryGetValue("ServiceName", out _))
            .GroupBy(m => m.Tags["ServiceName"])
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .ToDictionary(m => m.Tags["ServiceName"], m => m);
    }

    /// <summary>
    /// Helper method to escape CSV field values.
    /// </summary>
    /// <param name="field">The field value to escape.</param>
    /// <returns>The escaped CSV field.</returns>
    private static string EscapeCsvField(string? field)
    {
        return field switch
        {
            null => "",
            _ when field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r')
                => '"' + field.Replace("\"", "\"\"") + '"',
            _ => field
        };
    }
}

/// <summary>
/// Represents aggregated statistics for a group of metrics.
/// </summary>
/// <param name="Count">Number of metrics in the group.</param>
/// <param name="Average">Average value of metrics in the group.</param>
/// <param name="Minimum">Minimum value in the group.</param>
/// <param name="Maximum">Maximum value in the group.</param>
/// <param name="Total">Sum of all values in the group.</param>
public readonly record struct MetricStatistics(
    int Count,
    decimal Average,
    decimal Minimum,
    decimal Maximum,
    decimal Total);

/// <summary>
/// Represents a summary of resource usage across multiple metrics.
/// </summary>
/// <param name="CpuPercentage">Total CPU percentage usage.</param>
/// <param name="MemoryUsageMb">Total memory usage in MB.</param>
/// <param name="NetworkBytesIn">Total network bytes received.</param>
/// <param name="NetworkBytesOut">Total network bytes sent.</param>
/// <param name="DiskReadBytesPerSec">Total disk read bytes per second.</param>
/// <param name="DiskWriteBytesPerSec">Total disk write bytes per second.</param>
/// <param name="MetricCount">Number of metrics aggregated.</param>
/// <param name="Timestamp">Timestamp of the summary.</param>
public readonly record struct ResourceUsageSummary(
    double CpuPercentage,
    double MemoryUsageMb,
    long NetworkBytesIn,
    long NetworkBytesOut,
    long DiskReadBytesPerSec,
    long DiskWriteBytesPerSec,
    int MetricCount,
    DateTime Timestamp);