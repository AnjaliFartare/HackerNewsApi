using HackerNews.API.Interfaces;
using HackerNews.API.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNews.Tests;

public class HackerNewsControllerTests
{
    [Fact]
    public async Task GetTopStories_ReturnsOkResult()
    {
        var mockService = new Mock<IHackerNewsService>();
        mockService.Setup(s => s.GetTopStoriesAsync()).ReturnsAsync(new List<StoryDto> {
            new StoryDto { Title = "Test", Url = "http://test.com" }
        });

        var controller = new HackerNewsController(mockService.Object);
        var result = await controller.GetStories();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var stories = Assert.IsType<List<StoryDto>>(okResult.Value);
        Assert.Single(stories);
    }
}