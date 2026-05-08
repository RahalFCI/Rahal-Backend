using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Shared.Application.Interfaces;
using Gamification.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Gamification.Application.Strategies.Implementations
{
    public class ChallengeXpStrategy : IXpCalculationStrategy
    {
        private readonly IGenericRepository<Domain.Entities.Challenge> _challengeRepository;
        private readonly ILogger<ChallengeXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.Challenge;

        public ChallengeXpStrategy(
            IGenericRepository<Domain.Entities.Challenge> challengeRepository,
            ILogger<ChallengeXpStrategy> logger)
        {
            _challengeRepository = challengeRepository;
            _logger = logger;
        }

        public async Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for challenge {ChallengeId}", sourceId);

            var challenge = await _challengeRepository.GetByIdAsync(sourceId, cancellationToken);

            if (challenge is null)
            {
                _logger.LogWarning("Challenge {ChallengeId} not found", sourceId);
                return 0;
            }

            var baseXp = challenge.XpReward;
            var difficultyBonus = GetDifficultyBonus(challenge.Difficulty);
            var firstTimeCompletionBonus = 5;

            var totalXp = baseXp + difficultyBonus + firstTimeCompletionBonus;

            _logger.LogInformation("Calculated challenge XP: {TotalXp} (Base: {BaseXp}, Difficulty: {Bonus}, FirstTime: {FirstTime})", 
                totalXp, baseXp, difficultyBonus, firstTimeCompletionBonus);

            return totalXp;
        }

        private static int GetDifficultyBonus(ChallengeDifficulty difficulty)
        {
            return difficulty switch
            {
                ChallengeDifficulty.Easy => 0,
                ChallengeDifficulty.Medium => 10,
                ChallengeDifficulty.Hard => 20,
                _ => 0
            };
        }
    }
}
