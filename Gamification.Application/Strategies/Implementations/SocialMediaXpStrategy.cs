using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gamification.Application.Strategies.Implementations
{
    public class SocialMediaXpStrategy : IXpCalculationStrategy
    {
        private readonly ILogger<SocialMediaXpStrategy> _logger;

        public XpSourceType SourceType => XpSourceType.SocialMediaPost;

        public SocialMediaXpStrategy(ILogger<SocialMediaXpStrategy> logger)
        {
            _logger = logger;
        }

        public Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Calculating XP for social media post {PostId}", sourceId);

            const int fixedSocialMediaXp = 5;
            const int engagementBonus = 2;

            var totalXp = fixedSocialMediaXp + engagementBonus;

            _logger.LogInformation("Calculated social media XP: {TotalXp}", totalXp);

            return Task.FromResult(totalXp);
        }
    }
}
