using Gamification.Application.DTOs.Achievement;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Achievement
{
    public record GetAchievementByIdQuery(Guid Id) : IRequest<ApiResponse<GetAchievementDto>>;

}
