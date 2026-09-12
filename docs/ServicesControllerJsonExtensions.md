# ServicesControllerJsonExtensions

`ServicesControllerJsonExtensions` provides `System.Text.Json` helpers for serializing and deserializing `ServicesController` instances. JSON uses web defaults with camel-case property names and is compact unless indented output is requested.

## API

### `string ToJson(this ServicesController value, bool indented = false)`

Serializes a controller instance to JSON.

- `value`: The controller to serialize. This method can be called as `controller.ToJson()`.
- `indented`: Set to `true` for human-readable indentation; the default produces compact JSON.
- Returns: The JSON representation of the controller's serializable public state.
- Throws `ArgumentNullException` when `value` is `null`.
- May propagate exceptions raised by `System.Text.Json` for unsupported values or serialization failures.

### `ServicesController? FromJson(string json)`

Deserializes JSON into a `ServicesController` instance.

- `json`: The JSON text to deserialize.
- Returns: The deserialized controller, or `null` when the input is malformed JSON or the JSON literal is `null`.
- Throws `ArgumentNullException` when `json` is `null`.
- Throws `ArgumentException` when `json` is empty.
- Exceptions other than `JsonException`, such as errors caused by an unsupported controller construction shape, are not caught.

This is a static helper rather than a `string` extension method, so call it through `ServicesControllerJsonExtensions`.

### `bool TryFromJson(string json, out ServicesController? value)`

Attempts to deserialize JSON into a `ServicesController` instance.

- `json`: The JSON text to deserialize.
- `value`: Receives the deserialized controller. It is set to `null` when malformed JSON raises `JsonException`.
- Returns: `true` when `JsonSerializer.Deserialize` completes without a `JsonException`; otherwise, `false`.
- Throws `ArgumentNullException` when `json` is `null`.
- Throws `ArgumentException` when `json` is empty.
- Exceptions other than `JsonException` are not converted to `false`.

Because the JSON literal `null` deserializes successfully, `TryFromJson("null", out value)` returns `true` with `value` set to `null`.

## Usage

```csharp
using SystemdServiceMonitor.Controllers;

string compactJson = controller.ToJson();
string readableJson = controller.ToJson(indented: true);

ServicesController? restored =
    ServicesControllerJsonExtensions.FromJson(compactJson);

if (ServicesControllerJsonExtensions.TryFromJson(compactJson, out var parsed))
{
    // Deserialization completed without malformed-JSON errors.
    // Check parsed for null before using it.
}
```

## Notes

- The shared serializer configuration uses `JsonSerializerDefaults.Web`, `JsonNamingPolicy.CamelCase`, `DefaultJsonTypeInfoResolver`, and compact output.
- Requesting indented output creates a copy of the shared options, so later calls using the default remain compact.
- `ServicesController` has constructor-injected service dependencies. Whether it can be reconstructed from JSON depends on whether `System.Text.Json` can bind and create that constructor shape; these helpers do not provide dependency-injection services or a custom converter.
- These methods serialize controller state only. They do not invoke controller actions or communicate with systemd.
