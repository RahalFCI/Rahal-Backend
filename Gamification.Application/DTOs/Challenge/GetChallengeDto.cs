namespace Gamification.Application.DTOs.Challenge
{
    public class GetChallengeDto
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ValidationPrompt { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int MinimumLevelRequired { get; set; }
        public int XpReward { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
