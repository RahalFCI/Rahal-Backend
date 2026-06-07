using Gamification.Application.DTOs.AchievementCriteriaType;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.AchievementCriteriaTypes
{
    public record CreateAchievementCriteriaTypeCommand(AddAchievementCriteriaTypeDto Dto) : IRequest<ApiResponse<GetAchievementCriteriaTypeDto>>;
}
