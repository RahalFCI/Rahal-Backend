using Gamification.Application.DTOs.CheckInChallenge;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.CheckInChallenge
{
    public record GetCheckInChallengesByCheckInIdQuery(Guid CheckInId, OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetCheckInChallengeDto>>>;

}
