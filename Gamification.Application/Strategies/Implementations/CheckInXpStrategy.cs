using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gamification.Application.Strategies.Implementations
{
    public class CheckInXpStrategy : IXpCalculationStrategy
    {
        private readonly ILogger<CheckInXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.CheckIn;

        public CheckInXpStrategy(ILogger<CheckInXpStrategy> logger)
        {
            _logger = logger;
        }

        public Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for check-in {CheckInId}", sourceId);

            const int baseCheckInXp = 10;
            const float streakMultiplier = 1.1f;
            const int maxStreakBonus = 50;

            var streakBonus = Math.Min((int)(baseCheckInXp * streakMultiplier), maxStreakBonus);
            var totalXp = baseCheckInXp + streakBonus;

            _logger.LogInformation("Calculated check-in XP: {TotalXp}", totalXp);

            return Task.FromResult(totalXp);
        }
    }
}
