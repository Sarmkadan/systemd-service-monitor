// ... rest of the file content ...
## ResultExtensions

The `ResultExtensions` class provides a set of static extension methods for working with `ApiResponse` and `PaginatedResponse` types. It simplifies common operations such as mapping, transforming, and checking the state of these types.

### Usage Example

```csharp
using SystemdServiceMonitor.Extensions;

// Create an ApiResponse instance
var response = new ApiResponse<string>("Success message");

// Convert to success response
var successResponse = response.ToSuccess(); // ApiResponse<string> with "Success message"

// Convert to error response
var errorResponse = response.ToError("Error message"); // ApiResponse<string> with error code and message

// Map to a different type
var mappedResponse = response.Map<string, int>().Value; // int value

// Check if response has data
bool hasData = response.HasData(); // true

// Get data or throw an exception
var data = response.GetDataOrThrow(); // string value

// Get data or return default value
var dataOrDefault = response.GetDataOrDefault(); // string value or null
```
// ... rest of the file content ...
