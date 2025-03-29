
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



# CI/CD Pipeline for APILevelRateLimiter

This document outlines the steps to set up a CI/CD pipeline for the APILevelRateLimiter project using GitHub Actions. The pipeline includes building, testing, and publishing a Docker image to Docker Hub.

## Prerequisites

1. **GitHub Repository**: Ensure you have a GitHub repository for your project.
2. **Docker Hub Account**: Ensure you have a Docker Hub account and a repository named `mariselvam93/myrepo`.

## Adding Secrets to GitHub

1. **Navigate to Your Repository**:  
   - Go to your GitHub repository.
2. **Go to Settings**:  
   - Click on the **Settings** tab.
3. **Access Secrets**:  
   - In the left sidebar, click on **Secrets** and then **Actions**.
4. **Add New Repository Secret**:  
   - Click on the **New repository secret** button.
5. **Add the Required Secrets**:  
   - `DOCKER_USERNAME`: Your Docker Hub username.  
   - `DOCKER_PASSWORD`: Your Docker Hub password.

## GitHub Actions Workflow

Create a file named `.github/workflows/ci-cd.yaml` in your repository with the following content:

```yaml
name: Build, Test, and Publish Docker Image for APILevelRateLimiter

on:
  push:
    branches:
      - main
      - 'refs/tags/*' # Trigger on any tag push
  pull_request:
    branches:
      - main # Trigger on pull requests to the main branch

jobs:
  build:
    name: Build, Test, and Publish Docker Image
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Code
        uses: actions/checkout@v3 # Check out the repository code

      - name: Set up .NET 8
        uses: actions/setup-dotnet@v3 # Set up .NET 8
        with:
          dotnet-version: '8.0.x'

      - name: Restore Dependencies
        run: dotnet restore APILevelRateLimiter/APILevelRateLimiter/APILevelRateLimiter.csproj # Restore project dependencies

      - name: Build Application
        run: dotnet build --no-restore --configuration Release APILevelRateLimiter/APILevelRateLimiter/APILevelRateLimiter.csproj # Build the application

      - name: Get Git Commit Hash
        id: vars
        run: echo "GIT_COMMIT=$(git rev-parse --short HEAD)" >> $GITHUB_ENV # Get the short commit hash and store it in the environment variable

      - name: Build Docker Image
        run: docker build -t mariselvam93/myrepo:${{ github.sha }} -t mariselvam93/myrepo:latest -f APILevelRateLimiter/APILevelRateLimiter/Dockerfile . # Build the Docker image with commit hash and latest tags

  push:
    name: Push Docker Image
    needs: build
    runs-on: ubuntu-latest

    steps:
      - name: Enable Debug Logging
        run: echo "GITHUB_ACTIONS=true" >> $GITHUB_ENV

      - name: Checkout Code
        uses: actions/checkout@v3 # Check out the repository code

      - name: Log in to Docker Hub
        run: echo "${{ secrets.DOCKER_PASSWORD }}" | docker login -u "${{ secrets.DOCKER_USERNAME }}" --password-stdin # Log in to Docker Hub using secrets

      - name: Build Docker Image
        run: docker build -t mariselvam93/myrepo:${{ github.sha }} -t mariselvam93/myrepo:latest -f APILevelRateLimiter/APILevelRateLimiter/Dockerfile . # Build the Docker image again

      - name: Push Docker Image
        run: |
          docker push mariselvam93/myrepo:${{ github.sha }} # Push the Docker image with the commit hash tag
          docker push mariselvam93/myrepo:latest # Push the Docker image with the latest tag
```

## Explanation

1. **Trigger**:
   - The workflow is triggered on pushes to the `main` branch and any tags (`refs/tags/*`), as well as pull requests to the `main` branch.

2. **Build Job**:
   - **Checkout Code**: Uses the `actions/checkout@v3` action to check out the repository code.
   - **Set up .NET 8**: Uses the `actions/setup-dotnet@v3` action to set up .NET 8.
   - **Restore Dependencies**: Runs `dotnet restore` to restore the project dependencies.
   - **Build Application**: Runs `dotnet build` to build the application.
   - **Get Git Commit Hash**: Retrieves the short commit hash and stores it in the environment variable `GIT_COMMIT`.
   - **Build Docker Image**: Builds the Docker image with two tags: one using the commit hash (`${{ github.sha }}`) and one using `latest`.

3. **Push Job**:
   - **Enable Debug Logging**: Enables debug logging for more detailed output.
   - **Checkout Code**: Uses the `actions/checkout@v3` action to check out the repository code.
   - **Log in to Docker Hub**: Logs in to Docker Hub using the credentials stored in GitHub Secrets (`DOCKER_USERNAME` and `DOCKER_PASSWORD`).
   - **Build Docker Image**: Builds the Docker image again (necessary because the build context is not shared between jobs).
   - **Push Docker Image**: Pushes the Docker image with both the commit hash tag and the `latest` tag.

## Testing Docker Hub Access

To test Docker Hub access using the command line:

1. **Log in to Docker Hub**:
   ```sh
   docker login
   ```

2. **Create a Test Image**:
   ```sh
   echo -e "FROM alpine:latest\nCMD [\"echo\", \"Hello, Docker Hub!\"]" > Dockerfile
   docker build -t mariselvam93/test-image .
   ```

3. **Push the Test Image**:
   ```sh
   docker push mariselvam93/test-image
   ```


