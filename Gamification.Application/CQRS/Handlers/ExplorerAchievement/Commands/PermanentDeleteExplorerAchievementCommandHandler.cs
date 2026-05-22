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
    public class PermanentDeleteExplorerAchievementCommandHandler : IRequestHandler<PermanentDeleteExplorerAchievementCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.ExplorerAchievement> _repository;
        private readonly ILogger<PermanentDeleteExplorerAchievementCommandHandler> _logger;

        public PermanentDeleteExplorerAchievementCommandHandler(
            IGenericRepository<Domain.Entities.ExplorerAchievement> repository,
            ILogger<PermanentDeleteExplorerAchievementCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermanentDeleteExplorerAchievementCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Permanently deleting explorer achievement {ExplorerAchievementId}", request.Id);

            var explorerAchievement = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (explorerAchievement is null)
            {
                _logger.LogWarning("Explorer achievement {ExplorerAchievementId} not found for permanent deletion", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(explorerAchievement);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Explorer achievement {ExplorerAchievementId} permanently deleted", request.Id);

            return ApiResponse<string>.Success("Explorer achievement permanently deleted");
        }
    }
}
