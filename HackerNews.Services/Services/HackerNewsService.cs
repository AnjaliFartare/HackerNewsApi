using HackerNews.API.Interfaces;
using HackerNews.API.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

public class HackerNewsService : IHackerNewsService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HackerNewsService> _logger;

    public HackerNewsService(IMemoryCache cache, IHttpClientFactory httpClientFactory, ILogger<HackerNewsService> logger)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the top 200 stories from Hacker News and caches the result.
    /// </summary>
    /// <returns>
    /// A <see cref="List{T}"/> of <see cref="StoryDto"/> containing the title and URL of each top story.
    /// Returns an empty list if the request fails or no stories are found.
    /// </returns>
    /// <remarks>
    /// Uses an in-memory cache to avoid repeated API calls.
    /// In case of an exception, logs the error and returns an empty list.
    /// </remarks>
    public async Task<List<StoryDto>> GetTopStoriesAsync()
    {
        return await _cache.GetOrCreateAsync("top_stories", async entry =>
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var idsResponse = await client.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/topstories.json");

                if (idsResponse == null || !idsResponse.Any())
                    return new List<StoryDto>();

                var topIds = idsResponse.Take(200).ToList();

                var tasks = topIds.Select(async id =>
                {
                    var story = await client.GetFromJsonAsync<StoryDto>($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                    return new StoryDto { Title = story?.Title ?? "", Url = story?.Url ?? "" };
                });

                var stories = await Task.WhenAll(tasks);

                return stories.Where(s => !string.IsNullOrEmpty(s.Title)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch top stories");
                return new List<StoryDto>();
            }
        }) ?? [];
    }

}
