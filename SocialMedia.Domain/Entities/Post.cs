using Shared.Domain.Entities;

namespace SocialMedia.Domain.Entities
{
    public class Post : BaseEntity
    {
        /// <summary>
        /// References users.AspNetUsers — stored as plain Guid (cross-module, no EF navigation).
        /// </summary>
        public Guid UserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Ordered list of media URLs (images/videos). Stored as JSONB in PostgreSQL.
        /// </summary>
        public List<string> MediaUrls { get; set; } = new();

        // Navigation properties (within SocialMedia schema)
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<PostPlace> PostPlaces { get; set; } = new List<PostPlace>();
    }
}
