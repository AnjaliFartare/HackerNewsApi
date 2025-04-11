using HackerNews.API.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

public class HackerNewsServiceTests
{
    [Fact]
    public async Task GetNewestStoriesAsync_ReturnsStories_FromApiAndThenFromCache()
    {
        // Arrange
        var expectedStory = new StoryDto { Id = 1, Title = "Test Story", Url = "https://example.com" };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new List<int> { 1 })
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedStory)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
        };

        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var service = new HackerNewsService(httpClient, memoryCache);

        // Act (First call - from API)
        var result1 = await service.GetNewestStoriesAsync();

        // Act (Second call - from cache)
        var result2 = await service.GetNewestStoriesAsync();

        // Assert
        Assert.Single(result1);
        Assert.Equal(expectedStory.Title, result1[0].Title);
        Assert.Same(result1, result2);
    }
}