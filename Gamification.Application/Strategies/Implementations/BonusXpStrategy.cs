using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gamification.Application.Strategies.Implementations
{
    public class BonusXpStrategy : IXpCalculationStrategy
    {
        private readonly ILogger<BonusXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.Bonus;

        public BonusXpStrategy(ILogger<BonusXpStrategy> logger)
        {
            _logger = logger;
        }

        public Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for bonus {BonusId}", sourceId);

            const int bonusXp = 25;

            _logger.LogInformation("Calculated bonus XP: {TotalXp}", bonusXp);

            return Task.FromResult(bonusXp);
        }
    }
}
