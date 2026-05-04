using MediatR;
using Gamification.Application.DTOs.Achievement;

namespace Gamification.Application.CQRS.Commands.Achievement
{
    public record CreateAchievementCommand(CreateAchievementDto Dto) : IRequest<string>;
    public record UpdateAchievementCommand(Guid Id, UpdateAchievementDto Dto) : IRequest<string>;
    public record DeleteAchievementCommand(Guid Id) : IRequest<string>;
}
