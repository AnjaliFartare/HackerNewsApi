namespace HackerNews.API.Models
{
    public class StoryListViewModel
    {
        public List<StoryDto> Stories { get; set; } = new List<StoryDto>();
        public int TotalPages { get; set; }
    }
}
