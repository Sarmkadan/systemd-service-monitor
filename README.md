// ... existing content ...
## StringExtensions

The `StringExtensions` class provides a set of static methods for manipulating and validating strings. These methods can be used to truncate strings, check for whitespace, convert between different casing conventions, and more.

### Usage Example

```csharp
using SystemdServiceMonitor.Extensions;

// Truncate a string to 10 characters
var truncatedString = StringExtensions.Truncate("This is a long string", 10);
Console.WriteLine($"Truncated string: {truncatedString}");

// Check if a string is null or whitespace
var isWhitespace = StringExtensions.IsNullOrWhiteSpaceEx("   ");
Console.WriteLine($"Is whitespace: {isWhitespace}");

// Convert a string to Pascal case
var pascalCase = StringExtensions.ToPascalCase("hello world");
Console.WriteLine($"Pascal case: {pascalCase}");

// Check if a string is a valid service name
var isValidName = StringExtensions.IsValidServiceName("my-service");
Console.WriteLine($"Is valid service name: {isValidName}");

// Sanitize a string for logging
var sanitizedString = StringExtensions.SanitizeForLogging("This is a string with special characters!");
Console.WriteLine($"Sanitized string: {sanitizedString}");

// Split a string into a list of substrings
var substrings = StringExtensions.ToList("hello,world,foo,bar");
Console.WriteLine($"Substrings: [{string.Join(", ", substrings)}]");

// Check if a string contains any of a set of characters
var containsAny = StringExtensions.ContainsAny("hello", new[] { 'l', 'o' });
Console.WriteLine($"Contains any: {containsAny}");

// Repeat a string a specified number of times
var repeatedString = StringExtensions.Repeat("hello", 3);
Console.WriteLine($"Repeated string: {repeatedString}");

// Wrap text to a specified width
var wrappedText = StringExtensions.WrapText("This is a long string of text", 20);
Console.WriteLine($"Wrapped text: {wrappedText}");

// Calculate the Levenshtein distance between two strings
var distance = StringExtensions.LevenshteinDistance("kitten", "sitting");
Console.WriteLine($"Levenshtein distance: {distance}");
```

// ... existing content ...
