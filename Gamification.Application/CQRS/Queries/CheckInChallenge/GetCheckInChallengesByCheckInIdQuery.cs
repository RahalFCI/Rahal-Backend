using Gamification.Application.DTOs.CheckInChallenge;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.CheckInChallenge
{
    public record GetCheckInChallengesByCheckInIdQuery(Guid CheckInId) : IRequest<ApiResponse<IEnumerable<GetCheckInChallengeDto>>>;

}
