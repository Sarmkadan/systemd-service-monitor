# ServiceMetricExtensions

`ServiceMetricExtensions` is a static helper class that provides LINQ‑style filtering, grouping, and aggregation operations for collections of `ServiceMetric` objects. It is intended to simplify common queries performed against metric data, such as selecting metrics by type or service name, computing statistics per metric type, converting metrics to CSV, and summarizing resource usage.

## API

### `public static IEnumerable<ServiceMetric> WhereMetricType(this IEnumerable<ServiceMetric> metrics, MetricType metricType)`

Filters the supplied sequence to only those metrics whose `MetricType` equals the specified `metricType`.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
  - `metricType`: The metric type to filter on.
- **Returns**: An `IEnumerable<ServiceMetric>` containing only metrics that match the specified type.
- **Throws**: `ArgumentNullException` if `metrics` is `null`.

### `public static IEnumerable<ServiceMetric> WhereServiceName(this IEnumerable<ServiceMetric> metrics, string serviceName)`

Filters the supplied sequence to only those metrics whose `ServiceName` equals the specified `serviceName`.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
  - `serviceName`: The service name to filter on.
- **Returns**: An `IEnumerable<ServiceMetric>` containing only metrics that belong to the specified service.
- **Throws**: `ArgumentNullException` if `metrics` or `serviceName` is `null`.

### `public static IReadOnlyDictionary<MetricType, MetricStatistics> GroupByMetricType(this IEnumerable<ServiceMetric> metrics)`

Groups the supplied sequence by `MetricType` and returns a read‑only dictionary mapping each metric type to its aggregated statistics.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
- **Returns**: A `IReadOnlyDictionary<MetricType, MetricStatistics>` where each key is a metric type and each value contains aggregated statistics (`Count`, `Average`, `Min`, `Max`, `Sum`).
- **Throws**: `ArgumentNullException` if `metrics` is `null`.

### `public static string ToCsv(this IEnumerable<ServiceMetric> metrics)`

Converts the supplied sequence of metrics into a CSV string. The first line contains column headers; subsequent lines contain metric values.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
- **Returns**: A CSV representation of the metrics.
- **Throws**: `ArgumentNullException` if `metrics` is `null`.

### `public static ResourceUsageSummary CalculateResourceUsage(this IEnumerable<ServiceMetric> metrics)`

Calculates a summary of resource usage across the supplied metrics, aggregating CPU, memory, and disk usage statistics.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
- **Returns**: A `ResourceUsageSummary` containing aggregated CPU, memory, and disk usage statistics.
- **Throws**: `ArgumentNullException` if `metrics` is `null`.

### `public static IEnumerable<ServiceMetric> WhereTimestampBetween(this IEnumerable<ServiceMetric> metrics, DateTime start, DateTime end)`

Filters the supplied sequence to only those metrics whose `Timestamp` falls within the inclusive range `[start, end]`.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
  - `start`: The start of the time window.
  - `end`: The end of the time window.
- **Returns**: An `IEnumerable<ServiceMetric>` containing only metrics whose timestamps are within the specified range.
- **Throws**: `ArgumentNullException` if `metrics` is `null`; `ArgumentException` if `start > end`.

### `public static IReadOnlyDictionary<string, ServiceMetric> GetLatestPerService(this IEnumerable<ServiceMetric> metrics)`

Returns a dictionary mapping each service name to the most recent metric for that service.

- **Parameters**
  - `metrics`: The source sequence of `ServiceMetric` objects.
- **Returns**: A `IReadOnlyDictionary<string, ServiceMetric>` where each key is a service name and each value is the latest metric for that service.
- **Throws**: `ArgumentNullException` if `metrics` is `null`.

### `public readonly record struct MetricStatistics`

Represents aggregated statistics for a set of metrics.

- **Properties**
  - `int Count`
  - `double Average`
  - `double Min`
  - `double Max`
  - `double Sum`

### `public readonly record struct ResourceUsageSummary`

Represents a summary of resource usage across metrics.

- **Properties**
  - `double CpuUsagePercent`
  - `long MemoryUsageBytes`
  - `long DiskUsageBytes`

## Usage

