using HackerNews.API.Interfaces;
using HackerNews.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HackerNews.Tests
{
    public class HackerNewsControllerTests
    {
        private readonly Mock<IHackerNewsService> _mockService;
        private readonly Mock<ILogger<HackerNewsController>> _mockLogger;
        private readonly HackerNewsController _controller;

        public HackerNewsControllerTests()
        {
            _mockService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<HackerNewsController>>();
            _controller = new HackerNewsController(_mockService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetTopStories_ReturnsOkResult_WithPagedData()
        {
            // Arrange
            var stories = new List<StoryDto>
            {
                new StoryDto { Title = "Test", Url = "http://test.com" },
                new StoryDto { Title = "Test2", Url = "http://test2.com" }
            };
            _mockService.Setup(s => s.GetTopStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.GetStories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<StoryListViewModel>(okResult.Value);
            Assert.Equal(2, model.TotalPages);
            Assert.Equal(2, model.Stories.Count);
        }

        [Fact]
        public async Task GetTopStories_EmptyList_ReturnsOkWithEmptyStoryList()
        {
            // Arrange
            _mockService.Setup(s => s.GetTopStoriesAsync()).ReturnsAsync(new List<StoryDto>());

            // Act
            var result = await _controller.GetStories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<StoryListViewModel>(okResult.Value);
            Assert.Empty(model.Stories);
            Assert.Equal(0, model.TotalPages);
        }

        [Fact]
        public async Task GetTopStories_WithSearch_ReturnsFilteredPagedResults()
        {
            // Arrange
            var stories = new List<StoryDto>
            {
                new StoryDto { Title = "Test Story", Url = "http://test.com" },
                new StoryDto { Title = "Another Test", Url = "http://test2.com" },
                new StoryDto { Title = "Unrelated", Url = "http://test3.com" }
            };
            _mockService.Setup(s => s.GetTopStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.GetStories(search: "Test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<StoryListViewModel>(okResult.Value);
            Assert.Equal(2, model.Stories.Count);
            Assert.All(model.Stories, s => Assert.Contains("Test", s.Title, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetTopStories_WithNoMatchSearch_ReturnsEmptyList()
        {
            // Arrange
            var stories = new List<StoryDto>
            {
                new StoryDto { Title = "Some Title", Url = "http://test.com" }
            };
            _mockService.Setup(s => s.GetTopStoriesAsync()).ReturnsAsync(stories);

            // Act
            var result = await _controller.GetStories(search: "XYZ");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<StoryListViewModel>(okResult.Value);
            Assert.Empty(model.Stories);
            Assert.Equal(0, model.TotalPages);
        }

        [Fact]
        public async Task GetTopStories_WithException_ReturnsInternalServerError()
        {
            // Arrange
            _mockService.Setup(s => s.GetTopStoriesAsync()).ThrowsAsync(new Exception("Test error"));

            // Act
            var result = await _controller.GetStories();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Internal server error", statusCodeResult.Value);
        }

        [Fact]
        public async Task GetTopStories_WithInvalidPageSize_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetStories(pageSize: -5);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;

            var message = value?.GetType().GetProperty("message")?.GetValue(value);
            Assert.Equal("Page size must be greater than zero.", message);
        }
    }
}