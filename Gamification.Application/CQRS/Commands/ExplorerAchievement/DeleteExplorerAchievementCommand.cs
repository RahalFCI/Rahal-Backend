using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.ExplorerAchievement
{
    public record DeleteExplorerAchievementCommand(Guid Id) : IRequest<ApiResponse<string>>;
}
