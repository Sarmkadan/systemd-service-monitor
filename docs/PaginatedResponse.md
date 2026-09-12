# PaginatedResponse

`PaginatedResponse<T>` is the standard response envelope for a single page of API results. It derives from `ApiResponse<List<T>>`, so `Data` contains the items on the current page while the response also carries pagination metadata and the common success, error, timestamp, and trace fields.

The generic type parameter `T` is the type of one item, not the list type. For example, `PaginatedResponse<ServiceLog>` exposes `Data` as `List<ServiceLog>?`.

## Inherited response fields

Because the class derives from `ApiResponse<List<T>>`, it inherits:

- `Data`: The current page as a `List<T>`, or `null` when no payload is supplied.
- `Success`: Whether the operation succeeded. Defaults to `true`.
- `Message`: A human-readable result message. Defaults to an empty string.
- `ErrorDetails`: Optional diagnostic information, normally used for failures.
- `Timestamp`: The response creation time as Unix time in seconds.
- `TraceId`: A generated response correlation identifier.

See [ApiResponse](ApiResponse.md) for more information about the base envelope.

## Pagination fields

### `public int PageNumber { get; set; }`

The current 1-based page number. The default is `1`.

### `public int PageSize { get; set; }`

The number of items requested per page. The default is `50`.

### `public int TotalCount { get; set; }`

The number of matching items across all pages. The default is `0`.

### `public int TotalPages { get; }`

The page count, calculated as `(TotalCount + PageSize - 1) / PageSize`. A zero `TotalCount` produces `0` pages.

### `public bool HasNextPage { get; }`

`true` when `PageNumber` is less than `TotalPages`; otherwise `false`.

### `public bool HasPreviousPage { get; }`

`true` when `PageNumber` is greater than `1`; otherwise `false`.

### `public int StartIndex { get; }`

The 1-based position of the first item represented by the page, calculated as `(PageNumber - 1) * PageSize + 1`.

### `public int EndIndex { get; }`

The 1-based position of the last available item on the page, calculated as the smaller of `PageNumber * PageSize` and `TotalCount`.

The computed properties are derived whenever they are read; they are not separately stored or set.

## Creating a response

Construct the response after applying the page window to the complete result set. `TotalCount` must describe the complete filtered set, while `Data` contains only the selected page.

```csharp
using SystemdServiceMonitor.Models;
using SystemdServiceMonitor.Responses;

var pageNumber = 2;
var pageSize = 25;
var matchingLogs = allLogs.OrderByDescending(log => log.Timestamp).ToList();

var response = new PaginatedResponse<ServiceLog>
{
    Data = matchingLogs
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToList(),
    PageNumber = pageNumber,
    PageSize = pageSize,
    TotalCount = matchingLogs.Count,
    Success = true,
    Message = "Logs retrieved successfully"
};
```

For an in-memory sequence, `ResultExtensions.ToPaginatedResponse` performs the count, `Skip`, and `Take` operations and creates the successful response:

```csharp
using SystemdServiceMonitor.Extensions;

PaginatedResponse<ServiceLog> response = allLogs.ToPaginatedResponse(
    pageNumber: 2,
    pageSize: 25,
    message: "Logs retrieved successfully");
```

This helper materializes the complete input sequence before selecting the page. For a database or other remote source, apply pagination in the query and construct `PaginatedResponse<T>` directly to avoid loading every matching item into memory.

## Reading pagination metadata

```csharp
if (response.HasNextPage)
{
    var nextPage = response.PageNumber + 1;
}

Console.WriteLine($"Showing {response.StartIndex}-{response.EndIndex} " +
                  $"of {response.TotalCount}");
```

For `PageNumber = 2`, `PageSize = 25`, and `TotalCount = 63`, the computed values are `TotalPages = 3`, `HasNextPage = true`, `HasPreviousPage = true`, `StartIndex = 26`, and `EndIndex = 50`.

## JSON shape

The application and the response JSON extensions use camel-case property names. A serialized response has this general shape:

```json
{
  "pageNumber": 1,
  "pageSize": 2,
  "totalCount": 3,
  "totalPages": 2,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "startIndex": 1,
  "endIndex": 2,
  "data": [
    { "unitName": "sshd.service" },
    { "unitName": "nginx.service" }
  ],
  "success": true,
  "message": "Services retrieved successfully",
  "timestamp": 1789142400,
  "traceId": "6fe4c75a-e81d-4b10-95f8-665995a25170"
}
```

Computed pagination properties are read-only in C#, but they are included when the response is serialized.

## Validation and edge cases

`PaginatedResponse<T>` does not validate values assigned to `PageNumber`, `PageSize`, or `TotalCount`. Callers should ensure that the page number and page size are at least `1` and that the total count is non-negative. In particular, a `PageSize` of `0` causes `TotalPages` to throw `DivideByZeroException` when read.

For an empty result (`TotalCount = 0`) with the normal first-page settings, `TotalPages` and `EndIndex` are `0`, `StartIndex` is `1`, and both navigation flags are `false`. Consumers that display an item range should handle the empty case explicitly instead of presenting `1-0`.
