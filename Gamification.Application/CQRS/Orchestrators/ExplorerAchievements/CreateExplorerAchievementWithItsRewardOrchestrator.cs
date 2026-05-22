using Gamification.Application.DTOs.ExplorerAchievement;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Orchestrators.ExplorerAchievements
{
    public record CreateExplorerAchievementWithItsRewardOrchestrator(CreateExplorerAchievementDto Dto) : IRequest<ApiResponse<string>>;
}
