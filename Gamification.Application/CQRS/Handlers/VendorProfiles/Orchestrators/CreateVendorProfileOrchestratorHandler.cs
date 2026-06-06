using Gamification.Application.CQRS.Commands.ExplorerProfiles;
using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Commands.UserStat;
using Gamification.Application.CQRS.Commands.VendorProfiles;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
using Gamification.Application.DTOs.UserStats;
using Gamification.Application.DTOs.Vendor;
using Gamification.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Orchestrators
{
    public class CreateVendorProfileOrchestratorHandler : IRequestHandler<CreateVendorProfileOrchestrator, ApiResponse<GetVendorDto>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateVendorProfileOrchestratorHandler> _logger;
        public CreateVendorProfileOrchestratorHandler(IMediator mediator, ILogger<CreateVendorProfileOrchestratorHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<GetVendorDto>> Handle(CreateVendorProfileOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile creation orchestration for user {UserId}", request.addVendorDto.UserId);

                var profilePictureResult = await _mediator.Send(new UploadPorfilePictureCommand(request.profilePicture), cancellationToken);
                if (profilePictureResult.IsSuccess)
                    _logger.LogError("Failed to upload vendor profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                _logger.LogError("Uploaded vendor profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                var profileResult = await _mediator.Send(new CreateVendorProfileCommand(request.addVendorDto, profilePictureResult.Data!), cancellationToken);

                if (!profileResult.IsSuccess)
                {
                    _logger.LogError("Failed to create vendor profile for user {UserId} with error code {ErrorCode}", request.addVendorDto.UserId, profileResult.errorCode);
                    await _mediator.Send(new DeleteProfilePictureCommand(profilePictureResult.Data!), cancellationToken);
                    _logger.LogError("Deleted uploaded profile picture for user {UserId} due to profile creation failure", request.addVendorDto.UserId);
                    return ApiResponse<GetVendorDto>.Failure(profileResult.errorCode);
                }

                _logger.LogError("Profile creation orchestration completed for vendor profile for user {UserId}", request.addVendorDto.UserId);

                return ApiResponse<GetVendorDto>.Success(profileResult.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating vendor profile");
                return ApiResponse<GetVendorDto>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
