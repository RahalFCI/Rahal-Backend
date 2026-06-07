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
    public class UpdatePorfilePictureCommndHandler : IRequestHandler<UpdateProfilePictureCommand, ApiResponse<string?>>
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<UpdatePorfilePictureCommndHandler> _logger;
        public UpdatePorfilePictureCommndHandler(IFileStorageService fileStorageService, ILogger<UpdatePorfilePictureCommndHandler> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }
        public async Task<ApiResponse<string?>> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.OldProfilePictureUrl))
            {
                _logger.LogInformation("No profile picture URL provided for deletion");
                return ApiResponse<string?>.Failure(ErrorCode.InvalidRequest);
            }

            await _fileStorageService.DeleteAsync(request.OldProfilePictureUrl, cancellationToken);
            _logger.LogInformation("Profile picture successfully deleted from {Url}", request.OldProfilePictureUrl);

            if (request.ProfilePicture == null || request.ProfilePicture.Length == 0)
            {
                _logger.LogInformation("No profile picture provided for upload");
                return ApiResponse<string?>.Failure(ErrorCode.InvalidRequest);
            }

            var profilePictureUrl = await _fileStorageService.UploadAsync(request.ProfilePicture, cancellationToken);

            _logger.LogInformation("Profile picture successfully uploaded to {Url}", profilePictureUrl);
            return ApiResponse<string?>.Success(profilePictureUrl);
        }
    }
}
