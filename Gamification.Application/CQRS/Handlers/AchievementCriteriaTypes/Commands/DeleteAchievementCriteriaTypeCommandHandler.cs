using Gamification.Application.CQRS.Commands.AchievementCriteriaTypes;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.AchievementCriteriaTypes.Commands
{
    public class DeleteAchievementCriteriaTypeCommandHandler : IRequestHandler<DeleteAchievementCriteriaTypeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<AchievementCriteriaType> _repository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<DeleteAchievementCriteriaTypeCommandHandler> _logger;

        public DeleteAchievementCriteriaTypeCommandHandler(
            IGenericRepository<AchievementCriteriaType> repository,
            ICacheService cacheService,
            ILogger<DeleteAchievementCriteriaTypeCommandHandler> logger)
        {
            _repository = repository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteAchievementCriteriaTypeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleteing achievement criteria type {AchievementCriteriaTypeId}", request.Id);

            var achievementCriteriaType = await _repository.GetTable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == request.Id && a.IsDeleted, cancellationToken);

            if (achievementCriteriaType is null)
            {
                _logger.LogWarning("Achievement criteria type {AchievementCriteriaTypeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            _repository.Delete(achievementCriteriaType);
            await _repository.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync("achievement-criteria-types:all");

            _logger.LogInformation("Achievement criteria type {AchievementCriteriaTypeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Achievement criteria type deleted successfully");
        }
    }
}
