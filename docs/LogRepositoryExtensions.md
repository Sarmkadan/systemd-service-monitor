# LogRepositoryExtensions

Extension methods for querying service logs from a log repository, providing common filtering and aggregation operations for service monitoring scenarios.

## API

### `GetByServiceNameAsync`

Queries all log entries associated with a specific service by its name.

- **Parameters**
  - `ILogRepository repository` – The repository instance to query.
  - `string serviceName` – The name of the service to filter logs by.
  - `CancellationToken cancellationToken` – Optional token to monitor for cancellation requests.

- **Returns**
  - `Task<IEnumerable<ServiceLog>>` – An asynchronous sequence of `ServiceLog` entries matching the service name, in ascending chronological order.

- **Exceptions**
  - `ArgumentNullException` – Thrown if `repository` or `serviceName` is `null`.
  - `OperationCanceledException` – Thrown if `cancellationToken` is canceled before completion.

---

### `GetLatestForServiceAsync`

Retrieves the most recent log entry for a specified service.

- **Parameters**
  - `ILogRepository repository` – The repository instance to query.
  - `string serviceName` – The name of the service to retrieve the latest log for.
  - `CancellationToken cancellationToken` – Optional token to monitor for cancellation requests.

- **Returns**
  - `Task<ServiceLog?>` – The most recent `ServiceLog` entry for the service, or `null` if no logs exist. The result is ordered by timestamp in descending order.

- **Exceptions**
  - `ArgumentNullException` – Thrown if `repository` or `serviceName` is `null`.
  - `OperationCanceledException` – Thrown if `cancellationToken` is canceled before completion.

---

### `GetLogLevelCountsAsync`

Aggregates log entries by their severity level, returning a dictionary of counts per level.

- **Parameters**
  - `ILogRepository repository` – The repository instance to query.
  - `string? serviceName` – Optional service name to filter logs. If `null`, counts are aggregated across all services.
  - `CancellationToken cancellationToken` – Optional token to monitor for cancellation requests.

- **Returns**
  - `Task<Dictionary<SyslogLevel, int>>` – A dictionary mapping each `SyslogLevel` to the number of logs at that level. Only levels with non-zero counts are included.

- **Exceptions**
  - `ArgumentNullException` – Thrown if `repository` is `null`.
  - `OperationCanceledException` – Thrown if `cancellationToken` is canceled before completion.

---

### `GetByLevelsAsync`

Filters log entries by one or more specified severity levels.

- **Parameters**
  - `ILogRepository repository` – The repository instance to query.
  - `IEnumerable<SyslogLevel> levels` – The severity levels to include in the result.
  - `string? serviceName` – Optional service name to filter logs. If `null`, logs from all services are considered.
  - `CancellationToken cancellationToken` – Optional token to monitor for cancellation requests.

- **Returns**
  - `Task<IEnumerable<ServiceLog>>` – An asynchronous sequence of `ServiceLog` entries matching any of the specified levels, ordered chronologically.

- **Exceptions**
  - `ArgumentNullException` – Thrown if `repository`, `levels`, or any element in `levels` is `null`.
  - `OperationCanceledException` – Thrown if `cancellationToken` is canceled before completion.

## Usage
