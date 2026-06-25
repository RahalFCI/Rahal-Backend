namespace SocialMedia.Application.DTOs.Follows
{
    public class FollowResponse
    {
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
