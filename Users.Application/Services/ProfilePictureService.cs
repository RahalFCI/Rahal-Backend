using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Services
{
    /// <summary>
    /// Service for handling profile picture uploads and deletions
    /// Abstracts the file storage logic for user profile pictures
    /// </summary>
    public interface IProfilePictureService
    {
        Task<string?> UploadProfilePictureAsync(IFormFile? profilePicture, CancellationToken cancellationToken = default);
        Task DeleteProfilePictureAsync(string? profilePictureUrl, CancellationToken cancellationToken = default);
        Task<string?> UpdateProfilePictureAsync(IFormFile? newProfilePicture, string? oldProfilePictureUrl, CancellationToken cancellationToken = default);
    }

    internal class ProfilePictureService : IProfilePictureService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ProfilePictureService> _logger;

        public ProfilePictureService(IFileStorageService fileStorageService, ILogger<ProfilePictureService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<string?> UploadProfilePictureAsync(IFormFile? profilePicture, CancellationToken cancellationToken = default)
        {
            try
            {
                if (profilePicture == null || profilePicture.Length == 0)
                {
                    _logger.LogInformation("No profile picture provided for upload");
                    return null;
                }

                var profilePictureUrl = await _fileStorageService.UploadAsync(profilePicture);
                
                _logger.LogInformation("Profile picture successfully uploaded to {Url}", profilePictureUrl);
                return profilePictureUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading profile picture");
                throw;
            }
        }

        public async Task DeleteProfilePictureAsync(string? profilePictureUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profilePictureUrl))
                {
                    _logger.LogInformation("No profile picture URL provided for deletion");
                    return;
                }

                await _fileStorageService.DeleteAsync(profilePictureUrl);
                
                _logger.LogInformation("Profile picture successfully deleted from {Url}", profilePictureUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting profile picture from {Url}", profilePictureUrl);
                // Don't throw - allow user updates to continue even if old picture deletion fails
            }
        }

        public async Task<string?> UpdateProfilePictureAsync(IFormFile? newProfilePicture, string? oldProfilePictureUrl, CancellationToken cancellationToken = default)
        {
            try
            {
                // If no new picture provided, keep the old URL
                if (newProfilePicture == null || newProfilePicture.Length == 0)
                {
                    _logger.LogInformation("No new profile picture provided, keeping existing picture");
                    return oldProfilePictureUrl;
                }

                // Upload new picture
                var newProfilePictureUrl = await UploadProfilePictureAsync(newProfilePicture, cancellationToken);

                // Delete old picture if it exists
                if (!string.IsNullOrWhiteSpace(oldProfilePictureUrl) && oldProfilePictureUrl != newProfilePictureUrl)
                {
                    await DeleteProfilePictureAsync(oldProfilePictureUrl, cancellationToken);
                }

                return newProfilePictureUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile picture");
                throw;
            }
        }
    }
}
