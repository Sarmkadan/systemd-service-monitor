# MetricRepository
The `MetricRepository` class is designed to manage and retrieve service metrics in the `systemd-service-monitor` project. It provides a range of methods for creating, reading, updating, and deleting service metrics, allowing for efficient data storage and retrieval.

## API
The `MetricRepository` class exposes the following public members:
* `GetByIdAsync`: Retrieves a `ServiceMetric` by its ID. Returns a `ServiceMetric` object if found, or `null` if not found. Throws if the database operation fails.
* `GetByServiceIdAsync`: Retrieves a collection of `ServiceMetric` objects associated with a given service ID. Returns an empty collection if no metrics are found. Throws if the database operation fails.
* `GetByMetricTypeAsync`: Retrieves a collection of `ServiceMetric` objects of a given metric type. Returns an empty collection if no metrics are found. Throws if the database operation fails.
* `GetTimeSeriesAsync`: Retrieves a collection of `ServiceMetric` objects representing a time series of metrics. Returns an empty collection if no metrics are found. Throws if the database operation fails.
* `GetRecentAsync`: Retrieves a collection of recent `ServiceMetric` objects. Returns an empty collection if no metrics are found. Throws if the database operation fails.
* `CreateAsync`: Creates a new `ServiceMetric` object and returns it. Throws if the database operation fails.
* `CreateBatchAsync`: Creates multiple new `ServiceMetric` objects in a single operation and returns the number of objects created. Throws if the database operation fails.
* `DeleteAsync`: Deletes a `ServiceMetric` object by its ID and returns a boolean indicating success. Throws if the database operation fails.
* `DeleteOlderThanAsync`: Deletes all `ServiceMetric` objects older than a given date and returns the number of objects deleted. Throws if the database operation fails.
* `GetLatestAsync`: Retrieves the most recent `ServiceMetric` object. Returns `null` if no metrics are found. Throws if the database operation fails.
* `GetAverageAsync`: Calculates the average value of a metric and returns it as a decimal. Throws if the database operation fails.

## Usage
Here are two examples of using the `MetricRepository` class:
```csharp
// Example 1: Retrieving metrics by service ID
var metricRepository = new MetricRepository();
var serviceId = 123;
var metrics = await metricRepository.GetByServiceIdAsync(serviceId);
foreach (var metric in metrics)
{
    Console.WriteLine($"Metric ID: {metric.Id}, Value: {metric.Value}");
}

// Example 2: Creating a new metric
var newMetric = new ServiceMetric { ServiceId = 123, MetricType = "CPUUsage", Value = 50.0m };
var createdMetric = await metricRepository.CreateAsync(newMetric);
Console.WriteLine($"Created Metric ID: {createdMetric.Id}");
```

## Notes
When using the `MetricRepository` class, note the following:
* All methods are asynchronous and may throw exceptions if the underlying database operations fail.
* The `GetByIdAsync`, `GetByServiceIdAsync`, `GetByMetricTypeAsync`, `GetTimeSeriesAsync`, and `GetRecentAsync` methods return empty collections if no metrics are found, rather than throwing exceptions.
* The `CreateAsync` and `CreateBatchAsync` methods will throw exceptions if the database operation fails, such as if the metric already exists or if there is a constraint violation.
* The `DeleteAsync` and `DeleteOlderThanAsync` methods will throw exceptions if the database operation fails, such as if the metric does not exist or if there is a constraint violation.
* The `MetricRepository` class is designed to be thread-safe, but it is still important to ensure that the underlying database connection is properly synchronized to avoid concurrency issues.
