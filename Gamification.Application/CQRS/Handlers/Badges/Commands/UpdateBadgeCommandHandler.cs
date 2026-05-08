using Gamification.Application.CQRS.Commands.Badges;
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

namespace Gamification.Application.CQRS.Handlers.Badges.Commands
{
    public class UpdateBadgeCommandHandler : IRequestHandler<UpdateBadgeCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Badge> _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UpdateBadgeCommandHandler> _logger;

        public UpdateBadgeCommandHandler(
            IGenericRepository<Badge> repository,
            IFileStorageService fileStorageService,
            ILogger<UpdateBadgeCommandHandler> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateBadgeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating badge {BadgeId}", request.Id);

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
