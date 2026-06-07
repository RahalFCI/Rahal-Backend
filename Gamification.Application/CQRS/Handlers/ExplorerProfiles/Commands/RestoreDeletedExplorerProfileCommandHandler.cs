using Gamification.Application.CQRS.Commands.ExplorerProfiles;
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

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Commands
{
    public class RestoreDeletedExplorerProfileCommandHandler : IRequestHandler<RestoreDeletedExplorerProfileCommand, ApiResponse<string>>    
    {
        private readonly IGamificationRepository<ExplorerProfile> _repository;
        private readonly ILogger<RestoreDeletedExplorerProfileCommandHandler> _logger;

        public RestoreDeletedExplorerProfileCommandHandler(IGamificationRepository<ExplorerProfile> repository, ILogger<RestoreDeletedExplorerProfileCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(RestoreDeletedExplorerProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Restoring explorer profile for user {UserId}", request.ExplorerId);
                var existingExplorer = await _repository.GetTable().Where(x => x.UserId == request.ExplorerId).AnyAsync(cancellationToken);
                if (!existingExplorer)
                {
                    _logger.LogError("Explorer profile does not exist for user {UserId}", request.ExplorerId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                ExplorerProfile explorerProfile = new ExplorerProfile
                {
                    UserId = request.ExplorerId,
                    DeletedAt = null,
                    IsDeleted = false
                };

                _repository.SaveInclude(explorerProfile, nameof(explorerProfile.IsDeleted), nameof(explorerProfile.DeletedAt));
                await _repository.SaveChangesAsync();

                _logger.LogError("Restored explorer profile for user {UserId}", request.ExplorerId);

                return ApiResponse<string>.Success("Explorer profile restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while restoring explorer profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
