namespace SocialMedia.Domain.Entities
{
    /// <summary>
    /// Junction entity representing a user liking a post.
    /// Uses a composite primary key (UserId, PostId) — does NOT inherit BaseEntity.
    /// UserId references users.AspNetUsers (cross-module — no EF navigation).
    /// </summary>
    public class Like
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public Post? Post { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
