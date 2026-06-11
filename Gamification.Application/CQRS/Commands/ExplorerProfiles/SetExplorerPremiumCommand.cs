using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.ExplorerProfiles
{
    public record SetExplorerPremiumCommand(Guid ExplorerId, bool IsPremium, Guid? PlanTierId) : IRequest<ApiResponse<string>>;
}
