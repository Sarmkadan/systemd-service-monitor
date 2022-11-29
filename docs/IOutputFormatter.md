# IOutputFormatter

Defines the contract for formatting output produced by the systemd-service-monitor tool. Implementations control serialization style, header injection, encoding, and whether null values appear in the final output. This interface allows the monitor to emit structured data in different formats without coupling the core logic to a specific presentation.

## API

### `bool PrettyPrint`

Gets a value indicating whether the formatter should produce indented, human-readable output rather than compact, machine-optimized output.

- **Value**: `true` if the output should be formatted with whitespace for readability; `false` for minimal, dense output.
- **Default behavior**: Implementation-defined; consumers should check this property before deciding on formatting style.
- **Exceptions**: None. This is a property getter.

### `bool IncludeNullValues`

Gets a value indicating whether properties or fields with `null` values should be explicitly included in the output.

- **Value**: `true` if null-valued entries are rendered (e.g., as explicit `null` tokens or empty nodes); `false` if they are omitted entirely.
- **Default behavior**: Implementation-defined. When `false`, the output is typically more compact but may lose schema completeness.
- **Exceptions**: None. This is a property getter.

### `Dictionary<string, string>? CustomHeaders`

Gets an optional dictionary of custom headers to include in the output stream, where each key is a header name and each value is the header content.

- **Value**: A dictionary of header name/value pairs, or `null` if no custom headers are specified.
- **Usage**: Formatters that produce text-based output (e.g., CSV, JSON streams, log-like formats) may prepend these as comment lines or metadata blocks. Binary formatters may ignore this property.
- **Exceptions**: None. This is a property getter. The returned dictionary, if non-null, should be treated as read-only by consumers.

### `System.Text.Encoding Encoding`

Gets the character encoding to use when writing formatted output to a stream or file.

- **Value**: A `System.Text.Encoding` instance (e.g., `Encoding.UTF8`, `Encoding.ASCII`).
- **Default behavior**: Implementations must return a non-null encoding. Consumers must respect this encoding when producing byte output.
- **Exceptions**: None. This is a property getter.

## Usage

### Example 1: Selecting a formatter based on PrettyPrint

```csharp
IOutputFormatter formatter = GetFormatter(userPreferences);

string payload = BuildServiceStatusPayload();

if (formatter.PrettyPrint)
{
    // Use indented serialization path
    string indented = SerializeIndented(payload, formatter.Encoding);
    Console.WriteLine(indented);
}
else
{
    // Use compact serialization path
    string compact = SerializeCompact(payload, formatter.Encoding);
    Console.WriteLine(compact);
}
```

### Example 2: Writing output with custom headers and encoding control

```csharp
async Task WriteReportAsync(Stream outputStream, IOutputFormatter formatter,
                            CancellationToken cancellationToken)
{
    // Write custom headers if present
    if (formatter.CustomHeaders is { Count: > 0 })
    {
        foreach (var kvp in formatter.CustomHeaders)
        {
            string headerLine = $"# {kvp.Key}: {kvp.Value}\n";
            byte[] headerBytes = formatter.Encoding.GetBytes(headerLine);
            await outputStream.WriteAsync(headerBytes, cancellationToken);
        }

        // Separator line
        byte[] separator = formatter.Encoding.GetBytes("\n");
        await outputStream.WriteAsync(separator, cancellationToken);
    }

    // Build the body, omitting nulls if configured
    var entries = GetServiceEntries().Where(e =>
        formatter.IncludeNullValues || e.Status != null);

    foreach (var entry in entries)
    {
        string line = FormatEntry(entry, formatter.IncludeNullValues);
        byte[] lineBytes = formatter.Encoding.GetBytes(line);
        await outputStream.WriteAsync(lineBytes, cancellationToken);
    }
}
```

## Notes

- **Encoding consistency**: All string-to-byte conversions must use the `Encoding` instance returned by the formatter. Mixing encodings (e.g., using `Encoding.UTF8` directly when the formatter specifies `Encoding.ASCII`) will produce corrupted output or runtime exceptions for characters outside the target encoding's range.
- **Null handling**: When `IncludeNullValues` is `false`, consumers must filter out null-valued entries before serialization. The formatter itself does not perform filtering; it merely advertises the desired behavior.
- **CustomHeaders nullability**: Consumers must guard against a `null` return from `CustomHeaders`. Attempting to iterate over a null dictionary will throw `NullReferenceException`. The property is explicitly nullable to allow formatters to signal "no headers" without allocating an empty dictionary.
- **Thread safety**: The interface defines only property getters, which are expected to return immutable or snapshot values. Implementations should ensure that property values do not change mid-operation once formatting begins. If an implementation allows mutation of these properties at runtime, callers must capture the values into local variables before starting a formatting pass to avoid inconsistent output.
- **Format-agnostic design**: The interface does not prescribe a serialization format. A single formatter might produce JSON, CSV, XML, or plain text. Code consuming `IOutputFormatter` should not assume a specific output structure beyond the metadata provided by these four members.
