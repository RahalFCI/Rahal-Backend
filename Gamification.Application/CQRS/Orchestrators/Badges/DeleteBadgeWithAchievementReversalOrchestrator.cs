using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Orchestrators.Badges
{
    public record DeleteBadgeWithAchievementReversalOrchestrator(Guid BadgeId) : IRequest<ApiResponse<string>>;
}
