using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;

namespace HackerNews.Tests;

public class HackerNewsServiceTests
{
    private HackerNewsService CreateServiceWithMockHandler(Mock<HttpMessageHandler> handler, IMemoryCache memoryCache)
    {
        var mockLogger = new Mock<ILogger<HackerNewsService>>();
        var mockFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
        };
        mockFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return new HackerNewsService(memoryCache, mockFactory.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsMockedData()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("topstories")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[1,2]"),
            });

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("item/1")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": \"Story 1\", \"url\": \"http://story1.com\"}"),
            });

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("item/2")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": \"Story 2\", \"url\": \"http://story2.com\"}"),
            });

        var service = CreateServiceWithMockHandler(handler, memoryCache);

        var stories = await service.GetTopStoriesAsync();

        Assert.NotNull(stories);
        Assert.Equal(2, stories.Count);
    }

    [Fact]
    public async Task GetTopStoriesAsync_UsesCacheOnSecondCall()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var mockFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<HackerNewsService>>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        int sendAsyncCallCount = 0;

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback(() => sendAsyncCallCount++) 
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri.ToString().Contains("topstories"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("[1]"),
                    };
                }

                if (request.RequestUri.ToString().Contains("item/1"))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"title\": \"Story 1\", \"url\": \"http://story1.com\"}"),
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
        };

        mockFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new HackerNewsService(memoryCache, mockFactory.Object, mockLogger.Object);

        // Act - First call (should fetch from API and populate cache)
        var firstCall = await service.GetTopStoriesAsync();
        // Act - Second call (should use cached data)
        var secondCall = await service.GetTopStoriesAsync();

        // Assert
        Assert.Equal(1, sendAsyncCallCount / 2); 
        Assert.Single(firstCall);
        Assert.Single(secondCall);
        Assert.Equal("Story 1", secondCall[0].Title);
    }


    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmpty_WhenTopStoriesIsEmpty()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("topstories")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]"),
            });

        var service = CreateServiceWithMockHandler(handler, memoryCache);

        var result = await service.GetTopStoriesAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTopStoriesAsync_ReturnsEmpty_WhenApiFails()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("API failed"));

        var service = CreateServiceWithMockHandler(handler, memoryCache);

        var result = await service.GetTopStoriesAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTopStoriesAsync_SkipsStoriesWithNullTitle()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("topstories")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[1,2]"),
            });

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("item/1")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": null, \"url\": \"http://story1.com\"}"),
            });

        handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.ToString().Contains("item/2")),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": \"Story 2\", \"url\": \"http://story2.com\"}"),
            });

        var service = CreateServiceWithMockHandler(handler, memoryCache);

        var result = await service.GetTopStoriesAsync();

        Assert.Single(result);
        Assert.Equal("Story 2", result[0].Title);
    }
}
