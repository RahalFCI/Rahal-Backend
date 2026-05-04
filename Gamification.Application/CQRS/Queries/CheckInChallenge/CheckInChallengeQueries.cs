using MediatR;
using Gamification.Application.DTOs.CheckInChallenge;

namespace Gamification.Application.CQRS.Queries.CheckInChallenges
{
    public record GetCheckInChallengeByIdQuery(Guid Id) : IRequest<GetCheckInChallengeDto?>;
    public record GetCheckInChallengesByCheckInIdQuery(Guid CheckInId) : IRequest<IEnumerable<GetCheckInChallengeDto>>;
    public record GetCheckInChallengesByChallengeIdQuery(Guid ChallengeId) : IRequest<IEnumerable<GetCheckInChallengeDto>>;
}
