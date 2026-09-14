# PathResolverValidation

Provides validation methods for `PathResolver`. Validates path strings, service names, and related properties for correctness and security.

## Methods

### Validate()
Validates the `PathResolver` and returns a list of human-readable problems.

**Returns:** A read-only list of validation problems; empty if valid.

**Validation checks:**
- System unit paths collection is not null and not empty
- Each system unit path is not null, whitespace, or containing path traversal (`..`)
- Default system unit directory is not null, whitespace, or containing path traversal
- Default user unit directory is not null, whitespace, or containing path traversal

### IsValid()
Determines whether the `PathResolver` is valid.

**Returns:** `true` if valid; otherwise, `false`.

### EnsureValid()
Ensures that the `PathResolver` is valid.

**Throws:** `ArgumentException` if `PathResolver` is invalid, containing a list of problems.

**Exception message format:**
```
PathResolver is invalid. Problems:
- [problem 1]
- [problem 2]
```