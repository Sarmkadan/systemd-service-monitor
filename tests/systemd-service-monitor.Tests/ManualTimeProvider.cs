#nullable enable

/// <summary>
/// Controllable <see cref="TimeProvider"/> for deterministic tests. Time moves only when a test advances it.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    /// <summary>Creates a clock frozen at <paramref name="start"/>.</summary>
    public ManualTimeProvider(DateTimeOffset start) => _utcNow = start;

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow() => _utcNow;

    /// <summary>Moves the clock forward by <paramref name="by"/>.</summary>
    public void Advance(TimeSpan by) => _utcNow += by;
}
