using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Shared.Domain.Events.Users;
using Shared.Infrastructure.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;
using Users.Domain.Events;

namespace Users.Application.Services
{

    internal class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IProfilePictureService _profilePictureService;
        private readonly IMediator _mediator;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            IProfilePictureService profilePictureService,
            IMediator mediator,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _profilePictureService = profilePictureService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default)
        {
            //Delete User
            _logger.LogInformation("User deletion initiated for user {UserId}", id);
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                _logger.LogWarning("User deletion failed: User {UserId} not found or not an User", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            
            if (!result.Succeeded)
            {
                _logger.LogError("User deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            //Delete UserProfile and Update search index
            try
            {
                await _mediator.Publish(new UserDeletedEvent(id), cancellationToken);
                _logger.LogInformation("UserDeletedEvent published for user {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish UserDeletedEvent for user {UserId}. " + "Deletion succeeded but user may still be in search index.", id);

                await _userManager.DeleteAsync(user);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("User {UserId} successfully deleted", id);
            return ApiResponse<string>.Success("User deleted successfully.");
        }

        public async Task<ApiResponse<string>> DeleteUserPermanently(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Permanent deletion initiated for User user {UserId}", id);

            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                _logger.LogWarning("Permanent User deletion failed: User {UserId} not found or not an User", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.DeleteAsync(user);


            if (!result.Succeeded)
            {
                _logger.LogError("Permanent User deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            try
            {
                await _mediator.Publish(new UserDeletedEvent(id), cancellationToken);
                _logger.LogInformation("UserDeletedEvent published for permanent deletion of user {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish UserDeletedEvent for user {UserId}. " +
                    "Deletion succeeded but user may still be in search index.",
                    id);
                // Don't throw - search event failure shouldn't fail deletion
            }

            _logger.LogInformation("User {UserId} permanently deleted", id);
            return ApiResponse<string>.Success("User permanently deleted.");
        }

        public async Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Users - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var users = await _userManager.Users
                .Select(u => MappingExtension.UserToDto(u))
                .ToPagedResultAsync(request, cancellationToken);


            return ApiResponse<PagedResult<BaseUserDto>>.Success(users);
        }

        public async Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllUsersIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Users including deleted - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var users = await _userManager.Users
                .IgnoreQueryFilters()
                .Select(u => MappingExtension.UserToDto(u))
                .ToPagedResultAsync(request, cancellationToken);

            return ApiResponse<PagedResult<BaseUserDto>>.Success(users);
        }

        public async Task<ApiResponse<BaseUserDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User retrieval initiated for user {UserId}", id);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if(user is null)
            {
                _logger.LogWarning("User retrieval failed: User {UserId} not found or not an User", id);
                return ApiResponse<BaseUserDto>.Failure(ErrorCode.NotFound);
            }

            var userDto = MappingExtension.UserToDto(user);

            _logger.LogInformation("User {UserId} successfully retrieved", id);
            return ApiResponse<BaseUserDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Password update initiated for User {UserId}", id);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                _logger.LogWarning("Password update failed: User {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password update failed for User {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);
            }

            _logger.LogInformation("Password successfully updated for User {UserId}", id);
            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(BaseUserDto userDto, IFormFile? profilePicture = null, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User update initiated for user {UserId}", userDto.Id);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userDto.Id, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User update failed: User {UserId} not found or not an User", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != userDto.Id)
            {
                _logger.LogWarning("User update failed: Email {Email} is already in use by another user", userDto.Email);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            try
            {
                // Handle profile picture update if provided
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    _logger.LogInformation("Updating profile picture for User {UserId}", userDto.Id);
                    var profilePictureUrl = await _profilePictureService.UpdateProfilePictureAsync(
                        profilePicture, 
                        user.ProfilePictureURL, 
                        cancellationToken);
                    user.ProfilePictureURL = profilePictureUrl ?? string.Empty;
                    _logger.LogInformation("Profile picture successfully updated for User {UserId}", userDto.Id);
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

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError("User update failed for user {UserId}. Errors: {Errors}",
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

                _logger.LogInformation("User {UserId} successfully updated", userDto.Id);
                return ApiResponse<string>.Success("User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during User update for user {UserId}", userDto.Id);
                throw;
            }
        }

        public async Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User restoration initiated for user {UserId}", id);

            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Explorer)
            {
                _logger.LogWarning("User restoration failed: User {UserId} not found or not an User", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!user.IsDeleted)
            {
                _logger.LogWarning("User restoration failed: User {UserId} is not deleted", id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            user.IsDeleted = false;
            user.DeletedAt = null;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("User restoration failed: Could not restore user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("User {UserId} successfully restored", id);
            return ApiResponse<string>.Success("User restored successfully.");
        }
    }
}
