# DateTimeExtensions

The `DateTimeExtensions` class provides a set of static utility methods for manipulating, converting, and formatting `DateTime` structures within the `systemd-service-monitor` project. These extensions facilitate common time-related operations, including interoperability with Unix timestamps, standardized ISO 8601 formatting, interval rounding, and relative time representation.

## API

*   **`ToRelativeTime(DateTime dateTime, DateTime? referenceTime = null)`**
    *   **Purpose:** Generates a human-readable string representing the elapsed time between `dateTime` and the optional `referenceTime`.
    *   **Parameters:** `dateTime` to evaluate; `referenceTime` (defaults to `DateTime.UtcNow` if null).
    *   **Returns:** A `string` (e.g., "5 minutes ago", "in 2 hours").
*   **`ToUnixTimestamp(DateTime dateTime)`**
    *   **Purpose:** Converts a `DateTime` to a Unix timestamp in seconds.
    *   **Parameters:** `dateTime` to convert.
    *   **Returns:** A `long` representing seconds since the Unix epoch (January 1, 1970).
*   **`ToUnixTimestampMilliseconds(DateTime dateTime)`**
    *   **Purpose:** Converts a `DateTime` to a Unix timestamp in milliseconds.
    *   **Parameters:** `dateTime` to convert.
    *   **Returns:** A `long` representing milliseconds since the Unix epoch.
*   **`FromUnixTimestamp(long timestamp)`**
    *   **Purpose:** Creates a `DateTime` object from a Unix timestamp in seconds.
    *   **Parameters:** `timestamp` (seconds).
    *   **Returns:** A `DateTime` representing the specified time.
*   **`FromUnixTimestampMilliseconds(long timestamp)`**
    *   **Purpose:** Creates a `DateTime` object from a Unix timestamp in milliseconds.
    *   **Parameters:** `timestamp` (milliseconds).
    *   **Returns:** A `DateTime` representing the specified time.
*   **`ToIso8601String(DateTime dateTime)`**
    *   **Purpose:** Formats a `DateTime` object into an ISO 8601 compliant string.
    *   **Parameters:** `dateTime` to format.
    *   **Returns:** A `string` representation of the date and time.
*   **`IsWithinRange(DateTime dateTime, DateTime start, DateTime end)`**
    *   **Purpose:** Determines if a given `DateTime` falls within a specified inclusive range.
    *   **Parameters:** `dateTime` to check; `start` boundary; `end` boundary.
    *   **Returns:** A `bool` indicating if the `dateTime` is within the range.
*   **`RoundToNearest(DateTime dateTime, TimeSpan interval)`**
    *   **Purpose:** Rounds the `DateTime` instance to the nearest specified `TimeSpan` interval.
    *   **Parameters:** `dateTime` to round; `interval` for rounding.
    *   **Returns:** A `DateTime` rounded to the nearest interval.
*   **`StartOfDay(DateTime dateTime)`**
    *   **Purpose:** Returns the start of the day (00:00:00.000) for the given `DateTime`.
    *   **Parameters:** `dateTime` instance.
    *   **Returns:** A `DateTime` object set to the beginning of the day.
*   **`EndOfDay(DateTime dateTime)`**
    *   **Purpose:** Returns the end of the day (23:59:59.999...) for the given `DateTime`.
    *   **Parameters:** `dateTime` instance.
    *   **Returns:** A `DateTime` object set to the end of the day.
*   **`StartOfHour(DateTime dateTime)`**
    *   **Purpose:** Returns the start of the hour (0 minutes, 0 seconds) for the given `DateTime`.
    *   **Parameters:** `dateTime` instance.
    *   **Returns:** A `DateTime` object set to the beginning of the hour.
*   **`EndOfHour(DateTime dateTime)`**
    *   **Purpose:** Returns the end of the hour (59 minutes, 59 seconds...) for the given `DateTime`.
    *   **Parameters:** `dateTime` instance.
    *   **Returns:** A `DateTime` object set to the end of the hour.
*   **`ToHumanReadableString(DateTime dateTime)`**
    *   **Purpose:** Formats a `DateTime` into a user-friendly string representation suitable for display.
    *   **Parameters:** `dateTime` to format.
    *   **Returns:** A formatted `string`.

## Usage

```csharp
// Example 1: Unix timestamp conversion and range checking
var now = DateTime.UtcNow;
long unixSeconds = now.ToUnixTimestamp();

// Convert back from milliseconds
var fromMs = DateTimeExtensions.FromUnixTimestampMilliseconds(now.ToUnixTimestampMilliseconds());

// Check if a timestamp is within the last 5 minutes
bool isRecent = now.IsWithinRange(now.AddMinutes(-5), now);
```

```csharp
// Example 2: Rounding and time boundary manipulation
var current = DateTime.Now;

// Round to the nearest 15-minute interval
var rounded = current.RoundToNearest(TimeSpan.FromMinutes(15));

// Get boundaries for processing logs or time-based data
var dayStart = current.StartOfDay();
var hourEnd = current.EndOfHour();
```

## Notes

*   **Thread Safety:** These methods are static and operate on `DateTime` structures, which are immutable value types in C#. Consequently, these methods are inherently thread-safe and can be called concurrently without additional synchronization.
*   **Unix Epoch Assumptions:** Methods converting to or from Unix timestamps assume the `DateTime` values represent UTC time. Using local time with these methods may lead to unexpected discrepancies.
*   **Precision and Boundaries:** `EndOfDay` and `EndOfHour` methods may be subject to platform-specific precision limitations inherent to the `DateTime` type (e.g., tick granularity). Extreme date values outside the range supportable by `long` representation (for milliseconds) may cause overflow exceptions.
