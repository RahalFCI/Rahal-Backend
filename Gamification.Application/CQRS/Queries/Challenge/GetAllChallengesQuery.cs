using Gamification.Application.DTOs.Challenge;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Challenge
{
    public record GetAllChallengesQuery : IRequest<IEnumerable<GetChallengeDto>>;

}
