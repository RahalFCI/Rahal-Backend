using Gamification.Application.DTOs.CheckInChallenge;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.CheckInChallenge
{
    public record GetCheckInChallengesByChallengeIdQuery(Guid ChallengeId) : IRequest<ApiResponse<IEnumerable<GetCheckInChallengeDto>>>;

}
