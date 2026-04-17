using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Explorer;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;
using Users.Domain.Events;

namespace Users.Application.Services
{
    /// <summary>
    /// Explorer-specific user service
    /// Handles CRUD operations for Explorer profiles
    /// </summary>
    internal class ExplorerService : IUserService<ExplorerDto, ExplorerSummaryDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserMapper<ExplorerDto, ExplorerSummaryDto> _mapper;
        private readonly IProfilePictureService _profilePictureService;
        private readonly IMediator _mediator;
        private readonly ILogger<ExplorerService> _logger;

        public ExplorerService(
            UserManager<User> userManager,
            IUserMapper<ExplorerDto, ExplorerSummaryDto> mapper,
            IProfilePictureService profilePictureService,
            IMediator mediator,
            ILogger<ExplorerService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _profilePictureService = profilePictureService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Explorer deletion initiated for user {UserId}", id);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("Explorer deletion failed: User {UserId} not found or not an Explorer", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Explorer deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            // Publish UserDeletedEvent for search index cleanup
            try
            {
                await _mediator.Publish(new UserDeletedEvent(id), cancellationToken);
                _logger.LogInformation("UserDeletedEvent published for user {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish UserDeletedEvent for user {UserId}. " +
                    "Deletion succeeded but user may still be in search index.",
                    id);
                // Don't throw - search event failure shouldn't fail deletion
            }

            _logger.LogInformation("Explorer {UserId} successfully deleted", id);
            return ApiResponse<string>.Success("Explorer deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<ExplorerSummaryDto>>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Explorers");

            var explorers = await _userManager.Users
                .Where(u => u.UserType == UserRoleEnum.Explorer)
                .Include(u => u.ExplorerProfile)
                .ToListAsync(cancellationToken);

            var summaries = explorers
                .Where(u => u.ExplorerProfile != null)
                .Select(u => _mapper.ToSummary(u))
                .Cast<ExplorerSummaryDto>()
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} Explorers", summaries.Count);

            return ApiResponse<IEnumerable<ExplorerSummaryDto>>.Success(summaries);
        }

        public async Task<ApiResponse<IEnumerable<ExplorerSummaryDto>>> GetAllUsersIncludingDeleted(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Explorers");

            var explorers = await _userManager.Users
                .IgnoreQueryFilters()
                .Where(u => u.UserType == UserRoleEnum.Explorer)
                .Include(u => u.ExplorerProfile)
                .ToListAsync(cancellationToken);

            var summaries = explorers
                .Where(u => u.ExplorerProfile != null)
                .Select(u => _mapper.ToSummary(u))
                .Cast<ExplorerSummaryDto>()
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} Explorers", summaries.Count);

            return ApiResponse<IEnumerable<ExplorerSummaryDto>>.Success(summaries);
        }

        public async Task<ApiResponse<ExplorerDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Explorer retrieval initiated for user {UserId}", id);

            var user = await _userManager.Users
                .Include(u => u.ExplorerProfile)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if(user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("Explorer retrieval failed: User {UserId} not found or not an Explorer", id);
                return ApiResponse<ExplorerDto>.Failure(ErrorCode.NotFound);
            }

            if (user.ExplorerProfile is null)
            {
                _logger.LogError("Explorer retrieval failed: Explorer profile missing for user {UserId}", id);
                return ApiResponse<ExplorerDto>.Failure(ErrorCode.UnknownError);
            }

            var userDto = _mapper.ToDto(user);

            _logger.LogInformation("Explorer {UserId} successfully retrieved", id);
            return ApiResponse<ExplorerDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Password update initiated for Explorer {UserId}", id);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("Password update failed: Explorer {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password update failed for Explorer {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);
            }

            _logger.LogInformation("Password successfully updated for Explorer {UserId}", id);
            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(ExplorerDto userDto, IFormFile? profilePicture = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Explorer update initiated for user {UserId}", userDto.Id);

            var user = await _userManager.Users
                .Include(u => u.ExplorerProfile)
                .FirstOrDefaultAsync(u => u.Id == userDto.Id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("Explorer update failed: User {UserId} not found or not an Explorer", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (user.ExplorerProfile is null)
            {
                _logger.LogError("Explorer update failed: Explorer profile missing for user {UserId}", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != userDto.Id)
            {
                _logger.LogWarning("Explorer update failed: Email {Email} is already in use by another user", userDto.Email);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            try
            {
                // Handle profile picture update if provided
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    _logger.LogInformation("Updating profile picture for Explorer {UserId}", userDto.Id);
                    var profilePictureUrl = await _profilePictureService.UpdateProfilePictureAsync(
                        profilePicture, 
                        user.ProfilePictureURL, 
                        cancellationToken);
                    user.ProfilePictureURL = profilePictureUrl ?? string.Empty;
                    _logger.LogInformation("Profile picture successfully updated for Explorer {UserId}", userDto.Id);
                }

                // Update User entity
                user.DisplayName = userDto.Name;
                user.Email = userDto.Email;
                user.NormalizedEmail = userDto.Email.ToUpper();
                user.UserName = userDto.Email;
                user.NormalizedUserName = userDto.Email.ToUpper();
                user.PhoneNumber = userDto.PhoneNumber;
                if (profilePicture == null || profilePicture.Length == 0)
                {
                    user.ProfilePictureURL = userDto.ProfilePictureUrl ?? string.Empty;
                }

                // Update Explorer Profile
                user.ExplorerProfile.Bio = userDto.Bio;
                user.ExplorerProfile.CountryCode = userDto.CountryCode;
                user.ExplorerProfile.IsPublic = userDto.IsPublic;
                user.ExplorerProfile.Gender = userDto.gender;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("Explorer update failed for user {UserId}. Errors: {Errors}",
                        userDto.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return ApiResponse<string>.Failure(ErrorCode.UnknownError);
                }

                // Publish UserUpdatedEvent for search index update
                try
                {
                    await _mediator.Publish(new UserUpdatedEvent(userDto.Id), cancellationToken);
                    _logger.LogInformation("UserUpdatedEvent published for user {UserId}", userDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish UserUpdatedEvent for user {UserId}. " +
                        "Update succeeded but search index may be stale.",
                        userDto.Id);
                    // Don't throw - search event failure shouldn't fail update
                }

                _logger.LogInformation("Explorer {UserId} successfully updated", userDto.Id);
                return ApiResponse<string>.Success("Explorer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Explorer update for user {UserId}", userDto.Id);
                throw;
            }
        }

        public async Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Explorer restoration initiated for user {UserId}", id);

            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("Explorer restoration failed: User {UserId} not found or not an Explorer", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!user.IsDeleted)
            {
                _logger.LogWarning("Explorer restoration failed: User {UserId} is not deleted", id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            user.IsDeleted = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Explorer restoration failed: Could not restore user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Explorer {UserId} successfully restored", id);
            return ApiResponse<string>.Success("Explorer restored successfully.");
        }
    }
}
