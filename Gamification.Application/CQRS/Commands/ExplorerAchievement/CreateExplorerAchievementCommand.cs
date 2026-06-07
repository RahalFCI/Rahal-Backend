using Gamification.Application.DTOs.ExplorerAchievement;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.ExplorerAchievement
{
    public record CreateExplorerAchievementCommand(CreateExplorerAchievementDto Dto) : IRequest<ApiResponse<GetExplorerAchievementDto>>;
}
