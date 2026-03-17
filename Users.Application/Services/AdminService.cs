using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Admin;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    /// <summary>
    /// Admin-specific user service
    /// Handles CRUD operations for Admin profiles
    /// </summary>
    internal class AdminService : IUserService<AdminDto, AdminSummaryDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserMapper<AdminDto, AdminSummaryDto> _mapper;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            UserManager<User> userManager,
            IUserMapper<AdminDto, AdminSummaryDto> mapper,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin deletion initiated for user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null || user.UserType != UserRoleEnum.Admin)
            {
                _logger.LogWarning("Admin deletion failed: User {UserId} not found or not an Admin", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.DeleteAsync(user);

            if(!result.Succeeded)
            {
                _logger.LogError("Admin deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Admin {UserId} successfully deleted", id);
            return ApiResponse<string>.Success("Admin deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<AdminSummaryDto>>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Admins");

            var admins = await _userManager.Users
                .Where(u => u.UserType == UserRoleEnum.Admin)
                .Include(u => u.AdminProfile)
                .ToListAsync(cancellationToken);

            var summaries = admins
                .Where(u => u.AdminProfile != null)
                .Select(u => _mapper.ToSummary(u))
                .Cast<AdminSummaryDto>()
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} Admins", summaries.Count);

            return ApiResponse<IEnumerable<AdminSummaryDto>>.Success(summaries);
        }

        public async Task<ApiResponse<AdminDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin retrieval initiated for user {UserId}", id);

            var user = await _userManager.Users
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if(user is null || user.UserType != UserRoleEnum.Admin)
            {
                _logger.LogWarning("Admin retrieval failed: User {UserId} not found or not an Admin", id);
                return ApiResponse<AdminDto>.Failure(ErrorCode.NotFound);
            }

            if (user.AdminProfile is null)
            {
                _logger.LogError("Admin retrieval failed: Admin profile missing for user {UserId}", id);
                return ApiResponse<AdminDto>.Failure(ErrorCode.UnknownError);
            }

            var userDto = _mapper.ToDto(user);

            _logger.LogInformation("Admin {UserId} successfully retrieved", id);
            return ApiResponse<AdminDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Password update initiated for Admin {UserId}", id);

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null || user.UserType != UserRoleEnum.Admin)
            {
                _logger.LogWarning("Password update failed: Admin {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password update failed for Admin {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);
            }

            _logger.LogInformation("Password successfully updated for Admin {UserId}", id);
            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(AdminDto userDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Admin update initiated for user {UserId}", userDto.Id);

            var user = await _userManager.Users
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(u => u.Id == userDto.Id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Admin)
            {
                _logger.LogWarning("Admin update failed: User {UserId} not found or not an Admin", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (user.AdminProfile is null)
            {
                _logger.LogError("Admin update failed: Admin profile missing for user {UserId}", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != userDto.Id)
            {
                _logger.LogWarning("Admin update failed: Email {Email} is already in use by another user", userDto.Email);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            // Update User entity
            user.DisplayName = userDto.Name;
            user.Email = userDto.Email;
            user.NormalizedEmail = userDto.Email.ToUpper();
            user.UserName = userDto.Email;
            user.NormalizedUserName = userDto.Email.ToUpper();
            user.PhoneNumber = userDto.PhoneNumber;
            user.ProfilePictureURL = userDto.ProfilePictureUrl ?? string.Empty;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Admin update failed for user {UserId}. Errors: {Errors}",
                    userDto.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Admin {UserId} successfully updated", userDto.Id);
            return ApiResponse<string>.Success("Admin updated successfully");
        }
    }
}
