using Gamification.Application.DTOs.Challenge;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Queries.Challenge
{
    public record GetAllChallengesQuery(OffsetPaginationRequest PaginationRequest) : IRequest<ApiResponse<PagedResult<GetChallengeDto>>>;

}
