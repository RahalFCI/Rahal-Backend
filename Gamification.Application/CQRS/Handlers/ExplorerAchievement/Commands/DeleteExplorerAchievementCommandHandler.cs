using Gamification.Application.CQRS.Commands.ExplorerAchievement;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.ExplorerAchievement.Commands
{
    public class DeleteExplorerAchievementCommandHandler : IRequestHandler<DeleteExplorerAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<DeleteExplorerAchievementCommandHandler> _logger;

        public DeleteExplorerAchievementCommandHandler(
            IGenericRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<DeleteExplorerAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteExplorerAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting explorer achievement {ExplorerAchievementId}", request.Id);

            var explorerAchievementExists = await _repository.GetTable()
                .AnyAsync(ea => ea.Id == request.Id, cancellationToken);

            if (!explorerAchievementExists)
            {
                _logger.LogWarning("Explorer achievement {ExplorerAchievementId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Domain.Entities.ExplorerAchievement explorerAchievement = new Domain.Entities.ExplorerAchievement()
            {
                Id = request.Id,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            _repository.SaveInclude(explorerAchievement, nameof(explorerAchievement.IsDeleted), nameof(explorerAchievement.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Explorer achievement {ExplorerAchievementId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Explorer achievement deleted successfully");
        }
    }
}
