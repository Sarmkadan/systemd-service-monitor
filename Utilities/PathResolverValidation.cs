#nullable enable

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Provides validation methods for <see cref="PathResolver"/>.
/// Validates path strings, service names, and related properties for correctness and security.
/// </summary>
public static class PathResolverValidation
{
    /// <summary>
    /// Validates the <see cref="PathResolver"/> and returns a list of human-readable problems.
    /// </summary>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate system unit paths
        if (PathResolver.GetSystemUnitPaths() is null)
        {
            problems.Add("SystemUnitPaths collection is null");
        }
        else if (!PathResolver.GetSystemUnitPaths().Any())
        {
            problems.Add("SystemUnitPaths collection is empty");
        }
        else
        {
            foreach (var path in PathResolver.GetSystemUnitPaths())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    problems.Add("SystemUnitPaths contains null or whitespace entry");
                }
                else if (path.Contains(".."))
                {
                    problems.Add($"SystemUnitPaths contains invalid path with path traversal: '{path}'");
                }
            }
        }

        // Validate default system unit directory
        try
        {
            var defaultSystemDir = PathResolver.GetDefaultSystemUnitDirectory();
            if (string.IsNullOrWhiteSpace(defaultSystemDir))
            {
                problems.Add("GetDefaultSystemUnitDirectory() returned null or whitespace");
            }
            else if (defaultSystemDir.Contains(".."))
            {
                problems.Add($"GetDefaultSystemUnitDirectory() returned invalid path with path traversal: '{defaultSystemDir}'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetDefaultSystemUnitDirectory() threw exception: {ex.Message}");
        }

        // Validate default user unit directory
        try
        {
            var defaultUserDir = PathResolver.GetDefaultUserUnitDirectory();
            if (string.IsNullOrWhiteSpace(defaultUserDir))
            {
                problems.Add("GetDefaultUserUnitDirectory() returned null or whitespace");
            }
            else if (defaultUserDir.Contains(".."))
            {
                problems.Add($"GetDefaultUserUnitDirectory() returned invalid path with path traversal: '{defaultUserDir}'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GetDefaultUserUnitDirectory() threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="PathResolver"/> is valid.
    /// </summary>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="PathResolver"/> is valid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="PathResolver"/> is invalid, containing a list of problems.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PathResolver is invalid. Problems:\n- {string.Join("\n- ", problems)}");
        }
    }
}
