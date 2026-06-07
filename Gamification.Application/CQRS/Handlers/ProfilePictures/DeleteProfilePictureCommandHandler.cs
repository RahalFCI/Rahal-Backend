using Gamification.Application.CQRS.Commands.ProfilePictures;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Application.CQRS.Handlers.ProfilePictures
{
    internal class DeleteProfilePictureCommandHandler : IRequestHandler<DeleteProfilePictureCommand , ApiResponse<bool>>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DeleteProfilePictureCommandHandler> _logger;

        public DeleteProfilePictureCommandHandler(IFileStorageService fileStorageService, ILogger<DeleteProfilePictureCommandHandler> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteProfilePictureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
                {
                    _logger.LogInformation("No profile picture URL provided for deletion");
                    return ApiResponse<bool>.Failure(ErrorCode.InvalidRequest);
                }

                await _fileStorageService.DeleteAsync(request.ProfilePictureUrl, cancellationToken);
                _logger.LogInformation("Profile picture successfully deleted from {Url}", request.ProfilePictureUrl);
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting profile picture from {Url}", request.ProfilePictureUrl);
                return ApiResponse<bool>.Failure(ErrorCode.ExternalServiceError);
            }
        }
    }
}
