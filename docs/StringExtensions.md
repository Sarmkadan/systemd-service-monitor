# StringExtensions

The `StringExtensions` class provides a comprehensive set of static utility methods designed to extend the functionality of the standard C# `string` type. These methods address common string manipulation challenges, particularly those encountered when monitoring systemd services, handling configuration settings, and ensuring secure, readable log output.

## API

*   **Truncate(string input, int maxLength)**
    Truncates the input string to the specified maximum length. If the input is shorter than `maxLength`, the original string is returned.
*   **IsNullOrWhiteSpaceEx(string input)**
    Extends `string.IsNullOrWhiteSpace` with additional validation logic. Returns `true` if the input is `null`, empty, or consists only of white-space characters.
*   **ToPascalCase(string input)**
    Converts the input string to PascalCase (e.g., "my_service_name" becomes "MyServiceName").
*   **ToCamelCase(string input)**
    Converts the input string to camelCase (e.g., "MyServiceName" becomes "myServiceName").
*   **ToSnakeCase(string input)**
    Converts the input string to snake_case (e.g., "MyServiceName" becomes "my_service_name").
*   **IsValidServiceName(string serviceName)**
    Validates if the provided string conforms to the naming conventions required for a valid systemd service identifier. Returns `true` if valid, otherwise `false`.
*   **SanitizeForLogging(string input)**
    Removes or escapes sensitive or problematic characters from the input string to ensure it is safe for inclusion in log files.
*   **ToList(string input, char separator)**
    Splits the input string into a `List<string>` based on the provided character separator.
*   **ContainsAny(string input, params string[] values)**
    Checks if the input string contains any of the provided search strings. Returns `true` if at least one match is found.
*   **Repeat(string input, int count)**
    Returns a new string consisting of the input string repeated the specified number of times. Throws an `ArgumentOutOfRangeException` if `count` is negative.
*   **WrapText(string input, int width)**
    Wraps the input text to the specified maximum width, inserting line breaks as necessary to maintain readability.
*   **LevenshteinDistance(string source, string target)**
    Calculates the Levenshtein distance between the `source` and `target` strings, representing the minimum number of single-character edits required to change one string into the other.

## Usage

### Example 1: Validating and Sanitizing Service Data
```csharp
string rawInput = "  my-app.service  ";
if (StringExtensions.IsNullOrWhiteSpaceEx(rawInput)) {
    // Handle error
}

string serviceName = rawInput.Trim();
if (StringExtensions.IsValidServiceName(serviceName)) {
    string logSafeName = StringExtensions.SanitizeForLogging(serviceName);
    Console.WriteLine($"Monitoring service: {logSafeName}");
}
```

### Example 2: Transforming Configuration Strings
```csharp
string configValue = "db_connection_string";
string pascalCase = StringExtensions.ToPascalCase(configValue); // "DbConnectionString"

string rawList = "item1,item2,item3";
List<string> items = StringExtensions.ToList(rawList, ',');

if (StringExtensions.ContainsAny(configValue, "connection", "db")) {
    Console.WriteLine("Database configuration detected.");
}
```

## Notes

*   **Thread Safety:** As these methods are implemented as static utility functions that do not maintain internal state, they are inherently thread-safe.
*   **Null Handling:** Methods that accept a `string` input should generally be treated as null-safe, typically returning an empty string or `false` when a null input is provided, depending on the specific return type of the method. Users should verify specific behaviors in the underlying implementation if strict null handling is required.
*   **Performance:** Be mindful of frequent string allocations, particularly with methods like `Repeat` or transformation methods used in high-frequency loops. For critical path performance tuning, consider the overhead of these operations.
