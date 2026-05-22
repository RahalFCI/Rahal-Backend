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

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Queries
{
    public class GetAllExplorerAchievementsQueryHandler : IRequestHandler<GetAllExplorerAchievementsQuery, ApiResponse<PagedResult<GetExplorerAchievementDto>>>
    {
        private readonly IGenericRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<GetAllExplorerAchievementsQueryHandler> _logger;

        public GetAllExplorerAchievementsQueryHandler(
            IGenericRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<GetAllExplorerAchievementsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<GetExplorerAchievementDto>>> Handle(GetAllExplorerAchievementsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all explorer achievements - page {Page}, pageSize {PageSize}", request.PaginationRequest.Page, request.PaginationRequest.PageSize);

            var result = await _repository.GetTable()
                .Include(ea => ea.Achievement)
                .Select(ea => ExplorerAchievementMapper.ToGetDto(ea))
                .ToPagedResultAsync(request.PaginationRequest, cancellationToken);

            _logger.LogInformation("Retrieved {Count} explorer achievements out of {TotalCount}", result.Items.Count(), result.TotalCount);

            return ApiResponse<PagedResult<GetExplorerAchievementDto>>.Success(result);
        }
    }
}
