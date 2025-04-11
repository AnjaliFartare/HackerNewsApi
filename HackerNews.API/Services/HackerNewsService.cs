using HackerNews.API.Interfaces;
using HackerNews.API.Models;
using Microsoft.Extensions.Caching.Memory;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "newest_stories";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<StoryDto>> GetNewestStoriesAsync()
    {
        if (_cache.TryGetValue(CacheKey, out List<StoryDto> cachedStories))
        {
            return cachedStories;
        }

        var ids = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/newstories.json");

        var stories = new List<StoryDto>();
        foreach (var id in ids.Take(200))
        {
            var story = await _httpClient.GetFromJsonAsync<StoryDto>($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
            if (!string.IsNullOrWhiteSpace(story?.Url))
                stories.Add(story);

        }
        _cache.Set(CacheKey, stories, TimeSpan.FromMinutes(10));
        return stories;
    }
}
