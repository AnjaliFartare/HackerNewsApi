using HackerNews.API.Models;

namespace HackerNews.API.Interfaces
{
    public interface IHackerNewsService
    {
        Task<List<StoryDto>> GetNewestStoriesAsync();
    }
}
