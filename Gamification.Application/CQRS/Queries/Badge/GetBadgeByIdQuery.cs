using Gamification.Application.DTOs.Badge;
using MediatR;
using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Badge
{
    public record GetBadgeByIdQuery(Guid Id) : IRequest<ApiResponse<GetBadgeDto>>;

}
