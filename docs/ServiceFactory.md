# ServiceFactory

Factory class providing static methods to create and convert systemd service-related objects. Designed to standardize the construction of service monitoring artifacts and simplify integration with systemd's D-Bus interfaces.

## API

### `ServiceInfo CreateServiceInfo(string name, string description, string execStart, string? workingDirectory = null, Dictionary<string, string>? environment = null)`

Creates a `ServiceInfo` instance representing a systemd service definition. The `name` must be a valid systemd unit name; otherwise, the behavior is undefined. The `description` and `execStart` parameters are required and must not be empty. The optional `workingDirectory` and `environment` parameters allow specifying runtime context for the service process. Returns a populated `ServiceInfo` instance. Throws `ArgumentException` if `name`, `description`, or `execStart` is null or whitespace.

### `ServiceMetric CreateServiceMetric(string serviceName, double value, string unit, DateTime timestamp)`

Creates a `ServiceMetric` instance for monitoring service performance or health. The `serviceName` must correspond to a known service; otherwise, the metric may not be actionable. The `value` represents the measured quantity, and `unit` defines the measurement unit (e.g., "ms", "req/s"). The `timestamp` records when the measurement occurred. Returns a new `ServiceMetric` object. Throws `ArgumentException` if `serviceName` is null or empty, or if `unit` is null or whitespace.

### `ServiceLog CreateServiceLog(string serviceName, string message, LogLevel level, DateTime timestamp)`

Creates a `ServiceLog` entry capturing a systemd service event or message. The `serviceName` identifies the source service; `message` contains the log content; `level` indicates severity; `timestamp` marks when the log was generated. Returns a new `ServiceLog` instance. Throws `ArgumentException` if `serviceName` or `message` is null or empty, or if `level` is outside valid enum values.

### `ServiceStatus CreateServiceStatus(string serviceName, ServiceState state, DateTime timestamp, string? statusText = null)`

Constructs a `ServiceStatus` object reflecting the current operational state of a systemd service. The `serviceName` must be valid; `state` indicates the service's condition (e.g., active, failed); `timestamp` records when the status was observed. Optional `statusText` provides additional context. Returns a populated `ServiceStatus`. Throws `ArgumentException` if `serviceName` is null or empty, or if `state` is not a defined enum value.

### `RestartPolicyConfig CreateRestartPolicy(string policy, int? maxRetries = null)`

Generates a `RestartPolicyConfig` defining how a service should be restarted after failure or stop. The `policy` must be a valid systemd restart policy (e.g., "no", "on-failure", "always"). If `maxRetries` is provided, it limits the number of restart attempts; otherwise, the policy applies indefinitely. Returns a configured `RestartPolicyConfig`. Throws `ArgumentException` if `policy` is null, empty, or not a recognized value.

### `Dictionary<string, object> ServiceInfoToDictionary(ServiceInfo info)`

Converts a `ServiceInfo` object into a dictionary representation suitable for serialization or configuration binding. The returned dictionary includes keys for each property of `ServiceInfo` (e.g., "Name", "Description", "ExecStart"). Returns a non-null dictionary with at least one entry. Throws `ArgumentNullException` if `info` is null.

### `List<ServiceInfo> CreateServicesFromNames(IEnumerable<string> serviceNames)`

Creates a list of `ServiceInfo` objects from a collection of service names. Each name is used to derive a minimal `ServiceInfo` with placeholder values for required fields. Useful for bulk initialization or discovery. Returns a list of `ServiceInfo` instances; the list may be empty if `serviceNames` is empty or contains invalid names. Does not throw exceptions for invalid names—invalid entries are silently omitted from the result.

## Usage
