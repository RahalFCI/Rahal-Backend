using Gamification.Application.CQRS.Queries.Achievement;
using Gamification.Application.DTOs.Achievement;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Achievements.Queries
{
    public class GetAllAchievementsQueryHandler : IRequestHandler<GetAllAchievementsQuery, ApiResponse<PagedResult<GetAchievementDto>>>
    {
        private readonly IGamificationRepository<Domain.Entities.Achievement> _repository;
        private readonly ILogger<GetAllAchievementsQueryHandler> _logger;

        public GetAllAchievementsQueryHandler(
            IGamificationRepository<Domain.Entities.Achievement> repository,
            ILogger<GetAllAchievementsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetAchievementDto>>> Handle(GetAllAchievementsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all achievements - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Include(a => a.Badge)
                .Include(a => a.AchievementCriteriaType)
                .Select(a => AchievementMapper.ToGetDto(a))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} achievements out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetAchievementDto>>.Success(result);
        }
    }
}
