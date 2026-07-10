# JsonSerializerConfiguration

`JsonSerializerConfiguration` is a centralized utility for managing `System.Text.Json` serialization settings within the `systemd-service-monitor` project. It provides preconfigured `JsonSerializerOptions` instances for common output formats—default, compact, and verbose—along with generic serialization and deserialization helpers. The class also exposes custom converters for `DateTime` and `TimeSpan` types and supports extension of the default options through a dedicated method.

## API

### `GetDefaultOptions`

```csharp
public static JsonSerializerOptions GetDefaultOptions { get; }
```

Returns a static `JsonSerializerOptions` instance configured with the project’s standard defaults. This includes camelCase property naming, indented formatting, and the custom `DateTime` and `TimeSpan` converters. The returned instance is immutable in practice; callers should not modify it directly. Use `WithCustomConverters` if a mutable copy is required.

**Return value:** A read-only `JsonSerializerOptions` object.

**Throws:** Nothing. The property is statically initialized and does not throw.

---

### `GetCompactOptions`

```csharp
public static JsonSerializerOptions GetCompactOptions { get; }
```

Returns a `JsonSerializerOptions` instance optimized for minimal payload size. Indentation is disabled, and property naming follows the default camelCase convention. The same custom converters for `DateTime` and `TimeSpan` are included.

**Return value:** A `JsonSerializerOptions` object configured for compact output.

**Throws:** Nothing.

---

### `GetVerboseOptions`

```csharp
public static JsonSerializerOptions GetVerboseOptions { get; }
```

Returns a `JsonSerializerOptions` instance intended for human-readable output. Indentation is enabled, and property names are written in camelCase. Custom `DateTime` and `TimeSpan` converters are applied.

**Return value:** A `JsonSerializerOptions` object configured for verbose, indented output.

**Throws:** Nothing.

---

### `Serialize<T>` (overload 1)

```csharp
public static string Serialize<T>(T value)
```

Serializes the given value to a JSON string using the default options returned by `GetDefaultOptions`.

**Parameters:**
- `value` (`T`): The object to serialize.

**Return value:** A JSON string representation of `value`.

**Throws:** `ArgumentNullException` if `value` is `null` and `T` is a reference type. May throw `NotSupportedException` or `JsonException` if the object graph contains types that cannot be serialized with the default options.

---

### `Serialize<T>` (overload 2)

```csharp
public static string Serialize<T>(T value, JsonSerializerOptions options)
```

Serializes the given value to a JSON string using the explicitly provided `options`.

**Parameters:**
- `value` (`T`): The object to serialize.
- `options` (`JsonSerializerOptions`): The serializer options to use.

**Return value:** A JSON string representation of `value`.

**Throws:** `ArgumentNullException` if `options` is `null`. `ArgumentNullException` if `value` is `null` and `T` is a reference type. May throw `NotSupportedException` or `JsonException` for unsupported types.

---

### `Deserialize<T>` (overload 1)

```csharp
public static T? Deserialize<T>(string json)
```

Deserializes a JSON string to an instance of `T` using the default options from `GetDefaultOptions`.

**Parameters:**
- `json` (`string`): The JSON string to parse.

**Return value:** An instance of `T`, or `null` if the JSON represents a null value and `T` is a reference type or `Nullable<T>`.

**Throws:** `ArgumentNullException` if `json` is `null`. `JsonException` if the JSON is malformed or cannot be mapped to `T`.

---

### `Deserialize<T>` (overload 2)

```csharp
public static T? Deserialize<T>(string json, JsonSerializerOptions options)
```

Deserializes a JSON string to an instance of `T` using the explicitly provided `options`.

**Parameters:**
- `json` (`string`): The JSON string to parse.
- `options` (`JsonSerializerOptions`): The serializer options to use.

**Return value:** An instance of `T`, or `null` if the JSON represents a null value and `T` is a reference type or `Nullable<T>`.

**Throws:** `ArgumentNullException` if `json` or `options` is `null`. `JsonException` if the JSON is malformed or cannot be mapped to `T`.

---

### `Read` (DateTime converter)

```csharp
public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
```

Custom converter method that reads a `DateTime` value from a JSON reader. The implementation expects a specific string format used throughout the project (typically ISO 8601 with a trailing `Z` indicator). Overrides the base `JsonConverter<DateTime>.Read`.

**Parameters:**
- `reader` (`Utf8JsonReader`): The reader positioned at the JSON token to convert.
- `typeToConvert` (`Type`): The target type (always `DateTime`).
- `options` (`JsonSerializerOptions`): The options in use.

**Return value:** A `DateTime` parsed from the JSON token.

**Throws:** `JsonException` if the token is not a string or does not match the expected format. `FormatException` may propagate from internal parsing.

---

### `Write` (DateTime converter)

```csharp
public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
```

Writes a `DateTime` value as a JSON string in the project’s standard format. Overrides `JsonConverter<DateTime>.Write`.

**Parameters:**
- `writer` (`Utf8JsonWriter`): The writer to output the value to.
- `value` (`DateTime`): The date-time value to write.
- `options` (`JsonSerializerOptions`): The options in use.

**Throws:** Nothing. The write operation is safe for all valid `DateTime` values.

---

### `Read` (TimeSpan converter)

```csharp
public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
```

Custom converter method that reads a `TimeSpan` value from a JSON reader. The implementation expects a string representation compatible with `TimeSpan.Parse`. Overrides `JsonConverter<TimeSpan>.Read`.

**Parameters:**
- `reader` (`Utf8JsonReader`): The reader positioned at the JSON token to convert.
- `typeToConvert` (`Type`): The target type (always `TimeSpan`).
- `options` (`JsonSerializerOptions`): The options in use.

**Return value:** A `TimeSpan` parsed from the JSON token.

**Throws:** `JsonException` if the token is not a string or cannot be parsed as a `TimeSpan`. `FormatException` or `OverflowException` may propagate from `TimeSpan.Parse`.

---

### `Write` (TimeSpan converter)

```csharp
public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
```

Writes a `TimeSpan` value as a JSON string using the invariant culture format. Overrides `JsonConverter<TimeSpan>.Write`.

**Parameters:**
- `writer` (`Utf8JsonWriter`): The writer to output the value to.
- `value` (`TimeSpan`): The time-span value to write.
- `options` (`JsonSerializerOptions`): The options in use.

**Throws:** Nothing. The write operation is safe for all valid `TimeSpan` values.

---

### `WithCustomConverters`

```csharp
public static JsonSerializerOptions WithCustomConverters(JsonSerializerOptions baseOptions)
```

Creates a new `JsonSerializerOptions` instance by cloning the provided `baseOptions` and adding the project’s custom `DateTime` and `TimeSpan` converters. This is the recommended way to obtain a mutable options object that retains the standard converter set.

**Parameters:**
- `baseOptions` (`JsonSerializerOptions`): The options to clone and extend.

**Return value:** A new `JsonSerializerOptions` instance with the custom converters added.

**Throws:** `ArgumentNullException` if `baseOptions` is `null`.

## Usage

### Example 1: Serializing a service status record with compact options

```csharp
using systemd_service_monitor;

var status = new ServiceStatus
{
    Name = "sshd.service",
    ActiveState = "active",
    Uptime = TimeSpan.FromHours(72.5),
    LastCheck = DateTime.UtcNow
};

string compactJson = JsonSerializerConfiguration.Serialize(
    status,
    JsonSerializerConfiguration.GetCompactOptions);

Console.WriteLine(compactJson);
// Output: {"name":"sshd.service","activeState":"active","uptime":"3.00:30:00","lastCheck":"2025-03-15T08:12:34Z"}
```

### Example 2: Deserializing with custom converters on a modified options instance

```csharp
using System.Text.Json;
using systemd_service_monitor;

var baseOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = false
};

JsonSerializerOptions myOptions = JsonSerializerConfiguration.WithCustomConverters(baseOptions);

string json = @"{""name"":""nginx.service"",""activeState"":""inactive"",""uptime"":""1.02:15:00"",""lastCheck"":""2025-03-14T22:00:00Z""}";

ServiceStatus? status = JsonSerializerConfiguration.Deserialize<ServiceStatus>(json, myOptions);

Console.WriteLine(status?.Name);
// Output: nginx.service
```

## Notes

- All static `Get*Options` properties return the same cached instances on repeated access. These instances are effectively immutable and should not be altered via `Converters.Add` or property assignment. Use `WithCustomConverters` to derive a safe mutable copy.
- The custom `DateTime` converter expects a specific string format. Deserialization will fail with a `JsonException` if the input deviates from that format. Ensure external JSON payloads conform before deserializing.
- The custom `TimeSpan` converter uses `TimeSpan.Parse` with invariant culture. Round-trip fidelity depends on the string representation; avoid culture-specific or ambiguous formats.
- The `Serialize<T>` and `Deserialize<T>` overloads that accept `JsonSerializerOptions` throw `ArgumentNullException` when the options argument is `null`. The overloads without options use the default instance and do not accept a null value for the data parameter when `T` is a non-nullable reference type.
- All public static members are thread-safe. The cached options instances are initialized once and read concurrently without locks. `WithCustomConverters` creates a new object on each call and does not mutate shared state.
