# OutputFormatter

A utility class providing static methods to format structured data (objects, metrics, service details) into common output formats such as JSON, CSV, and aligned tables. Designed for CLI tools and service monitoring applications where human-readable and machine-readable output are both required.

## API

### `FormatAsJson<T>(T data)`
Formats the provided object as a JSON string with indentation for readability.

- **Parameters**
  - `data` (`T`): The object to serialize to JSON.
- **Return value**
  - A `string` containing the indented JSON representation of `data`.
- **Exceptions**
  - Throws `System.Text.Json.JsonException` if serialization fails (e.g., due to circular references or unsupported types).

---

### `FormatAsCsv<T>(IEnumerable<T> data)`
Converts a sequence of objects into a CSV-formatted string. The first row contains headers derived from public property names.

- **Parameters**
  - `data` (`IEnumerable<T>`): The collection of objects to convert.
- **Return value**
  - A `string` containing the CSV output with headers and rows.
- **Exceptions**
  - Throws `System.ArgumentNullException` if `data` is `null`.
  - Throws `System.InvalidOperationException` if any object in the sequence lacks public readable properties.

---

### `FormatAsTable<T>(IEnumerable<T> data)`
Renders a collection of objects as an aligned, multi-line table with headers and borders.

- **Parameters**
  - `data` (`IEnumerable<T>`): The collection of objects to display.
- **Return value**
  - A `string` containing the formatted table.
- **Exceptions**
  - Throws `System.ArgumentNullException` if `data` is `null`.
  - Throws `System.InvalidOperationException` if any object in the sequence lacks public readable properties.

---

### `FormatMetricsAsTable(IEnumerable<ServiceMetric> metrics)`
Formats a sequence of service metrics into a table with columns for timestamp, service name, status, and value.

- **Parameters**
  - `metrics` (`IEnumerable<ServiceMetric>`): The metrics to display.
- **Return value**
  - A `string` containing the formatted metrics table.
- **Exceptions**
  - Throws `System.ArgumentNullException` if `metrics` is `null`.

---
### `FormatServiceDetails(ServiceDetails details)`
Renders detailed information about a single service as a multi-line, indented block.

- **Parameters**
  - `details` (`ServiceDetails`): The service details to format.
- **Return value**
  - A `string` containing the formatted service details.
- **Exceptions**
  - Throws `System.ArgumentNullException` if `details` is `null`.

---
### `CreateProgressBar(int width, double progress)`
Generates a progress bar string of the specified width and progress percentage.

- **Parameters**
  - `width` (`int`): The total character width of the progress bar (minimum 1).
  - `progress` (`double`): The progress value between 0.0 and 1.0.
- **Return value**
  - A `string` representing the progress bar (e.g., `[====    ] 50%`).
- **Exceptions**
  - Throws `System.ArgumentOutOfRangeException` if `width < 1` or if `progress` is outside [0.0, 1.0].

## Usage

```csharp
// Example 1: Formatting a list of services as JSON
var services = new[] { new { Name = "nginx", Status = "running" }, new { Name = "postgres", Status = "degraded" } };
string jsonOutput = OutputFormatter.FormatAsJson(services);
Console.WriteLine(jsonOutput);

// Example 2: Rendering service metrics as a table
var metrics = new[]
{
    new ServiceMetric { Timestamp = DateTime.UtcNow, ServiceName = "redis", Status = "ok", Value = 42 },
    new ServiceMetric { Timestamp = DateTime.UtcNow.AddMinutes(-1), ServiceName = "redis", Status = "degraded", Value = 21 }
};
string tableOutput = OutputFormatter.FormatMetricsAsTable(metrics);
Console.WriteLine(tableOutput);
```

## Notes

- All formatting methods are thread-safe and may be called concurrently from multiple threads.
- Methods that accept `IEnumerable<T>` will materialize the sequence immediately (e.g., via `.ToList()`) to avoid multiple enumerations.
- Progress bar rendering uses simple ASCII characters and does not support Unicode or ANSI color escapes.
- JSON serialization uses `System.Text.Json` with default options (camelCase naming policy and indentation).
