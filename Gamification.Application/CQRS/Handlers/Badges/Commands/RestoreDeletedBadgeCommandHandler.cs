using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class RestoreDeletedBadgeCommandHandler : IRequestHandler<RestoreDeletedBadgeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly ILogger<RestoreDeletedBadgeCommandHandler> _logger;

        public RestoreDeletedBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            ILogger<RestoreDeletedBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Restoring deleted badge {BadgeId}", request.Id);

            var badgeExists = await _repository.GetTable()
                .IgnoreQueryFilters()
                .AnyAsync(b => b.Id == request.Id && b.IsDeleted, cancellationToken);

            if (!badgeExists)
            {
                _logger.LogWarning("Deleted badge {BadgeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            Badge badge = new Badge()
            {
                Id = request.Id,
                IsDeleted = false,
                DeletedAt = null
            };

            _repository.SaveInclude(badge, nameof(badge.IsDeleted), nameof(badge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Badge {BadgeId} restored successfully", request.Id);

            return ApiResponse<string>.Success("Badge restored successfully");
        }
    }
}
