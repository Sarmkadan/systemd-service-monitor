# ValidationHelper

Utility class providing static methods for validating common service-monitor input values such as service names, network addresses, ports, URLs, time ranges, pagination, and string lengths. It also offers an instance-based validator for more complex validation workflows and input sanitization utilities.

## API

### `public static ValidationResult ValidateServiceName(string serviceName)`

Validates that a service name adheres to systemd unit naming conventions. The name must be non-empty, contain only valid characters (alphanumeric, colon, underscore, hyphen, period, and forward slash), and not exceed 256 characters in length.

- **Parameters**
  - `serviceName`: The service name string to validate.
- **Return value**
  - `ValidationResult`: A result indicating whether the name is valid and, if not, containing an error message.
- **Exceptions**
  - Throws `ArgumentNullException` if `serviceName` is `null`.

---

### `public static ValidationResult ValidateIpAddress(string ipAddress)`

Validates that the provided string is a syntactically valid IPv4 or IPv6 address.

- **Parameters**
  - `ipAddress`: The IP address string to validate.
- **Return value**
  - `ValidationResult`: A result indicating whether the address is valid and, if not, containing an error message.
- **Exceptions**
  - Throws `ArgumentNullException` if `ipAddress` is `null`.

---

### `public static ValidationResult ValidatePort(int port)`

Validates that the port number is within the valid TCP/UDP range (1–65535).

- **Parameters**
  - `port`: The port number to validate.
- **Return value**
  - `ValidationResult`: A result indicating whether the port is valid and, if not, containing an error message.
- **Exceptions**
  - None.

---

### `public static ValidationResult ValidateUrl(string url)`

Validates that the provided string is a well-formed absolute HTTP or HTTPS URL with a valid host and optional path.

- **Parameters**
  - `url`: The URL string to validate.
- **Return value**
  - `ValidationResult`: A result indicating whether the URL is valid and, if not, containing an error message.
- **Exceptions**
  - Throws `ArgumentNullException` if `url` is `null`.

---

### `public static ValidationResult ValidateTimeRange(DateTimeOffset start, DateTimeOffset end)`

Validates that the time range is logically consistent: `start` must be less than or equal to `end`, and both values must be within a 30-day window.

- **Parameters**
  - `start`: The start of the time range.
  - `end`: The end of the time range.
- **Return value**
  - `ValidationResult`: A result indicating whether the range is valid and, if not, containing an error message.
- **Exceptions**
  - None.

---
### `public static ValidationResult ValidatePagination(int page, int pageSize)`

Validates that pagination parameters are within acceptable bounds: `page` must be ≥1 and `pageSize` must be ≥1 and ≤100.

- **Parameters**
  - `page`: The page number.
  - `pageSize`: The number of items per page.
- **Return value**
  - `ValidationResult`: A result indicating whether the pagination parameters are valid and, if not, containing an error message.
- **Exceptions**
  - None.

---
### `public static ValidationResult ValidateStringLength(string input, int maxLength, string? fieldName = null)`

Validates that the length of `input` does not exceed `maxLength`. Optionally includes the field name in the error message for clarity.

- **Parameters**
  - `input`: The string to validate.
  - `maxLength`: The maximum allowed length.
  - `fieldName`: Optional display name for the field used in error messages.
- **Return value**
  - `ValidationResult`: A result indicating whether the string length is valid and, if not, containing an error message.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.
  - Throws `ArgumentOutOfRangeException` if `maxLength` is negative.

---
### `public static string SanitizeInput(string input)`

Removes or escapes potentially harmful characters (e.g., control characters, HTML tags) from the input string to prevent injection attacks. Returns a sanitized version suitable for logging or display.

- **Parameters**
  - `input`: The raw input string to sanitize.
- **Return value**
  - `string`: The sanitized string.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.

---
### `public bool IsValid`

Gets a value indicating whether the current validation instance represents a valid state.

- **Return value**
  - `bool`: `true` if the validation passed; otherwise, `false`.
- **Exceptions**
  - None.

---
### `public string? ErrorMessage`

Gets the error message associated with the current validation instance, or `null` if the validation passed.

- **Return value**
  - `string?`: The error message, or `null`.
- **Exceptions**
  - None.

---
### `public static ValidationResult Valid`

Returns a `ValidationResult` indicating a successful validation with no error message.

- **Return value**
  - `ValidationResult`: A valid result.
- **Exceptions**
  - None.

---
### `public static ValidationResult Invalid(string errorMessage)`

Constructs a `ValidationResult` indicating a failed validation with the specified error message.

- **Parameters**
  - `errorMessage`: The error message to associate with the failure.
- **Return value**
  - `ValidationResult`: An invalid result.
- **Exceptions**
  - Throws `ArgumentNullException` if `errorMessage` is `null`.

## Usage
