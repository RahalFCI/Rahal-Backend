namespace Gamification.Application.DTOs.CheckInChallenge
{
    public class GetCheckInChallengeDto
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public Guid CheckInId { get; set; }
        public string? ProofMediaUrl { get; set; }
        public string ValidationStatus { get; set; } = string.Empty;
    }
}
