using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Orchestrators.ExplorerProfiles;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
using Gamification.Application.CQRS.Queries.ExplorerProfiles;
using Gamification.Application.CQRS.Queries.VendorProfiles;
using Gamification.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Orchestrators
{

    public class PermanentDeleteVendorProfileOrchestratorHandler : IRequestHandler<PermanentDeleteVendorProfileOrchestrator, ApiResponse<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PermanentDeleteVendorProfileOrchestratorHandler> _logger;

        public PermanentDeleteVendorProfileOrchestratorHandler(IMediator mediator, ILogger<PermanentDeleteVendorProfileOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(PermanentDeleteVendorProfileOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile deletion orchestration for user {UserId}", request.Id);

                //Query vendor profile to get profile picture URL before deletion
                var vendorProfileResult = await _mediator.Send(new GetVendorProfileByIdQuery(request.Id), cancellationToken);
                if (!vendorProfileResult.IsSuccess)
                {
                    _logger.LogError("Vendor profile not found for user {UserId}", request.Id);
                    return ApiResponse<string>.Failure(vendorProfileResult.errorCode);
                }

                //Delete Vendor Profile
                var profileResult = await _mediator.Send(new DeleteVendorProfileCommand(request.Id), cancellationToken);
                if (!profileResult.IsSuccess)
                {
                    _logger.LogError("Failed to delete vendor profile for user {UserId} with error code {ErrorCode}", request.Id, profileResult.errorCode);
                    return ApiResponse<string>.Failure(profileResult.errorCode);
                }

                // Delete profile picture after successful profile deletion and user stats deletion
                var profilePictureResult = await _mediator.Send(new DeleteProfilePictureCommand(vendorProfileResult.Data.ProfilePictureUrl), cancellationToken);
                if (!profilePictureResult.IsSuccess)
                {
                    _logger.LogError("Failed to delete vendor profile picture with error code {ErrorCode}", profilePictureResult.errorCode);
                    return ApiResponse<string>.Failure(profilePictureResult.errorCode);
                }
                _logger.LogError("Deleted vendor profile picture with error code {ErrorCode}", profilePictureResult.errorCode);


                _logger.LogError("Profile deletion orchestration completed for vendor profile for user {UserId}", request.Id);

                return ApiResponse<string>.Success(profileResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting vendor profile");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
