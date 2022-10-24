# ValidationHelperTests

`ValidationHelperTests` is a unit test class within the `systemd-service-monitor` project designed to verify the correctness of validation logic implemented in the `ValidationHelper` utility. These tests ensure that input validation for service names, ports, time ranges, URLs, and other parameters adheres to expected constraints, returning appropriate validation results for valid and invalid inputs.

## API

### `ValidateServiceName_ValidFormats_ReturnsValid`
Verifies that `ValidationHelper` correctly identifies valid service name formats as valid.
- **Purpose**: Ensures well-formed service names (e.g., alphanumeric with underscores/hyphens) pass validation.
- **Parameters**: None (test data is hardcoded or mocked internally).
- **Return Value**: None (assertions validate expected outcomes).
- **Throws**: None (test failures raise assertion exceptions).

### `ValidateServiceName_InvalidFormats_ReturnsInvalidWithMessage`
Tests that `ValidationHelper` rejects invalid service names and provides descriptive error messages.
- **Purpose**: Confirms invalid formats (e.g., empty strings, special characters) are flagged with specific feedback.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `ValidatePort_OutOfBounds_ReturnsInvalid`
Checks that port numbers outside the valid range (1–65535) are rejected.
- **Purpose**: Ensures edge cases (e.g., 0, 65536) are invalidated.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `ValidatePort_ValidPorts_ReturnsValid`
Validates that port numbers within the acceptable range (1–65535) are accepted.
- **Purpose**: Confirms standard and boundary ports (e.g., 1, 80, 443, 65535) pass validation.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `ValidateTimeRange_ExceedsOneYear_ReturnsInvalid`
Ensures time ranges exceeding one year are rejected.
- **Purpose**: Prevents excessively long monitoring durations that could impact system performance.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `ValidateTimeRange_StartAfterEnd_ReturnsInvalid`
Tests that time ranges where the start datetime follows the end datetime are invalidated.
- **Purpose**: Guarantees logical temporal ordering (start ≤ end).
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `ValidateUrl_FtpScheme_ReturnsInvalid`
Verifies that URLs with non-HTTP/HTTPS schemes (e.g., FTP) are rejected.
- **Purpose**: Restricts URL validation to web-compatible schemes.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

### `SanitizeInput_StringExceedingMaxLength_TruncatesToLimit`
Confirms that strings exceeding a predefined maximum length are truncated to the limit.
- **Purpose**: Ensures input sanitization prevents buffer overflows or storage issues.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: None.

## Usage

### Example 1: Testing Service Name Validation
