# ResultExtensionsJsonExtensions

Provides extension methods for converting between .NET objects and JSON strings, primarily used for serializing and deserializing API responses and paginated results in the `systemd-service-monitor` project.

## API

### `ToJson<T>(this Result<T> result)`
Converts a `Result<T>` object into a JSON string representation.

- **Parameters**:
  - `result`: The `Result<T>` instance to serialize.
- **Return value**: A JSON string representing the `Result<T>` object.
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `ToJson<T>(this Result result)`
Converts a `Result` object into a JSON string representation.

- **Parameters**:
  - `result`: The `Result` instance to serialize.
- **Return value**: A JSON string representing the `Result` object.
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `FromJson<T>(this string json)`
Deserializes a JSON string into an `ApiResponse<T>` object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Return value**: An `ApiResponse<T>` object if deserialization succeeds; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `json` is `null`.

### `FromJsonPaginated<T>(this string json)`
Deserializes a JSON string into a `PaginatedResponse<T>` object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Return value**: A `PaginatedResponse<T>` object if deserialization succeeds; otherwise, `null`.
- **Throws**: `ArgumentNullException` if `json` is `null`.

### `TryFromJson<T>(this string json, out ApiResponse<T>? response)`
Attempts to deserialize a JSON string into an `ApiResponse<T>` object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `response`: Output parameter receiving the deserialized `ApiResponse<T>` if successful.
- **Return value**: `true` if deserialization succeeds; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `json` is `null`.

### `TryFromJsonPaginated<T>(this string json, out PaginatedResponse<T>? response)`
Attempts to deserialize a JSON string into a `PaginatedResponse<T>` object.

- **Parameters**:
  - `json`: The JSON string to deserialize.
  - `response`: Output parameter receiving the deserialized `PaginatedResponse<T>` if successful.
- **Return value**: `true` if deserialization succeeds; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `json` is `null`.

## Usage
