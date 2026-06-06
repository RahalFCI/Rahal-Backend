using Gamification.Application.CQRS.Queries.UserStats;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.UserStat.Queries
{
    public class GetAllUserStatsQueryHandler : IRequestHandler<GetAllUserStatsQuery, ApiResponse<PagedResult<GetUserStatsDto>>>
    {
        private readonly IGamificationRepository<UserStats> _repository;
        private readonly ILogger<GetAllUserStatsQueryHandler> _logger;

        public GetAllUserStatsQueryHandler(
            IGamificationRepository<UserStats> repository,
            ILogger<GetAllUserStatsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetUserStatsDto>>> Handle(GetAllUserStatsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all user stats - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Select(s => UserStatsMapper.ToGetDto(s))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} user stats records out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetUserStatsDto>>.Success(result);
        }
    }
}
