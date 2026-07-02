using Gamification.Application.DTOs.CheckInChallenge;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class CheckInChallengeMapper
    {
        public static GetCheckInChallengeDto ToGetDto(CheckInChallenge checkInChallenge)
        {
            return ToGetDto(checkInChallenge, string.Empty);
        }

        // CheckInChallenge.ExplorerId has no EF navigation property to ExplorerProfile
        // (adding one would require a migration to add the FK constraint), so callers
        // that need the explorer's name resolve it themselves (e.g. via a join against
        // IGamificationRepository<ExplorerProfile>) and pass it in here.
        public static GetCheckInChallengeDto ToGetDto(CheckInChallenge checkInChallenge, string explorerName)
        {
            return new GetCheckInChallengeDto
            {
                Id = checkInChallenge.Id,
                ChallengeId = checkInChallenge.ChallengeId,
                ChallengeName = checkInChallenge.Challenge?.Name ?? string.Empty,
                CheckInId = checkInChallenge.CheckInId,
                ExplorerId = checkInChallenge.ExplorerId,
                ExplorerName = explorerName,
                ProofMediaUrl = checkInChallenge.ProofUrl,
                ValidationStatus = checkInChallenge.ValidationStatus.ToString()
            };
        }

        public static CheckInChallenge ToEntity(CreateCheckInChallengeDto dto)
        {
            return new CheckInChallenge
            {
                ChallengeId = dto.ChallengeId,
                CheckInId = dto.CheckInId,
                ProofUrl = dto.ProofMediaUrl ?? string.Empty,
                ValidationStatus = Gamification.Domain.Enums.ChallengeValidationStatus.Pending
            };
        }

        public static IEnumerable<GetCheckInChallengeDto> ToGetDtos(IEnumerable<CheckInChallenge> checkInChallenges)
        {
            return checkInChallenges.Select(ToGetDto);
        }
    }
}

