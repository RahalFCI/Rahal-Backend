using Gamification.Application.DTOs.Challenge;
using Gamification.Domain.Entities;

namespace Gamification.Application.Mappers
{
    public static class ChallengeMapper
    {
        public static GetChallengeDto ToGetDto(Challenge challenge)
        {
            return new GetChallengeDto
            {
                Id = challenge.Id,
                PlaceId = challenge.PlaceId,
                Name = challenge.Name,
                Description = challenge.Description,
                Type = challenge.Type.ToString(),
                Difficulty = challenge.Difficulty.ToString(),
                MinimumLevelRequired = challenge.MinimumLevelRequired,
                XpReward = challenge.XpReward,
                IsActive = true,
                CreatedAt = challenge.CreatedAt,
                UpdatedAt = challenge.UpdatedAt
            };
        }

        public static Challenge ToEntity(CreateChallengeDto dto)
        {
            return new Challenge
            {
                PlaceId = dto.PlaceId,
                Name = dto.Name,
                Description = dto.Description,
                Type = Enum.Parse<Gamification.Domain.Enums.ChallengeType>(dto.Type),
                Difficulty = Enum.Parse<Gamification.Domain.Enums.ChallengeDifficulty>(dto.Difficulty),
                MinimumLevelRequired = dto.MinimumLevelRequired,
                XpReward = dto.XpReward
            };
        }

        public static void UpdateEntity(Challenge challenge, UpdateChallengeDto dto)
        {
            challenge.Name = dto.Name;
            challenge.Description = dto.Description;
            challenge.Difficulty = Enum.Parse<Gamification.Domain.Enums.ChallengeDifficulty>(dto.Difficulty);
            challenge.MinimumLevelRequired = dto.MinimumLevelRequired;
            challenge.XpReward = dto.XpReward;
        }

        public static IEnumerable<GetChallengeDto> ToGetDtos(IEnumerable<Challenge?> challenges)
        {
            return challenges.Where(c => c != null).Select(c => ToGetDto(c!));
        }
    }
}
