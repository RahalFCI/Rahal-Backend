using Gamification.Application.CQRS.Queries.ExplorerAchievement;
using Gamification.Application.DTOs.ExplorerAchievement;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Infrastructure.Pagination;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Queries
{
    public class GetExplorerAchievementsByExplorerIdQueryHandler : IRequestHandler<GetExplorerAchievementsByExplorerIdQuery, ApiResponse<PagedResult<GetExplorerAchievementDto>>>
    {
        private readonly IGamificationRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<GetExplorerAchievementsByExplorerIdQueryHandler> _logger;

        public GetExplorerAchievementsByExplorerIdQueryHandler(
            IGamificationRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<GetExplorerAchievementsByExplorerIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetExplorerAchievementDto>>> Handle(GetExplorerAchievementsByExplorerIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorer achievements for explorer {ExplorerId} - page {Page}, pageSize {PageSize}", request.ExplorerId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(ea => ea.ExplorerId == request.ExplorerId)
                .Include(ea => ea.Achievement)
                .Select(ea => ExplorerAchievementMapper.ToGetDto(ea))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} explorer achievements for explorer {ExplorerId} out of {TotalCount}", result.Items.Count(), request.ExplorerId, result.TotalCount);

            return ApiResponse<PagedResult<GetExplorerAchievementDto>>.Success(result);
        }
    }
}
