
# ASP.NET Core Rate Limiting Example

This project demonstrates how to implement different rate limiting strategies in an ASP.NET Core application using the built-in `Microsoft.AspNetCore.RateLimiting` library. The example covers various rate limiting techniques including fixed window, sliding window, token bucket, and concurrency limiters.

## Getting Started

### Prerequisites

- .NET 8 SDK installed
- A code editor like Visual Studio Code or Visual Studio

### How to Run the Application

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd <repository-folder>
   ```
2. Build and run the application:
   ```bash
   dotnet run
   ```
3. Open your browser and navigate to:
   - `http://localhost:5000/fixed` - Endpoint with fixed window rate limiting
   - `http://localhost:5000/sliding` - Endpoint with sliding window rate limiting
   - `http://localhost:5000/token` - Endpoint with token bucket rate limiting
   - `http://localhost:5000/concurrency` - Endpoint with concurrency limiting

---

## Rate Limiting Strategies Used

### 1. Fixed Window Limiter

- **Description**: Limits requests to a maximum of 20 requests every 2 minutes. If the limit is reached, all subsequent requests within the same window are blocked until the window resets.
- **Configuration**:
   ```csharp
   options.AddFixedWindowLimiter("fixed", opt =>
   {
       opt.Window = TimeSpan.FromMinutes(2);
       opt.PermitLimit = 20;
   });
   ```

### 2. Sliding Window Limiter

- **Description**: Allows 20 requests over a 2-minute period but divides the time into smaller sliding windows (4 segments of 30 seconds each). This smooths out the traffic by counting requests over smaller intervals.
- **Configuration**:
   ```csharp
   options.AddSlidingWindowLimiter("sliding", opt =>
   {
       opt.Window = TimeSpan.FromMinutes(2);
       opt.PermitLimit = 20;
       opt.SegmentsPerWindow = 4;
   });
   ```

### 3. Token Bucket Limiter

- **Description**: Allows bursts of up to 15 requests but refills tokens at a rate of 1 token per second. Requests consume tokens, and if no tokens are available, they are throttled until tokens are replenished.
- **Configuration**:
   ```csharp
   options.AddTokenBucketLimiter("token", opt =>
   {
       opt.TokenLimit = 15;
       opt.TokensPerPeriod = 1;
       opt.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
   });
   ```

### 4. Concurrency Limiter

- **Description**: Limits concurrent requests to a maximum of 5. If more requests are made while 5 are already in progress, they are queued (up to 10). Once the queue is full, further requests are rejected with a `503 Service Unavailable` status.
- **Configuration**:
   ```csharp
   options.AddConcurrencyLimiter("concurrency", opt =>
   {
       opt.PermitLimit = 5;
       opt.QueueLimit = 10;
   });
   ```

---

## API Endpoints

| Endpoint              | Limiter Type         | Description                                         |
|-----------------------|----------------------|-----------------------------------------------------|
| `/fixed`              | Fixed Window         | Limits to 20 requests per 2 minutes.                |
| `/sliding`            | Sliding Window       | Allows 20 requests per 2 minutes with smoother flow.|
| `/token`              | Token Bucket         | Allows bursts and smooth refills of tokens.         |
| `/concurrency`        | Concurrency Limiter  | Limits concurrent requests to 5 with a queue of 10. |

---

# Rate Limiter Configuration with 429 Status Code

 Modify the rate limiter in an ASP.NET Core application to return **HTTP 429 (Too Many Requests)** instead of the default **HTTP 503 (Service Unavailable)** when the rate limit is exceeded.

## Steps to Change Response Code to 429

In the rate limiting configuration, you can set the `RejectionStatusCode` property to **429** for each limiter to indicate that the request has been throttled due to too many requests.

### Updated Configuration Code:

```csharp
builder.Services.AddRateLimiter(options =>
{
    // 1. Fixed Window Limiter (20 requests per 2 minutes)
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(2);
        opt.PermitLimit = 20;
        opt.RejectionStatusCode = 429; // Change rejection status to 429
    });

    // 2. Sliding Window Limiter (20 requests per 2 minutes, 4 segments)
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(2);
        opt.PermitLimit = 20;
        opt.SegmentsPerWindow = 4;
        opt.RejectionStatusCode = 429; // Change rejection status to 429
    });

    // 3. Token Bucket Limiter (15 tokens max, refill 1 token/sec)
    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = 15;
        opt.TokensPerPeriod = 1;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
        opt.RejectionStatusCode = 429; // Change rejection status to 429
    });

    // 4. Concurrency Limiter (5 concurrent requests, 10 in queue)
    options.AddConcurrencyLimiter("concurrency", opt =>
    {
        opt.PermitLimit = 5;
        opt.QueueLimit = 10;
        opt.RejectionStatusCode = 429; // Change rejection status to 429
    });
});
```

## Notes

- Rate limiting is useful for controlling traffic, preventing abuse, and ensuring fair usage of system resources.
- Each limiter can be customized by changing the configuration values in the `AddRateLimiter` method.
- You can explore more options for rate limiting in the official [Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0).

---
