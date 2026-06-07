using Gamification.Application.Strategies;
using Gamification.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gamification.Application.Strategies
{
    public class XpCalculationStrategyResolver
    {
        private readonly IEnumerable<IXpCalculationStrategy> _strategies;
        private readonly ILogger<XpCalculationStrategyResolver> _logger;

        public XpCalculationStrategyResolver(
            IEnumerable<IXpCalculationStrategy> strategies,
            ILogger<XpCalculationStrategyResolver> logger)
        {
            _strategies = strategies;
            _logger = logger;
        }

        public IXpCalculationStrategy ResolveStrategy(XpSourceType sourceType)
        {
            _logger.LogInformation("Resolving XP calculation strategy for source type {SourceType}", sourceType);

            var strategy = _strategies.FirstOrDefault(s => s.SourceType == sourceType);

            if (strategy is null)
            {
                _logger.LogWarning("No strategy found for source type {SourceType}. Returning default strategy.", sourceType);
                return _strategies.First();
            }

            _logger.LogInformation("Strategy resolved: {StrategyType}", strategy.GetType().Name);

            return strategy;
        }
    }
}
