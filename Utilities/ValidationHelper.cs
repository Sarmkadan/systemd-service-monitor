#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace SystemdServiceMonitor.Utilities;

/// <summary>
/// Helper class for common validation operations.
/// Centralizes validation logic for service names, IDs, and other inputs.
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex ServiceNamePattern = new(
        @"^[a-zA-Z0-9._\-]+\.service$|^[a-zA-Z0-9._\-]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex IpAddressPattern = new(
        @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
        RegexOptions.Compiled);

    private static readonly Regex PortPattern = new(
        @"^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$",
        RegexOptions.Compiled);

    /// <summary>
    /// Validates a systemd service name.
    /// </summary>
    public static ValidationResult ValidateServiceName(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return ValidationResult.Invalid("Service name cannot be empty");
        }

        if (serviceName.Length > 255)
        {
            return ValidationResult.Invalid("Service name cannot exceed 255 characters");
        }

        if (!ServiceNamePattern.IsMatch(serviceName))
        {
            return ValidationResult.Invalid("Service name contains invalid characters");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Validates an IP address string.
    /// </summary>
    public static ValidationResult ValidateIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return ValidationResult.Invalid("IP address cannot be empty");
        }

        if (ipAddress.ToLower() == "localhost" || ipAddress == "127.0.0.1" || ipAddress == "::1")
        {
            return ValidationResult.Valid();
        }

        if (!IpAddressPattern.IsMatch(ipAddress))
        {
            return ValidationResult.Invalid("Invalid IP address format");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Validates a port number.
    /// </summary>
    public static ValidationResult ValidatePort(int port)
    {
        if (port < 1 || port > 65535)
        {
            return ValidationResult.Invalid("Port must be between 1 and 65535");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Validates a URL string.
    /// </summary>
    public static ValidationResult ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return ValidationResult.Invalid("URL cannot be empty");
        }

        try
        {
            var uri = new Uri(url);
            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                return ValidationResult.Invalid("URL must use HTTP or HTTPS protocol");
            }
            return ValidationResult.Valid();
        }
        catch
        {
            return ValidationResult.Invalid("Invalid URL format");
        }
    }

    /// <summary>
    /// Validates a time range (start date before end date).
    /// </summary>
    public static ValidationResult ValidateTimeRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            return ValidationResult.Invalid("Start date must be before end date");
        }

        if ((endDate - startDate).TotalDays > 365)
        {
            return ValidationResult.Invalid("Time range cannot exceed 365 days");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Validates pagination parameters.
    /// </summary>
    public static ValidationResult ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            return ValidationResult.Invalid("Page number must be at least 1");
        }

        if (pageSize < 1 || pageSize > 10000)
        {
            return ValidationResult.Invalid("Page size must be between 1 and 10000");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Validates string length constraints.
    /// </summary>
    public static ValidationResult ValidateStringLength(string? value, int minLength = 1, int maxLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (minLength > 0)
            {
                return ValidationResult.Invalid($"Value must be at least {minLength} characters");
            }
            return ValidationResult.Valid();
        }

        if (value.Length < minLength)
        {
            return ValidationResult.Invalid($"Value must be at least {minLength} characters");
        }

        if (value.Length > maxLength)
        {
            return ValidationResult.Invalid($"Value cannot exceed {maxLength} characters");
        }

        return ValidationResult.Valid();
    }

    /// <summary>
    /// Sanitizes user input by removing potentially dangerous characters.
    /// </summary>
    public static string SanitizeInput(string? input, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Trim and truncate
        var sanitized = input.Trim().Substring(0, Math.Min(input.Length, maxLength));

        // Remove control characters
        sanitized = Regex.Replace(sanitized, @"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", "");

        return sanitized;
    }

    /// <summary>
    /// Result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
