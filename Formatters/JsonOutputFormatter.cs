#nullable enable
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options; // Assuming we might need options for JsonSerializer

namespace SystemdServiceMonitor.Formatters;

/// <summary>
/// Implements IOutputFormatter for JSON serialization.
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public JsonOutputFormatter(IOptions<JsonSerializerOptions> options)
    {
        _jsonSerializerOptions = options.Value ?? new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public string MimeType => "application/json";

    public string FileExtension => "json";

    public byte[] Format<T>(T data, FormattingOptions? options = null) where T : class
    {
        var jsonOptions = GetJsonSerializerOptions(options);
        string jsonString = JsonSerializer.Serialize(data, jsonOptions);
        return (options?.Encoding ?? Encoding.UTF8).GetBytes(jsonString);
    }

    public byte[] FormatCollection<T>(IEnumerable<T> data, FormattingOptions? options = null) where T : class
    {
        var jsonOptions = GetJsonSerializerOptions(options);
        string jsonString = JsonSerializer.Serialize(data, jsonOptions);
        return (options?.Encoding ?? Encoding.UTF8).GetBytes(jsonString);
    }

    private JsonSerializerOptions GetJsonSerializerOptions(FormattingOptions? options)
    {
        if (options == null)
        {
            return _jsonSerializerOptions;
        }

        return new JsonSerializerOptions
        {
            WriteIndented = options.PrettyPrint,
            DefaultIgnoreCondition = options.IncludeNullValues ? JsonIgnoreCondition.Never : JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Assuming camelCase for JSON output
            // Copy other default options from _jsonSerializerOptions if needed
        };
    }
}
