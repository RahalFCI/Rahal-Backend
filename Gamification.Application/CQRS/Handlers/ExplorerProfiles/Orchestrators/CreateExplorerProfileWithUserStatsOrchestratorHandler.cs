using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Commands.UserStats;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.DTOs.Explorer;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.Interfaces;
using Gamification.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.ExplorerProfiles.Orchestrators
{
    public class CreateExplorerProfileWithUserStatsOrchestratorHandler : IRequestHandler<CreateExplorerProfileWithUserStatsOrchestrator, ApiResponse<Guid>>
    {
        private readonly IMediator _mediator;
        private readonly IGamificationUnitOfWork _unitOfWork;
        private readonly ILogger<CreateExplorerProfileWithUserStatsOrchestratorHandler> _logger;

        public CreateExplorerProfileWithUserStatsOrchestratorHandler(IMediator mediator, IGamificationUnitOfWork unitOfWork, ILogger<CreateExplorerProfileWithUserStatsOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateExplorerProfileWithUserStatsOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile creation orchestration for user {UserId}", request.explorerDto.UserId);
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var profilePictureResult = await _mediator.Send(new UploadPorfilePictureCommand(request.ProfilePicture), cancellationToken);
                if (profilePictureResult.IsSuccess)
                    _logger.LogError("Failed to upload explorer profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                _logger.LogError("Uploaded explorer profile picture with error code {ErrorCode}", profilePictureResult.errorCode);


                var profileResult = await _mediator.Send(new CreateExplorerProfileCommand(request.explorerDto, profilePictureResult.Data!), cancellationToken);

                if (!profileResult.IsSuccess)
                {
                    _logger.LogError("Failed to create explorer profile for user {UserId} with error code {ErrorCode}", request.explorerDto.UserId, profileResult.errorCode);

                    await _mediator.Send(new DeleteProfilePictureCommand(profilePictureResult.Data!), cancellationToken);
                    _logger.LogError("Deleted uploaded profile picture for user {UserId} due to profile creation failure", request.explorerDto.UserId);

                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<Guid>.Failure(profileResult.errorCode);
                } 


                var userStatsResult = await _mediator.Send(new CreateUserStatsCommand(new CreateUserStatsDto() { ExplorerId = profileResult.Data }), cancellationToken);

                if (!userStatsResult.IsSuccess)
                {
                    _logger.LogError("Failed to create user stats for explorer {ExplorerId} with error code {ErrorCode}", profileResult.Data, userStatsResult.errorCode);
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<Guid>.Failure(userStatsResult.errorCode);
                }

                _logger.LogError("Profile creation orchestration completed for explorer profile for user {UserId}", request.explorerDto.UserId);

                return ApiResponse<Guid>.Success(profileResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating explorer profile");
                return ApiResponse<Guid>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
