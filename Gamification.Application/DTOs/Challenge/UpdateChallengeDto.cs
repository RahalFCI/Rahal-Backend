namespace Gamification.Application.DTOs.Challenge
{
    public class UpdateChallengeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int MinimumLevelRequired { get; set; }
        public int XpReward { get; set; }
        public bool IsActive { get; set; }
    }
}
