using Gamification.Application.CQRS.Commands.Badges;
using Gamification.Application.CQRS.Queries.Badge;
using Gamification.Application.Mappers;
using Gamification.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Gamification.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, ApiResponse<string>>
    {
        private readonly IGamificationRepository<Badge> _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateBadgeCommandHandler> _logger;

        public UpdateBadgeCommandHandler(
            IGamificationRepository<Badge> repository,
            IFileStorageService fileStorageService,
            IMediator mediator,
            ILogger<UpdateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating badge {BadgeId}", request.Id);

            var existingBadge = await _repository.GetTable().Where(c => c.Name == request.Dto.Name && c.Id != request.Id).AnyAsync(cancellationToken);
            if (existingBadge)
            {
                _logger.LogWarning("Badge {badge} already exists", request.Dto.Name);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            var badge = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (badge is null)
            {
                _logger.LogWarning("Badge {BadgeId} not found", request.Id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            BadgeMapper.UpdateEntity(badge, request.Dto);

            if(request.Dto.Image != null)
            {
                if (!string.IsNullOrEmpty(badge.ImageUrl))
                {
                    await _fileStorageService.DeleteAsync(badge.ImageUrl, cancellationToken);
                }
                badge.ImageUrl = await _fileStorageService.UploadAsync(request.Dto.Image, cancellationToken);
            }

            _repository.Update(badge);
            await _repository.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Badge {BadgeId} updated successfully", request.Id);

            return ApiResponse<string>.Success("Badge updated successfully");
        }
    }
}
