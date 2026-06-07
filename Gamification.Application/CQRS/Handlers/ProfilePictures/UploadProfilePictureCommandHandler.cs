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
    internal class UploadProfilePictureCommandHandler : IRequestHandler<UploadPorfilePictureCommand, ApiResponse<string?>>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UploadProfilePictureCommandHandler> _logger;

        public UploadProfilePictureCommandHandler(IFileStorageService fileStorageService, ILogger<UploadProfilePictureCommandHandler> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<string?>> Handle(UploadPorfilePictureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ProfilePicture == null || request.ProfilePicture.Length == 0)
                {
                    _logger.LogInformation("No profile picture provided for upload");
                    return ApiResponse<string?>.Failure(ErrorCode.InvalidRequest);
                }

                var profilePictureUrl = await _fileStorageService.UploadAsync(request.ProfilePicture, cancellationToken);

                _logger.LogInformation("Profile picture successfully uploaded to {Url}", profilePictureUrl);
                return ApiResponse<string?>.Success(profilePictureUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading profile picture");
                throw;
            }
        }
    }
}
