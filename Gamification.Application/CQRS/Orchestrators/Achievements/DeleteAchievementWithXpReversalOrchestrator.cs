using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.Achievements
{
    public record DeleteAchievementWithXpReversalOrchestrator(Guid AchievementId) : IRequest<ApiResponse<string>>;
}
