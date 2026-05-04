using MediatR;
using Gamification.Application.DTOs.Achievement;

namespace Gamification.Application.CQRS.Queries.Achievement
{
    public record GetAchievementByIdQuery(Guid Id) : IRequest<GetAchievementDto?>;
    public record GetAllAchievementsQuery : IRequest<IEnumerable<GetAchievementDto>>;
    public record GetAchievementsByBadgeIdQuery(Guid BadgeId) : IRequest<IEnumerable<GetAchievementDto>>;
}
