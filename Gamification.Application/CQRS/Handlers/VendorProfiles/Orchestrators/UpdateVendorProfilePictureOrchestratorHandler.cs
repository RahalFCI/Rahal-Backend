using Gamification.Application.CQRS.Commands.ProfilePictures;
using Gamification.Application.CQRS.Orchestrators.VendorProfiles;
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

namespace Gamification.Application.CQRS.Handlers.VendorProfiles.Orchestrators
{

    public class UpdateVendorProfilePictureOrchestratorHandler : IRequestHandler<UpdateVendorProfilePictureOrchestrator, ApiResponse<string>>
    {
        private readonly IGamificationRepository<VendorProfile> _repository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateVendorProfilePictureOrchestratorHandler> _logger;

        public UpdateVendorProfilePictureOrchestratorHandler(IMediator mediator, ILogger<UpdateVendorProfilePictureOrchestratorHandler> logger, IGamificationRepository<VendorProfile> genericRepository)
        {
            _mediator = mediator;
            _repository = genericRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(UpdateVendorProfilePictureOrchestrator request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError("Started profile picture update orchestration for user {UserId}", request.UserId);

                var user = await _repository.GetTable().Where(u => u.UserId == request.UserId).FirstOrDefaultAsync(cancellationToken);

                if (user is null)
                {
                    _logger.LogError("User with ID {UserId} not found", request.UserId);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                var profilePictureResult = await _mediator.Send(new UpdateProfilePictureCommand(request.ProfilePicture, user.ProfilePictureURL), cancellationToken);
                if (profilePictureResult.IsSuccess)
                    _logger.LogError("Failed to update vendor profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                user.ProfilePictureURL = profilePictureResult.Data!;
                _repository.Update(user);
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogError("Uploaded explorer profile picture with error code {ErrorCode}", profilePictureResult.errorCode);

                return ApiResponse<string>.Success(profilePictureResult.Data!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating vendor profile picture");
                return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
            }
        }
    }
}
