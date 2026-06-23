namespace SocialMedia.Application.DTOs.Posts
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsPublic { get; set; }

        /// <summary>Full HTTPS Cloudinary URLs, ready for the client to use directly.</summary>
        public List<string> MediaUrls { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
