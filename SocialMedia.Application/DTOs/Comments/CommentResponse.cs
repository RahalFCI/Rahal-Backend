namespace SocialMedia.Application.DTOs.Comments
{
    public class CommentResponse
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid? UserId { get; set; }
        public string? UserDisplayName { get; set; }
        public Guid? ParentCommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int RepliesCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CommentPagedResponse
    {
        public List<CommentResponse> Comments { get; set; } = new();
        public DateTime? NextCursor { get; set; }
    }
}
