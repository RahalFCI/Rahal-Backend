using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Commands.UserStat
{
    public record UpdateStreakCommand(Guid ExplorerId, UserStats? UserStats = null) : IRequest<ApiResponse<string>>;
}
