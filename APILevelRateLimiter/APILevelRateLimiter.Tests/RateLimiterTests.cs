using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace APILevelRateLimiter.Tests
{
    public class RateLimiterTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RateLimiterTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/fixed")]
        [InlineData("/sliding")]
        [InlineData("/token")]
        [InlineData("/concurrency")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        [Theory]
        [InlineData("/fixed", "Fixed window response")]
        [InlineData("/sliding", "Sliding window response")]
        [InlineData("/token", "Token bucket response")]
        [InlineData("/concurrency", "Concurrency response")]
        public async Task Get_EndpointsReturnExpectedContent(string url, string expectedContent)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedContent, content);
        }
    }
}
