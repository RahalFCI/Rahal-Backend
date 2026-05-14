using Gamification.Application.DTOs.Explorer;
using Gamification.Domain.Entities;
using MediatR;
using Shared.Application.DTOs;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetExplorerProfileByUserIdQuery(Guid UserId) : IRequest<ApiResponse<GetExplorerDto>>;
}
