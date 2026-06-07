using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Shared.Application.Interfaces;
using Gamification.Domain.Entities;
using Microsoft.Extensions.Logging;
using Gamification.Application.Interfaces;

namespace Gamification.Application.Strategies.Implementations
{
    public class AchievementXpStrategy : IXpCalculationStrategy
    {
        private readonly IGamificationRepository<Achievement> _achievementRepository;
        private readonly ILogger<AchievementXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.Achievement;

        public AchievementXpStrategy(
            IGamificationRepository<Achievement> achievementRepository,
            ILogger<AchievementXpStrategy> logger)
        {
            _achievementRepository = achievementRepository;
            _logger = logger;
        }

        public async Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for achievement {AchievementId}", sourceId);

            var achievement = await _achievementRepository.GetByIdAsync(sourceId, cancellationToken);

            if (achievement is null)
            {
                _logger.LogWarning("Achievement {AchievementId} not found", sourceId);
                return 0;
            }

            var baseXp = achievement.Xp;

            _logger.LogInformation("Calculated achievement XP: {TotalXp}", baseXp);

            return baseXp;
        }
    }
}
