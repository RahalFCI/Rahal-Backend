using Gamification.Domain.Enums;

namespace Gamification.Application.Strategies
{
    public interface IXpCalculationStrategy
    {
        XpSourceType SourceType { get; }
        Task<int> CalculateXpAsync(Guid sourceId, CancellationToken cancellationToken = default);
    }
}
