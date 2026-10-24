#nullable enable

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

/// <summary>
/// Logger that keeps every entry with the UTC time it was written, so tests can observe
/// what a component reported and when.
/// </summary>
/// <typeparam name="T">The category type.</typeparam>
public sealed class RecordingLogger<T> : ILogger<T>
{
    private readonly ConcurrentQueue<Entry> _entries = new();

    /// <summary>
    /// A single recorded log entry.
    /// </summary>
    /// <param name="TimestampUtc">When the entry was written.</param>
    /// <param name="Level">The log level.</param>
    /// <param name="Message">The formatted message.</param>
    /// <param name="Values">Structured values from the message template, when the call provided them.</param>
    public sealed record Entry(
        DateTime TimestampUtc,
        LogLevel Level,
        string Message,
        IReadOnlyList<KeyValuePair<string, object?>>? Values);

    /// <summary>
    /// All entries in the order they were written.
    /// </summary>
    public IReadOnlyList<Entry> Entries => _entries.ToArray();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _entries.Enqueue(new Entry(
            DateTime.UtcNow,
            logLevel,
            formatter(state, exception),
            state as IReadOnlyList<KeyValuePair<string, object?>>));
    }
}
