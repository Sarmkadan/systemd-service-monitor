#nullable enable

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Helper class for resolving and validating systemd service paths.
/// Handles path normalization, unit file location discovery, and path security.
/// </summary>
public static class PathResolver
{
    /// <summary>
    /// Standard systemd system unit directories.
    /// </summary>
    private static readonly string[] SystemUnitPaths = new[]
    {
        "/etc/systemd/system",
        "/run/systemd/system",
        "/lib/systemd/system",
        "/usr/lib/systemd/system",
        "/etc/systemd/user",
        "/run/systemd/user",
        "/lib/systemd/user",
        "/usr/lib/systemd/user"
    };

    /// <summary>
    /// Gets all standard systemd unit search paths.
    /// </summary>
    public static IEnumerable<string> GetSystemUnitPaths() => SystemUnitPaths;

    /// <summary>
    /// Gets the default system unit directory (where user can write).
    /// </summary>
    public static string GetDefaultSystemUnitDirectory()
    {
        return "/etc/systemd/system";
    }

    /// <summary>
    /// Gets the default user unit directory.
    /// </summary>
    public static string GetDefaultUserUnitDirectory()
    {
        var home = Environment.GetEnvironmentVariable("HOME") ?? "/root";
        return Path.Combine(home, ".config", "systemd", "user");
    }

    /// <summary>
    /// Normalizes a service name to ensure it has the .service extension.
    /// </summary>
    public static string NormalizeServiceName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return serviceName;

        serviceName = serviceName.Trim();

        if (!serviceName.EndsWith(".service", StringComparison.OrdinalIgnoreCase))
        {
            serviceName += ".service";
        }

        return serviceName.ToLowerInvariant();
    }

    /// <summary>
    /// Removes the .service extension from a service name if present.
    /// </summary>
    public static string RemoveServiceExtension(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return serviceName;

        if (serviceName.EndsWith(".service", StringComparison.OrdinalIgnoreCase))
        {
            return serviceName.Substring(0, serviceName.Length - 8);
        }

        return serviceName;
    }

    /// <summary>
    /// Finds the full path to a service unit file.
    /// Searches standard systemd directories.
    /// </summary>
    public static string? FindServiceUnitFile(string serviceName)
    {
        var normalizedName = NormalizeServiceName(serviceName);

        foreach (var unitPath in SystemUnitPaths)
        {
            var fullPath = Path.Combine(unitPath, normalizedName);

            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            // Also check symlink targets
            try
            {
                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Exists)
                {
                    return fileInfo.FullName;
                }
            }
            catch
            {
                // Continue if we can't access the file
            }
        }

        return null;
    }

    /// <summary>
    /// Validates a service unit file path for security.
    /// Prevents path traversal and ensures the path is within systemd directories.
    /// </summary>
    public static bool IsValidServicePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(filePath);

            // Check for path traversal attempts
            if (fullPath.Contains(".."))
                return false;

            // Ensure the file is in a systemd directory
            return SystemUnitPaths.Any(unitPath =>
                fullPath.StartsWith(unitPath, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the directory where a service unit file is located.
    /// </summary>
    public static string? GetServiceDirectory(string serviceName)
    {
        var unitFile = FindServiceUnitFile(serviceName);
        return unitFile is not null ? Path.GetDirectoryName(unitFile) : null;
    }

    /// <summary>
    /// Checks if a service is a system service or user service.
    /// </summary>
    public static ServiceScope GetServiceScope(string serviceName)
    {
        var unitFile = FindServiceUnitFile(serviceName);

        if (unitFile is null)
            return ServiceScope.Unknown;

        // Check if it's in a user-specific directory
        if (unitFile.Contains("/.config/systemd/user") ||
            unitFile.Contains("/run/systemd/user") ||
            unitFile.Contains("/lib/systemd/user") ||
            unitFile.Contains("/usr/lib/systemd/user") ||
            unitFile.Contains("/etc/systemd/user"))
        {
            return ServiceScope.User;
        }

        return ServiceScope.System;
    }

    /// <summary>
    /// Gets related service files (dependencies, after, before units).
    /// </summary>
    public static List<string> GetRelatedServices(string serviceName)
    {
        var related = new List<string>();
        var unitFile = FindServiceUnitFile(serviceName);

        if (string.IsNullOrEmpty(unitFile) || !File.Exists(unitFile))
            return related;

        try
        {
            var content = File.ReadAllText(unitFile);

            // Extract After, Before, Requires, Wants directives
            var patterns = new[] { "After=", "Before=", "Requires=", "Wants=", "PartOf=" };

            foreach (var pattern in patterns)
            {
                foreach (var line in content.Split('\n'))
                {
                    if (line.Contains(pattern))
                    {
                        var value = line.Split('=').Last().Trim();
                        var services = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        related.AddRange(services);
                    }
                }
            }
        }
        catch
        {
            // Ignore errors reading the file
        }

        return related.Distinct().ToList();
    }
}

/// <summary>
/// Scope of a systemd service (system-wide or user-specific).
/// </summary>
public enum ServiceScope
{
    System,
    User,
    Unknown
}
