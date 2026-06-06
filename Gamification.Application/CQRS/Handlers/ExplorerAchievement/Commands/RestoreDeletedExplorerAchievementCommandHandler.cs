using Gamification.Application.CQRS.Commands.ExplorerAchievement;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Commands
{
    public class RestoreDeletedExplorerAchievementCommandHandler : IRequestHandler<RestoreDeletedExplorerAchievementCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<RestoreDeletedExplorerAchievementCommandHandler> _logger;

        public RestoreDeletedExplorerAchievementCommandHandler(
            IGamificationRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<RestoreDeletedExplorerAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedExplorerAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted explorer achievement {ExplorerAchievementId}", request.Id);

            var explorerAchievementExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(ea => ea.Id == request.Id && ea.IsDeleted, cancellationToken);

            if (!explorerAchievementExists)
            {
                _logger.LogWarning("Deleted explorer achievement {ExplorerAchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Domain.Entities.ExplorerAchievement explorerAchievement = new Domain.Entities.ExplorerAchievement()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(explorerAchievement, nameof(explorerAchievement.IsDeleted), nameof(explorerAchievement.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Explorer achievement {ExplorerAchievementId} restored successfully", request.Id);

            return ApiResponse<string>.Success("Explorer achievement restored successfully");
        }
    }
}
