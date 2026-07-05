namespace SocialMedia.Application.DTOs.Posts
{
    public class FeedPagedResponse
    {
        public List<PostResponseDto> Posts { get; set; } = new();
        public long? NextCursor { get; set; }
    }
}
