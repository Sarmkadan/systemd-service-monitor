# PaginationHelperTestsExtensions

The `PaginationHelperTestsExtensions` static class provides helper methods for unit testing pagination logic within the `systemd-service-monitor` project. It is designed to simplify the creation and validation of `PaginationMetadata` objects, as well as to compute expected total pages and index ranges based on common pagination parameters. All members are intended for use in test fixtures and assertions.

## API

### `CreateMetadata`

```csharp
public static PaginationMetadata CreateMetadata(int totalItems, int pageSize, int currentPage)
```

Creates a `PaginationMetadata` instance using the specified total number of items, page size, and current page number.

- **Parameters**  
  - `totalItems` – The total number of items across all pages.  
  - `pageSize` – The number of items per page. Must be greater than zero.  
  - `currentPage` – The one-based index of the current page. Must be at least 1.

- **Returns**  
  A fully populated `PaginationMetadata` object with properties such as `TotalItems`, `PageSize`, `CurrentPage`, `TotalPages`, `HasPreviousPage`, `HasNextPage`, etc., computed according to the project's pagination conventions.

- **Throws**  
  - `ArgumentOutOfRangeException` if `pageSize` ≤ 0 or `currentPage` < 1.  
  - `ArgumentOutOfRangeException` if `totalItems` < 0.

### `ValidateMetadata`

```csharp
public static void ValidateMetadata(PaginationMetadata metadata, int totalItems, int pageSize, int currentPage)
```

Validates that the given `PaginationMetadata` instance matches the expected values derived from the provided parameters. Typically used in assertions to verify that a pagination helper produced correct metadata.

- **Parameters**  
  - `metadata` – The `PaginationMetadata` instance to validate.  
  - `totalItems` – The expected total number of items.  
  - `pageSize` – The expected page size.  
  - `currentPage` – The expected current page number.

- **Returns**  
  Nothing.

- **Throws**  
  - `ArgumentNullException` if `metadata` is `null`.  
  - `ArgumentException` if any property of `metadata` does not match the expected value (e.g., `TotalItems`, `PageSize`, `CurrentPage`, `TotalPages`, `HasPreviousPage`, `HasNextPage`, `FirstItemIndex`, `LastItemIndex`).

### `CalculateExpectedTotalPages`

```csharp
public static int CalculateExpectedTotalPages(int totalItems, int pageSize)
```

Computes the expected number of total pages given the total item count and page size.

- **Parameters**  
  - `totalItems` – The total number of items.  
  - `pageSize` – The number of items per page. Must be greater than zero.

- **Returns**  
  An integer representing the total number of pages. Returns 0 if `totalItems` is 0; otherwise returns the ceiling of `totalItems / pageSize`.

- **Throws**  
  - `ArgumentOutOfRangeException` if `pageSize` ≤ 0.  
  - `ArgumentOutOfRangeException` if `totalItems` < 0.

### `CalculateExpectedIndices`

```csharp
public static (int StartIndex, int EndIndex) CalculateExpectedIndices(int totalItems, int pageSize, int currentPage)
```

Calculates the zero-based start and end indices for the items that should appear on the specified page.

- **Parameters**  
  - `totalItems` – The total number of items.  
  - `pageSize` – The number of items per page. Must be greater than zero.  
  - `currentPage` – The one-based page number. Must be at least 1.

- **Returns**  
  A tuple `(int StartIndex, int EndIndex)` where `StartIndex` is the zero-based index of the first item on the page, and `EndIndex` is the zero-based index of the last item on the page (inclusive). If the page is empty (e.g., `totalItems` is 0 or `currentPage` exceeds the total number of pages), both indices are set to -1.

- **Throws**  
  - `ArgumentOutOfRangeException` if `pageSize` ≤ 0 or `currentPage` < 1.  
  - `ArgumentOutOfRangeException` if `totalItems` < 0.

## Usage

### Example 1: Creating and validating metadata in a unit test

```csharp
[Fact]
public void GetPaginationMetadata_ReturnsCorrectMetadata()
{
    // Arrange
    int totalItems = 25;
    int pageSize = 10;
    int currentPage = 2;

    // Act – assume a method under test that returns PaginationMetadata
    var metadata = SomePaginationHelper.GetMetadata(totalItems, pageSize, currentPage);

    // Assert using the test extensions
    PaginationHelperTestsExtensions.ValidateMetadata(metadata, totalItems, pageSize, currentPage);
}
```

### Example 2: Computing expected indices for a data retrieval test

```csharp
[Fact]
public void GetPageItems_ReturnsCorrectSlice()
{
    // Arrange
    var allItems = Enumerable.Range(0, 25).ToList();
    int pageSize = 10;
    int currentPage = 3;

    // Act – compute expected indices
    var (start, end) = PaginationHelperTestsExtensions.CalculateExpectedIndices(
        allItems.Count, pageSize, currentPage);

    // Act – retrieve items using the method under test
    var pageItems = SomePaginationHelper.GetPage(allItems, pageSize, currentPage);

    // Assert
    var expectedItems = allItems[start..(end + 1)];
    Assert.Equal(expectedItems, pageItems);
}
```

## Notes

- **Edge cases**  
  - When `totalItems` is 0, `CalculateExpectedTotalPages` returns 0, and `CalculateExpectedIndices` returns `(-1, -1)` regardless of `currentPage`.  
  - When `currentPage` exceeds the total number of pages, `CalculateExpectedIndices` returns `(-1, -1)`.  
  - `CreateMetadata` and `ValidateMetadata` treat a `currentPage` of 1 as valid even when `totalItems` is 0; the resulting metadata will have `TotalPages` = 0 and no items.  
  - All methods throw `ArgumentOutOfRangeException` for negative `totalItems` or non-positive `pageSize`/`currentPage`.

- **Thread safety**  
  All methods are static and do not access any shared mutable state. They are inherently thread-safe and can be called concurrently from multiple test threads without synchronization.
