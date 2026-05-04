using MediatR;
using Gamification.Application.DTOs.Challenge;

namespace Gamification.Application.CQRS.Queries.Challenges
{
    public record GetChallengeByIdQuery(Guid Id) : IRequest<GetChallengeDto?>;
    public record GetAllChallengesQuery : IRequest<IEnumerable<GetChallengeDto>>;
    public record GetChallengesByPlaceIdQuery(Guid PlaceId) : IRequest<IEnumerable<GetChallengeDto>>;
}
