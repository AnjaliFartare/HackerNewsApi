using HackerNews.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HackerNewsController : ControllerBase
{
    private readonly IHackerNewsService _service;

    public HackerNewsController(IHackerNewsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get top 200 Hacker News stories.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStories([FromQuery] string search = "", int page = 1, int pageSize = 20)
    {
        var stories = await _service.GetTopStoriesAsync();

        if (!string.IsNullOrEmpty(search))
        {
            stories = stories.Where(s => s.Title != null && s.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var paged = stories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(paged);
    }
}
