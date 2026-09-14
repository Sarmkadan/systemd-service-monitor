# LogStreamFilterValidation
Provides validation helpers for `LogStreamFilter` instances.

## Methods

### Validate()
Validates the specified `LogStreamFilter` instance and returns a list of human-readable problems.

**Returns:** A read-only list of validation problems; empty if valid.

**Validation checks:**
- `ServiceName` must be at most 255 characters if not null or empty.
- `SearchTerm` must be at most 1024 characters if not null or empty.
- `MinLevel` must be a defined `LogLevel` enum value if it has a value.
- `BufferSize` must be between 0 and 500 (inclusive).
- `PollingIntervalMs` must be between 500 and 30000 (inclusive).

### IsValid()
Determines whether the specified `LogStreamFilter` instance is valid.

**Returns:** `true` if valid; otherwise, `false`.

### EnsureValid()
Ensures that the specified `LogStreamFilter` instance is valid.

**Throws:** `ArgumentException` if the filter is invalid, containing a list of validation failures.

**Exception message format:**
```
LogStreamFilter is invalid. Validation failed:
- [error 1]
- [error 2]
```