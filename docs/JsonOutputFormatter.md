# JsonOutputFormatter

The `JsonOutputFormatter` class is an implementation of the `IOutputFormatter` interface that provides JSON serialization capabilities for the SystemdServiceMonitor application.

## Role

This formatter is responsible for converting objects and collections of objects into JSON-formatted byte arrays, which can then be used in HTTP responses or file outputs. It implements the `IOutputFormatter` interface, allowing it to be plugged into the application's formatting pipeline.

## Implementation Details

### Constructor

The formatter accepts an `IOptions<JsonSerializerOptions>` parameter, which allows for configuration of JSON serialization settings via the application's dependency injection container. If no options are provided, it defaults to:
- Indented JSON output (`WriteIndented = true`)
- Ignoring null values during serialization (`DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`)
- Using camelCase property naming (`PropertyNamingPolicy = JsonNamingPolicy.CamelCase`)

### Properties

- `MimeType`: Returns `"application/json"` indicating the MIME type for JSON output.
- `FileExtension`: Returns `"json"` indicating the typical file extension for JSON files.

### Methods

#### `Format<T>(T data, FormattingOptions? options = null)`

Serializes a single object to JSON format.

Parameters:
- `data`: The object to serialize (must be a class type).
- `options`: Optional formatting options that can override the default serializer settings.

Returns:
- A byte array containing the UTF-8 encoded JSON string.

#### `FormatCollection<T>(IEnumerable<T> data, FormattingOptions? options = null)`

Serializes a collection of objects to JSON format.

Parameters:
- `data`: The collection of objects to serialize (each item must be a class type).
- `options`: Optional formatting options that can override the default serializer settings.

Returns:
- A byte array containing the UTF-8 encoded JSON string representing the collection.

#### `GetJsonSerializerOptions(FormattingOptions? options)`

Private helper method that creates a `JsonSerializerOptions` instance based on the provided formatting options or falls back to the default options configured in the constructor.

### FormattingOptions Integration

The formatter respects the `FormattingOptions` class to allow dynamic adjustment of:
- `PrettyPrint`: Controls whether JSON is indented (when true) or compact (when false).
- `IncludeNullValues`: Determines whether null-valued properties are included in the output.
- `Encoding`: Specifies the character encoding for the output bytes (defaults to UTF8).
- `CustomHeaders`: Not used for JSON formatting (reserved for other formats like CSV).

## Usage

The formatter is typically registered in the application's dependency injection container and selected based on the requested response format (e.g., via Accept header or file extension). It provides a consistent way to produce JSON output across different parts of the application.

## Thread Safety

This class is thread-safe for concurrent use by multiple threads as long as the provided `IOptions<JsonSerializerOptions>` is not modified after initialization. The formatter does not maintain any mutable state beyond the read-only serializer options.