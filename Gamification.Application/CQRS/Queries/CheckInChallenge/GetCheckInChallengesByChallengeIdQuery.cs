using Gamification.Application.DTOs.CheckInChallenge;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.CheckInChallenge
{
    public record GetCheckInChallengesByChallengeIdQuery(Guid ChallengeId) : IRequest<IEnumerable<GetCheckInChallengeDto>>;

}
