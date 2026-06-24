using Shared.Domain.Entities;

namespace SocialMedia.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public Guid PostId { get; set; }
        public Post? Post { get; set; }

        /// <summary>
        /// References users.AspNetUsers — stored as plain Guid (cross-module, no EF navigation).
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Null for root-level comments; set to the parent Comment's Id for nested replies.
        /// </summary>
        public Guid? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Denormalized count of direct replies to this comment. Updated atomically via ExecuteUpdateAsync.
        /// </summary>
        public int RepliesCount { get; set; } = 0;

        // Navigation: child replies
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
