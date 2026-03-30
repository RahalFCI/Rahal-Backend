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
using Users.Application.DTOs.Vendor;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    /// <summary>
    /// Vendor-specific user service
    /// Handles CRUD operations for Vendor profiles
    /// </summary>
    internal class VendorService : IUserService<VendorDto, VendorSummaryDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserMapper<VendorDto, VendorSummaryDto> _mapper;
        private readonly ILogger<VendorService> _logger;

        public VendorService(
            UserManager<User> userManager,
            IUserMapper<VendorDto, VendorSummaryDto> mapper,
            ILogger<VendorService> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vendor deletion initiated for user {UserId}", id);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null || user.UserType != UserRoleEnum.Vendor)
            {
                _logger.LogWarning("Vendor deletion failed: User {UserId} not found or not a Vendor", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.DeleteAsync(user);

            if(!result.Succeeded)
            {
                _logger.LogError("Vendor deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Vendor {UserId} successfully deleted", id);
            return ApiResponse<string>.Success("Vendor deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<VendorSummaryDto>>> GetAllUsers(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Vendors");

            var vendors = await _userManager.Users
                .Where(u => u.UserType == UserRoleEnum.Vendor)
                .Include(u => u.VendorProfile)
                .ToListAsync(cancellationToken);

            var summaries = vendors
                .Where(u => u.VendorProfile != null)
                .Select(u => _mapper.ToSummary(u))
                .Cast<VendorSummaryDto>()
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} Vendors", summaries.Count);

            return ApiResponse<IEnumerable<VendorSummaryDto>>.Success(summaries);
        }

        public async Task<ApiResponse<IEnumerable<VendorSummaryDto>>> GetAllUsersIncludingDeleted(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all Vendors");

            var vendors = await _userManager.Users
                .IgnoreQueryFilters()
                .Where(u => u.UserType == UserRoleEnum.Vendor)
                .Include(u => u.VendorProfile)
                .ToListAsync(cancellationToken);

            var summaries = vendors
                .Where(u => u.VendorProfile != null)
                .Select(u => _mapper.ToSummary(u))
                .Cast<VendorSummaryDto>()
                .ToList();

            _logger.LogInformation("Successfully retrieved {Count} Vendors", summaries.Count);

            return ApiResponse<IEnumerable<VendorSummaryDto>>.Success(summaries);
        }

        public async Task<ApiResponse<VendorDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vendor retrieval initiated for user {UserId}", id);

            var user = await _userManager.Users
                .Include(u => u.VendorProfile)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if(user is null || user.UserType != UserRoleEnum.Vendor)
            {
                _logger.LogWarning("Vendor retrieval failed: User {UserId} not found or not a Vendor", id);
                return ApiResponse<VendorDto>.Failure(ErrorCode.NotFound);
            }

            if (user.VendorProfile is null)
            {
                _logger.LogError("Vendor retrieval failed: Vendor profile missing for user {UserId}", id);
                return ApiResponse<VendorDto>.Failure(ErrorCode.UnknownError);
            }

            var userDto = _mapper.ToDto(user);

            _logger.LogInformation("Vendor {UserId} successfully retrieved", id);
            return ApiResponse<VendorDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Password update initiated for Vendor {UserId}", id);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null || user.UserType != UserRoleEnum.Vendor)
            {
                _logger.LogWarning("Password update failed: Vendor {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password update failed for Vendor {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);
            }

            _logger.LogInformation("Password successfully updated for Vendor {UserId}", id);
            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(VendorDto userDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vendor update initiated for user {UserId}", userDto.Id);

            var user = await _userManager.Users
                .Include(u => u.VendorProfile)
                .FirstOrDefaultAsync(u => u.Id == userDto.Id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Vendor)
            {
                _logger.LogWarning("Vendor update failed: User {UserId} not found or not a Vendor", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (user.VendorProfile is null)
            {
                _logger.LogError("Vendor update failed: Vendor profile missing for user {UserId}", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != userDto.Id)
            {
                _logger.LogWarning("Vendor update failed: Email {Email} is already in use by another user", userDto.Email);
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

            // Update Vendor Profile
            user.VendorProfile.CountryCode = userDto.CountryCode;
            user.VendorProfile.Address = userDto.Address;
            user.VendorProfile.AddressUrl = userDto.AddressUrl;
            user.VendorProfile.WorkingHours = userDto.WorkingHours;
            user.VendorProfile.CategoryId = userDto.CategoryId;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Vendor update failed for user {UserId}. Errors: {Errors}",
                    userDto.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Vendor {UserId} successfully updated", userDto.Id);
            return ApiResponse<string>.Success("Vendor updated successfully");
        }

        public async Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Vendor restoration initiated for user {UserId}", id);

            var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user is null || user.UserType != UserRoleEnum.Vendor)
            {
                _logger.LogWarning("Vendor restoration failed: User {UserId} not found or not a Vendor", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (!user.IsDeleted)
            {
                _logger.LogWarning("Vendor restoration failed: User {UserId} is not deleted", id);
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }

            user.IsDeleted = false;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Vendor restoration failed: Could not restore user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("Vendor {UserId} successfully restored", id);
            return ApiResponse<string>.Success("Vendor restored successfully.");
        }
    }
}
