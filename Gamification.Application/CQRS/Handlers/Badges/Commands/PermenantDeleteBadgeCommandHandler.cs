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
    public class PermenantDeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Badge> _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<PermenantDeleteBadgeCommandHandler> _logger;

        public PermenantDeleteBadgeCommandHandler(
            IGamificationRepository<Badge> repository,
            IFileStorageService fileStorageService,
            ILogger<PermenantDeleteBadgeCommandHandler> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting badge {BadgeId}", request.Id);

            var badge = await _repository.GetTable().Where(b => b.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            if (badge.ImageUrl is not null)
            {
                await _fileStorageService.DeleteAsync(badge.ImageUrl, cancellationToken);
                _logger.LogInformation("Badge {BadgeId} Image deleted successfully", badge.Id);
            }

            _repository.Delete(badge);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Badge {BadgeId} deleted successfully", request.Id);

            return ApiResponse<string>.Success("Badge deleted successfully");
        }
    }
}
