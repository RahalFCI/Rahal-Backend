using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Badge> _repository;
        private readonly ILogger<DeleteBadgeCommandHandler> _logger;

        public DeleteBadgeCommandHandler(
            IGamificationRepository<Badge> repository,
            ILogger<DeleteBadgeCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting badge {BadgeId}", request.Id);

            var badgeExists = await _repository.GetTable().Where(b => b.Id == request.Id).AnyAsync(cancellationToken);
            if (!badgeExists)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            Badge badge = new Badge()
            {
                Id = request.Id,
                DeletedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            _repository.SaveInclude(badge, nameof(badge.IsDeleted), nameof(badge.DeletedAt));
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Badge {BadgeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Badge deleted successfully");
        }
    }
}
