using Gamification.Application.DTOs.Achievement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Achievement
{
    public record GetAchievementByIdQuery(Guid Id) : IRequest<GetAchievementDto?>;

}
