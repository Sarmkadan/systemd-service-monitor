# ResultExtensions

The `ResultExtensions` class provides a set of static helper methods for fluent manipulation and transformation of `ApiResponse<T>` and `PaginatedResponse<T>` objects. Designed to streamline handling of service responses, these extensions facilitate common operations such as wrapping results, mapping data types, chaining side effects, and safely extracting data payloads within asynchronous or synchronous workflows.

## API

### `ToSuccess<T>`
Wraps an object into a successful `ApiResponse<T>`.
- **Parameters:** `T data` - The data to wrap.
- **Returns:** An `ApiResponse<T>` initialized with the provided data and a success status.

### `ToError<T>`
Creates an `ApiResponse<T>` representing a failure state.
- **Parameters:** `string error` - The error message associated with the failure.
- **Returns:** An `ApiResponse<T>` containing the error message and a failure status.

### `ToPaginatedResponse<T>`
Wraps an enumerable collection into a `PaginatedResponse<T>`.
- **Parameters:** `IEnumerable<T> data`, `int page`, `int pageSize`, `int totalCount`.
- **Returns:** A `PaginatedResponse<T>` containing the data and pagination metadata.

### `Map<TSource, TResult>`
Transforms the data within an `ApiResponse<TSource>` into an `ApiResponse<TResult>`.
- **Parameters:** `ApiResponse<TSource> source`, `Func<TSource, TResult> mapper`.
- **Returns:** An `ApiResponse<TResult>` containing the mapped data if successful, or the original error if failed.

### `OnSuccess<T>`
Executes an action if the `ApiResponse<T>` represents a success state, returning the original response.
- **Parameters:** `ApiResponse<T> response`, `Action<T> action`.
- **Returns:** The original `ApiResponse<T>`.

### `OnFailure<T>`
Executes an action if the `ApiResponse<T>` represents a failure state, returning the original response.
- **Parameters:** `ApiResponse<T> response`, `Action<string> action`.
- **Returns:** The original `ApiResponse<T>`.

### `HasData<T>`
Checks if an `ApiResponse<T>` contains a non-null data payload and represents a successful state.
- **Parameters:** `ApiResponse<T> response`.
- **Returns:** `bool` - `true` if successful and data is present; otherwise `false`.

### `GetDataOrThrow<T>`
Extracts the data from an `ApiResponse<T>` or throws an exception if the response indicates failure or data is null.
- **Parameters:** `ApiResponse<T> response`.
- **Returns:** `T`.
- **Throws:** `InvalidOperationException` if the response indicates an error or contains no data.

### `GetDataOrDefault<T>`
Attempts to extract the data from an `ApiResponse<T>`, returning a default value if the response indicates failure or is null.
- **Parameters:** `ApiResponse<T> response`, `T? defaultValue` (optional).
- **Returns:** `T?`.

## Usage

```csharp
// Example 1: Creating and mapping a response
var rawResponse = service.FetchUser(userId).ToSuccess();
var userDto = rawResponse.Map(user => new UserDto(user.Name));

// Example 2: Handling responses fluently
var response = service.PerformAction();
response
    .OnSuccess(data => Console.WriteLine($"Success: {data}"))
    .OnFailure(error => Console.WriteLine($"Error: {error}"));

if (response.HasData())
{
    var data = response.GetDataOrThrow();
}
```

## Notes

- **Thread-Safety:** These extension methods are purely functional and do not modify the state of the input `ApiResponse<T>` or `PaginatedResponse<T>` objects, making them inherently thread-safe when operated on immutable response types.
- **Null Handling:** While `HasData` and `GetDataOrDefault` provide safe checks, `GetDataOrThrow` strictly enforces data presence and successful status. Ensure proper defensive checking when dealing with API responses that may not always contain a data payload.
- **Mapping:** The `Map` method preserves the success/failure state of the original response. If the source response is in an error state, the mapper function will not be executed, and the error state is propagated to the result.
