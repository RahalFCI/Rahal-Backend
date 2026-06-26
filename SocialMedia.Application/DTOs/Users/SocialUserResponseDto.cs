namespace SocialMedia.Application.DTOs.Users
{
    public class SocialUserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}
