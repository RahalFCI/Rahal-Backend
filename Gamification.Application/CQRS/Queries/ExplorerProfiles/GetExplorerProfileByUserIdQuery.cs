using Gamification.Domain.Entities;
using MediatR;

namespace Gamification.Application.CQRS.Queries.ExplorerProfiles
{
    public record GetExplorerProfileByUserIdQuery(Guid UserId) : IRequest<ExplorerProfile?>;
}
