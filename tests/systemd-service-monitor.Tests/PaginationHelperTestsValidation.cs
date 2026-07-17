#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validation helpers for <see cref="PaginationHelperTests"/> to ensure test data is valid.
/// </summary>
public static class PaginationHelperTestsValidation
{
    /// <summary>
    /// Validates a <see cref="PaginationHelperTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new List<string>().AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="PaginationHelperTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return true;
    }

    /// <summary>
    /// Ensures that the <see cref="PaginationHelperTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this PaginationHelperTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}