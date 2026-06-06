using Gamification.Application.CQRS.Queries.ExplorerAchievement;
using Gamification.Application.DTOs.ExplorerAchievement;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Queries
{
    public class GetExplorerAchievementByIdQueryHandler : IRequestHandler<GetExplorerAchievementByIdQuery, ApiResponse<GetExplorerAchievementDto>>
    {
        private readonly IGamificationRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<GetExplorerAchievementByIdQueryHandler> _logger;

        public GetExplorerAchievementByIdQueryHandler(
            IGamificationRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<GetExplorerAchievementByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<GetExplorerAchievementDto>> Handle(GetExplorerAchievementByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching explorer achievement {ExplorerAchievementId}", request.Id);

            var explorerAchievement = await _repository.GetTable()
                .Include(ea => ea.Achievement)
                .FirstOrDefaultAsync(ea => ea.Id == request.Id, cancellationToken);

            if (explorerAchievement is null)
            {
                _logger.LogWarning("Explorer achievement {ExplorerAchievementId} not found", request.Id);
                return ApiResponse<GetExplorerAchievementDto>.Failure(ErrorCode.NotFound);
            }

            var dto = ExplorerAchievementMapper.ToGetDto(explorerAchievement);
            return ApiResponse<GetExplorerAchievementDto>.Success(dto);
        }
    }
}
