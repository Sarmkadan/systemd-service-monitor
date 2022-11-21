# PaginationHelper

A utility class that provides common pagination calculations and metadata for splitting collections or query results into discrete pages. It supports both zero-based and one-based page numbering conventions and exposes metadata useful for building pagination UIs.

## API

### `public static (int PageNumber, int PageSize) ValidatePaginationParams(int pageNumber, int pageSize, int totalCount)`

Validates pagination parameters and returns a tuple of the validated values. Ensures `pageNumber` is at least 1 and `pageSize` is at least 1. If validation fails, throws an `ArgumentOutOfRangeException`.

- **Parameters**
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The number of items per page.
  - `totalCount`: The total number of items across all pages.
- **Returns**
  A tuple `(PageNumber, PageSize)` with validated values.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1`, `pageSize < 1`, or if `pageSize > totalCount` when `pageNumber > 1`.

---

### `public static int CalculateSkip(int pageNumber, int pageSize)`

Calculates the number of items to skip when retrieving a page of results from a zero-based collection.

- **Parameters**
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The number of items per page.
- **Returns**
  The number of items to skip before the start of the requested page.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1` or `pageSize < 1`.

---

### `public static int CalculateTotalPages(int totalCount, int pageSize)`

Calculates the total number of pages required to paginate a collection of the given size.

- **Parameters**
  - `totalCount`: The total number of items.
  - `pageSize`: The number of items per page.
- **Returns**
  The total number of pages. Returns 1 if `totalCount` is 0.
- **Throws**
  `ArgumentOutOfRangeException` if `pageSize < 1`.

---

### `public static bool HasNextPage(int pageNumber, int totalPages)`

Determines whether there is a next page available after the current one.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
  - `totalPages`: The total number of pages.
- **Returns**
  `true` if `pageNumber < totalPages`; otherwise, `false`.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1` or `totalPages < 0`.

---

### `public static bool HasPreviousPage(int pageNumber)`

Determines whether there is a previous page available before the current one.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
- **Returns**
  `true` if `pageNumber > 1`; otherwise, `false`.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1`.

---
### `public static int GetNextPageNumber(int pageNumber)`

Returns the page number of the next page, if it exists.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
- **Returns**
  The next page number, or the same number if no next page exists.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1`.

---
### `public static int GetPreviousPageNumber(int pageNumber)`

Returns the page number of the previous page, if it exists.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
- **Returns**
  The previous page number, or the same number if no previous page exists.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1`.

---
### `public static List<T> Paginate<T>(IEnumerable<T> source, int pageNumber, int pageSize)`

Splits an enumerable sequence into a single page of results.

- **Parameters**
  - `source`: The source collection to paginate.
  - `pageNumber`: The requested page number (1-based).
  - `pageSize`: The number of items per page.
- **Returns**
  A list containing the items for the requested page. Returns an empty list if `pageNumber` is out of range.
- **Throws**
  `ArgumentNullException` if `source` is `null`.
  `ArgumentOutOfRangeException` if `pageNumber < 1` or `pageSize < 1`.

---
### `public static PaginationMetadata GetMetadata(int pageNumber, int pageSize, int totalCount)`

Constructs pagination metadata for the given parameters.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
  - `pageSize`: The number of items per page.
  - `totalCount`: The total number of items.
- **Returns**
  A `PaginationMetadata` object containing page and range information.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1`, `pageSize < 1`, or `totalCount < 0`.

---
### `public static List<int> GetPageNumbers(int pageNumber, int totalPages)`

Generates a list of page numbers centered around the current page, suitable for pagination UI controls.

- **Parameters**
  - `pageNumber`: The current page number (1-based).
  - `totalPages`: The total number of pages.
- **Returns**
  A list of page numbers to display. Always includes the first and last pages, with surrounding pages when possible.
- **Throws**
  `ArgumentOutOfRangeException` if `pageNumber < 1` or `totalPages < 0`.

---
### `public int PageNumber`

Gets the current page number (1-based).

- **Value**
  The current page number.

---
### `public int PageSize`

Gets the number of items per page.

- **Value**
  The page size.

---
### `public int TotalCount`

Gets the total number of items across all pages.

- **Value**
  The total item count.

---
### `public int TotalPages`

Gets the total number of pages.

- **Value**
  The total page count.

---
### `public bool HasNextPage`

Gets a value indicating whether there is a next page available.

- **Value**
  `true` if there is a next page; otherwise, `false`.

---
### `public bool HasPreviousPage`

Gets a value indicating whether there is a previous page available.

- **Value**
  `true` if there is a previous page; otherwise, `false`.

---
### `public int StartIndex`

Gets the zero-based index of the first item on the current page.

- **Value**
  The start index.

---
### `public int EndIndex`

Gets the zero-based index of the last item on the current page.

- **Value**
  The end index.

## Usage

### Example 1: Basic Pagination with Metadata
