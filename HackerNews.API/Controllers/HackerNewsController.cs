using HackerNews.API.Interfaces;
using HackerNews.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HackerNewsController : ControllerBase
{
    private readonly IHackerNewsService _service;
    private readonly ILogger<HackerNewsController> _logger;

    public HackerNewsController(IHackerNewsService service, ILogger<HackerNewsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get top 200 Hacker News stories.
    /// </summary>
    /// <param name="search">Search title or blank</param>
    /// <param name="page">Current page number</param>
    /// <param name="pageSize">Number of stories per page</param>
    /// <returns>List of stories</returns>
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStories(string? search = "", int page = 1, int pageSize = 200)
    {
        try
        {
            if (pageSize <= 0)
            {
                return BadRequest(new { message = "Page size must be greater than zero." });
            }

            _logger.LogInformation("GetStories API called at {Time}", DateTime.UtcNow);
            var stories = await _service.GetTopStoriesAsync();

            if (!string.IsNullOrEmpty(search))
            {
                stories = stories
                    .Where(s => s.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var pagedStories = stories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (!pagedStories.Any())
            {
                return Ok(new StoryListViewModel
                {
                    Stories = [],
                    TotalPages = 0
                });
            }

            return Ok(new StoryListViewModel { Stories = pagedStories, TotalPages = stories.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching stories.");
            return StatusCode(500, "Internal server error");
        }
    }
}
