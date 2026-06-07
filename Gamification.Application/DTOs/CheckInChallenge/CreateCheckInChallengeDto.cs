namespace Gamification.Application.DTOs.CheckInChallenge
{
    public class CreateCheckInChallengeDto
    {
        public Guid ChallengeId { get; set; }
        public Guid CheckInId { get; set; }
        public string? ProofMediaUrl { get; set; }
    }
}
