namespace SocialMedia.Domain.Entities
{
    /// <summary>
    /// Junction entity representing a follow relationship between two users.
    /// Uses a composite primary key (FollowerId, FolloweeId) — does NOT inherit BaseEntity.
    /// Both user IDs reference users.AspNetUsers (cross-module — no EF navigation).
    /// </summary>
    public class Follow
    {
        public Guid FollowerId { get; set; }
        public Guid FolloweeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
