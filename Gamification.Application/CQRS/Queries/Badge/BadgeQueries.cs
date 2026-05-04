using MediatR;
using Gamification.Application.DTOs.Badge;

namespace Gamification.Application.CQRS.Queries.Badges
{
    public record GetBadgeByIdQuery(Guid Id) : IRequest<GetBadgeDto?>;
    public record GetAllBadgesQuery : IRequest<IEnumerable<GetBadgeDto>>;
}
