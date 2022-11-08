# RateLimitingMiddleware

Middleware component that enforces rate limiting on incoming HTTP requests using a token bucket algorithm. It tracks request frequency and rejects or delays requests when the configured rate limit is exceeded, preventing abuse and ensuring fair resource usage.

## API

### `public class RateLimitingMiddleware`

Middleware class that implements rate limiting using a token bucket strategy. Configured via constructor parameters to define capacity and refill behavior.

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware pipeline with rate limiting enforcement.

- **Parameters**
  - `context` – The current HTTP context.
  - `next` – The next middleware delegate in the pipeline.
- **Return Value**
  - A `Task` representing the asynchronous operation.
- **Throws**
  - `ArgumentNullException` – If `context` or `next` is `null`.

### `public TokenBucket`

Gets the underlying token bucket instance used for rate limiting.

- **Return Value**
  - The `TokenBucket` instance managing token consumption and refill logic.

### `public bool TryConsumeToken()`

Attempts to consume a token from the bucket.

- **Return Value**
  - `true` if a token was consumed; otherwise, `false` if the bucket is empty.
- **Remarks**
  - Thread-safe operation. May return `false` under high concurrency even if tokens are available due to concurrent consumption.

### `public int RequestsPerMinute`

Gets the maximum number of requests allowed per minute.

- **Return Value**
  - The configured requests-per-minute limit.

### `public int RefillIntervalSeconds`

Gets the interval in seconds at which tokens are refilled.

- **Return Value**
  - The refill interval in seconds.

### `public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)`

Adds the `RateLimitingMiddleware` to the application pipeline with default settings.

- **Parameters**
  - `app` – The `IApplicationBuilder` instance.
- **Return Value**
  - The `IApplicationBuilder` for chaining.
- **Throws**
  - `ArgumentNullException` – If `app` is `null`.

### `public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, int requestsPerMinute, int refillIntervalSeconds)`

Adds the `RateLimitingMiddleware` to the application pipeline with custom rate limiting parameters.

- **Parameters**
  - `app` – The `IApplicationBuilder` instance.
  - `requestsPerMinute` – Maximum requests allowed per minute.
  - `refillIntervalSeconds` – Seconds between token refills.
- **Return Value**
  - The `IApplicationBuilder` for chaining.
- **Throws**
  - `ArgumentNullException` – If `app` is `null`.
  - `ArgumentOutOfRangeException` – If `requestsPerMinute` or `refillIntervalSeconds` is less than or equal to zero.

## Usage

### Basic Usage with Defaults
