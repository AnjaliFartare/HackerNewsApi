using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;

namespace HackerNews.Tests;

public class HackerNewsServiceTests
{
    [Fact]
    public async Task GetTopStoriesAsync_ReturnsMockedData()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var mockFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<HackerNewsService>>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString().Contains("topstories")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[1,2]"),
            });

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString().Contains("item/1")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": \"Story 1\", \"url\": \"http://story1.com\"}"),
            });

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.ToString().Contains("item/2")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"title\": \"Story 2\", \"url\": \"http://story2.com\"}"),
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
        };

        mockFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new HackerNewsService(memoryCache, mockFactory.Object, mockLogger.Object);

        var stories = await service.GetTopStoriesAsync();

        Assert.NotNull(stories);
        Assert.Equal(2, stories.Count());
        Assert.Contains(stories, s => s.Title == "Story 1");
    }

}