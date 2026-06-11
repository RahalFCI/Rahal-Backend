using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record SpendXpCommand(Guid ExplorerId, int Amount, string SourceType, Guid ReferenceId) : IRequest<ApiResponse<string>>;
}
