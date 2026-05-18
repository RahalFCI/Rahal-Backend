using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStats
{
    public record DecrementChallengeStatsCommand(Guid ExplorerId) : IRequest<ApiResponse<string>>;
}
