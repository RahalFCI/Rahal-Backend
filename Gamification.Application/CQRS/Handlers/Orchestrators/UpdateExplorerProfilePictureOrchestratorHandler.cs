using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Commands.UserStats;
using Gamification.Application.CQRS.Orchestrators;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.DTOs.UserStats;
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

namespace Gamification.Application.CQRS.Handlers.Orchestrators
{
    public class UpdateExplorerProfilePictureOrchestratorHandler : IRequestHandler<UpdateExplorerProfilePictureOrchestrator, ApiResponse<string>>
    {
        private readonly IGenericRepository<ExplorerProfile> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateExplorerProfilePictureOrchestrator> _logger;

        public UpdateExplorerProfilePictureOrchestratorHandler(IMediator mediator, ILogger<UpdateExplorerProfilePictureOrchestrator> logger, IGenericRepository<ExplorerProfile> genericRepository)
        {
            _mediator = mediator;
            _repository = genericRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateExplorerProfilePictureOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile picture update orchestration for user {UserId}", request.UserId);

                var user = await _repository.GetTable().Where(u => u.Id == request.UserId).FirstOrDefaultAsync(cancellationToken);

                if (user is null)
                {
                    _logger.LogError("User with ID {UserId} not found", request.UserId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                var profilePictureResult = await _mediator.Send(new UpdateProfilePictureCommand(request.ProfilePicture, user.ProfilePictureURL), cancellationToken);
                if (profilePictureResult.IsSuccess)
                    _logger.LogError("Failed to update explorer profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                user.ProfilePictureURL = profilePictureResult.Data!;
                _repository.Update(user);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogError("Uploaded explorer profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                return ApiResponse<string>.Success(profilePictureResult.Data!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating explorer profile picture");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
