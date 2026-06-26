namespace SocialMedia.Application.DTOs.Posts
{
    public class PostResponseDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid AuthorId { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public List<string> MediaUrls { get; set; } = new();
    }
}
