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
    public class GetExplorerAchievementsByAchievementIdQueryHandler : IRequestHandler<GetExplorerAchievementsByAchievementIdQuery, ApiResponse<PagedResult<GetExplorerAchievementDto>>>
    {
        private readonly IGamificationRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<GetExplorerAchievementsByAchievementIdQueryHandler> _logger;

        public GetExplorerAchievementsByAchievementIdQueryHandler(
            IGamificationRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<GetExplorerAchievementsByAchievementIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetExplorerAchievementDto>>> Handle(GetExplorerAchievementsByAchievementIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorer achievements for achievement {AchievementId} - page {Page}, pageSize {PageSize}", request.AchievementId, request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Where(ea => ea.AchievementId == request.AchievementId)
                .Include(ea => ea.Achievement)
                .Include(ea => ea.ExplorerProfile)
                .Select(ea => ExplorerAchievementMapper.ToGetDto(ea))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} explorer achievements for achievement {AchievementId} out of {TotalCount}", result.Items.Count(), request.AchievementId, result.TotalCount);

            return ApiResponse<PagedResult<GetExplorerAchievementDto>>.Success(result);
        }
    }
}
