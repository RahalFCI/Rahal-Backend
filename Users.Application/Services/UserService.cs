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
using Users.Application.DTOs.Explorer;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;

namespace Users.Application.Services
{
    internal class UserService<TUser, TDto, TSummary> : IUserService<TUser, TDto, TSummary> 
        where TUser : User 
        where TDto : BaseUserDto 
        where TSummary : BaseUserSummaryDto
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IUserMapper<TUser, TDto, TSummary> _mapper;
        private readonly ILogger<UserService<TUser, TDto, TSummary>> _logger;

        public UserService(
            UserManager<TUser> userManager,
            IUserMapper<TUser, TDto, TSummary> mapper,
            ILogger<UserService<TUser, TDto, TSummary>> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id)
        {
            _logger.LogInformation("User deletion initiated for user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                _logger.LogWarning("User deletion failed: User {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.DeleteAsync(user);

            if(!result.Succeeded)
            {
                _logger.LogError("User deletion failed: Could not delete user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("User {UserId} successfully deleted", id);
            return ApiResponse<string>.Success("User deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<TSummary>>> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users of type {UserType}", typeof(TUser).Name);

            var users = await _userManager.Users.ToListAsync();

            var summaries = users.Select(_mapper.ToSummary).ToList();

            _logger.LogInformation("Successfully retrieved {Count} users of type {UserType}", 
                summaries.Count, typeof(TUser).Name);

            return ApiResponse<IEnumerable<TSummary>>.Success(summaries);
        }

        public async Task<ApiResponse<TDto>> GetById(Guid id)
        {
            _logger.LogInformation("User retrieval initiated for user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id.ToString());

            if(user is null)
            {
                _logger.LogWarning("User retrieval failed: User {UserId} not found", id);
                return ApiResponse<TDto>.Failure(ErrorCode.NotFound);
            }

            var userDto = _mapper.ToDto(user);

            if (userDto is null)
            {
                _logger.LogError("User retrieval failed: Could not map user {UserId} to DTO", id);
                return ApiResponse<TDto>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("User {UserId} successfully retrieved", id);
            return ApiResponse<TDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto)
        {
            _logger.LogInformation("Password update initiated for user {UserId}", id);

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
            {
                _logger.LogWarning("Password update failed: User {UserId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Password update failed for user {UserId}. Errors: {Errors}",
                    id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);
            }

            _logger.LogInformation("Password successfully updated for user {UserId}", id);
            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(TDto userDto)
        {
            _logger.LogInformation("User update initiated for user {UserId}", userDto.Id);

            var user = await _userManager.FindByIdAsync(userDto.Id.ToString());

            if (user is null)
            {
                _logger.LogWarning("User update failed: User {UserId} not found", userDto.Id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is not null && existingUserWithEmail.Id != userDto.Id)
            {
                _logger.LogWarning("User update failed: Email {Email} is already in use by another user", userDto.Email);
                return ApiResponse<string>.Failure(ErrorCode.Conflict);
            }

            var newUser = _mapper.ToEntity(userDto);

            var result = await _userManager.UpdateAsync(newUser);

            if (!result.Succeeded)
            {
                _logger.LogError("User update failed for user {UserId}. Errors: {Errors}",
                    userDto.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }

            _logger.LogInformation("User {UserId} successfully updated", userDto.Id);
            return ApiResponse<string>.Success("User updated successfully");
        }
    }
}

            return ApiResponse<string>.Success("User updated successfully");

        }
    }
}
