# ApiResponse

A generic container for API responses that standardizes success/failure states, payloads, and diagnostic information. Used to wrap service-monitor API results so callers receive a consistent shape regardless of operation outcome.

## API

### `public T? Data`
The payload returned by a successful operation. `null` when `Success` is `false`. The type `T` is constrained by the caller’s use of the generic parameter.

### `public bool Success`
Indicates whether the operation completed successfully. `true` when the operation succeeded and `Data` contains the expected result; `false` when an error occurred and `ErrorDetails` should be inspected.

### `public string Message`
A human-readable summary of the result. On success it typically echoes the requested action; on failure it describes the error at a high level (e.g., “Service not found”).

### `public string? ErrorDetails`
Detailed diagnostic text populated only when `Success` is `false`. May include exception messages, stack traces, or other debugging information intended for logs or support staff. `null` otherwise.

### `public long Timestamp`
Milliseconds since Unix epoch (UTC) marking when the response was generated. Intended for auditing, caching, and ordering of events.

### `public string TraceId`
A correlation identifier (typically a GUID) shared across service boundaries. Enables tracing a single logical request as it propagates through the systemd-service-monitor and related components.

## Usage
