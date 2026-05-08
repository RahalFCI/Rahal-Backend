using Gamification.Application.DTOs.Badge;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Badge
{
    public record GetAllBadgesQuery : IRequest<ApiResponse<IEnumerable<GetBadgeDto>>>;

}
