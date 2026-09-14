# ApiResponseValidation

Provides validation helpers for `ApiResponse<T>` instances.

## Methods

### Validate<T>(ApiResponse<T> value)

Validates an `ApiResponse<T>` instance and returns a list of human-readable validation problems.

#### Type Parameters

- `T`: The type of data in the response.

#### Parameters

- `value`: The response to validate.

#### Returns

An enumerable of validation problem descriptions, or an empty list if the response is valid.

#### Exceptions

- `ArgumentNullException`: Thrown when `value` is null.

#### Remarks

The validation checks the following:

1. **Message**: Must not be null, empty, or whitespace.
2. **ErrorDetails**:
   - For successful responses (`Success == true`), `ErrorDetails` must be null or empty.
   - For failed responses (`Success == false`), `ErrorDetails` must not be null or empty.
3. **Timestamp**: Must be a positive Unix timestamp (greater than 0).
4. **TraceId**: Must not be null, empty, or whitespace, and must be at least 32 characters long.
5. **Data**:
   - For failed responses (`Success == false`), `Data` must be null.

### IsValid<T>(ApiResponse<T> value)

Determines whether an `ApiResponse<T>` instance is valid.

#### Type Parameters

- `T`: The type of data in the response.

#### Parameters

- `value`: The response to check.

#### Returns

True if the response is valid; otherwise, false.

### EnsureValid<T>(ApiResponse<T> value)

Ensures that an `ApiResponse<T>` instance is valid, throwing an `ArgumentException` with a detailed error message if it is not.

#### Type Parameters

- `T`: The type of data in the response.

#### Parameters

- `value`: The response to validate.

#### Exceptions

- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentException`: Thrown when the response is invalid, containing a list of validation problems.

## Usage Example

```csharp
var response = new ApiResponse<MyData>
{
    Success = true,
    Message = "Operation completed",
    TraceId = Guid.NewGuid().ToString("N"), // 32-character hex string
    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    Data = new MyData { /* ... */ }
};

var problems = response.Validate();
if (problems.Any())
{
    // Handle validation problems
}

if (!response.IsValid())
{
    // Handle invalid response
}

response.EnsureValid(); // Throws ArgumentException if invalid
```