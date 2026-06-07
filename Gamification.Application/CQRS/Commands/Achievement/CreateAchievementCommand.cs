using Gamification.Application.DTOs.Achievement;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.Achievement
{
    public record CreateAchievementCommand(CreateAchievementDto Dto) : IRequest<ApiResponse<GetAchievementDto>>;

}
