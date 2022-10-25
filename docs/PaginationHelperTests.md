# PaginationHelperTests

PaginationHelperTests provides a comprehensive test suite for the `PaginationHelper` utility class within the `systemd-service-monitor` project. These tests ensure the correctness of pagination arithmetic, parameter validation, and metadata generation required for rendering paginated data views.

## API

### `CalculateTotalPages_ZeroPageSize_ReturnsZero`
Validates that the total page calculation returns zero when the provided page size is zero, preventing division-by-zero exceptions in the underlying logic.

### `CalculateTotalPages_NotEvenlyDivisible_RoundsUp`
Verifies that the total number of pages is correctly rounded up when the total number of items is not perfectly divisible by the page size.

### `CalculateSkip_FirstPage_ReturnsZero`
Confirms that requesting the first page results in a skip count of zero, indicating that no items are skipped.

### `ValidatePaginationParams_NegativePageNumber_ClampsToOne`
Ensures that input page numbers less than one are clamped to the minimum valid page index of one.

### `GetPageNumbers_NearEndOfTotalPages_AdjustsWindowToFit`
Checks that the calculated window of visible page numbers correctly adjusts its boundary when nearing the final page of the total set.

### `GetMetadata_MiddlePage_ReportsCorrectStartAndEndIndex`
Validates that the pagination metadata accurately calculates the start and end indices of items for a page located in the middle of a dataset.

## Usage

Example 1: Basic validation test using xUnit:

```csharp
[Fact]
public void TotalPages_Calculation_Should_RoundUp()
{
    // Arrange
    var helper = new PaginationHelper();
    int totalItems = 10;
    int pageSize = 3;

    // Act
    var totalPages = helper.CalculateTotalPages(totalItems, pageSize);

    // Assert
    Assert.Equal(4, totalPages);
}
```

Example 2: Validation of clamping logic:

```csharp
[Fact]
public void PageNumber_Should_Clamp_To_Minimum()
{
    // Arrange
    var helper = new PaginationHelper();
    int requestedPage = -5;

    // Act
    var validatedPage = helper.ValidatePaginationParams(requestedPage);

    // Assert
    Assert.Equal(1, validatedPage);
}
```

## Notes

*   **Edge Cases**: The tests explicitly cover scenarios including zero page size, negative input parameters, and boundary conditions such as calculation near total page limits.
*   **Thread Safety**: These unit tests are designed to execute in isolation. While the `PaginationHelper` logic being tested is intended to be stateless and thread-safe, these tests do not explicitly validate concurrent access patterns.
