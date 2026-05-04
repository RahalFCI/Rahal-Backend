using MediatR;
using Gamification.Application.DTOs.Badge;

namespace Gamification.Application.CQRS.Commands.Badges
{
    public record CreateBadgeCommand(CreateBadgeDto Dto) : IRequest<string>;
    public record UpdateBadgeCommand(Guid Id, UpdateBadgeDto Dto) : IRequest<string>;
    public record DeleteBadgeCommand(Guid Id) : IRequest<string>;
}
