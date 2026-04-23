#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SystemdServiceMonitor.Formatters;

/// <summary>
/// Interface for output formatting plugins.
/// Allows extensible formatting of API responses in different formats (JSON, CSV, XML, etc.).
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Gets the file extension for this format.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Formats an object to the output format.
    /// </summary>
    byte[] Format<T>(T data, FormattingOptions? options = null) where T : class;

    /// <summary>
    /// Formats a collection of objects to the output format.
    /// </summary>
    byte[] FormatCollection<T>(IEnumerable<T> data, FormattingOptions? options = null) where T : class;
}

/// <summary>
/// Options for configuring output formatting.
/// </summary>
public class FormattingOptions
{
    /// <summary>
    /// Whether to include pretty formatting (indentation, line breaks).
    /// </summary>
    public bool PrettyPrint { get; set; } = true;

    /// <summary>
    /// Whether to include null values in output.
    /// </summary>
    public bool IncludeNullValues { get; set; } = false;

    /// <summary>
    /// Custom headers to include (for CSV).
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }

    /// <summary>
    /// Character encoding to use.
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;
}
