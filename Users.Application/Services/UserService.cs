using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    internal class UserService<TUser, TDto, TSummary> : IUserService<TUser, TDto, TSummary> where TUser : User where TDto : BaseUserDto where TSummary : BaseUserSummaryDto
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IUserMapper<TUser, TDto, TSummary> _mapper;

        public UserService(UserManager<TUser> userManager, IUserMapper<TUser, TDto, TSummary> mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            var result = await _userManager.DeleteAsync(user);

            if(!result.Succeeded)
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);

            return ApiResponse<string>.Success("User deleted successfully.");
        }

        public async Task<ApiResponse<IEnumerable<TSummary>>> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var summaries = users.Select(_mapper.ToSummary).ToList();

            return ApiResponse<IEnumerable<TSummary>>.Success(summaries);
        }

        public async Task<ApiResponse<TDto>> GetById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if(user is null)
                return ApiResponse<TDto>.Failure(ErrorCode.NotFound);

            var userDto = _mapper.ToDto(user);

            if (userDto is null)
                return ApiResponse<TDto>.Failure(ErrorCode.UnknownError);

            return ApiResponse<TDto>.Success(userDto);
        }

        public async Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            var result = await _userManager.ChangePasswordAsync(user, updatePasswordDto.OldPassword, updatePasswordDto.NewPassword);

            if (!result.Succeeded)
                return ApiResponse<string>.Failure(ErrorCode.InvalidCredentials);

            return ApiResponse<string>.Success("Password updated successfully");
        }

        public async Task<ApiResponse<string>> UpdateUser(TDto userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id.ToString());

            if (user is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            var existingUserWithEmail = await _userManager.FindByEmailAsync(userDto.Email);
            if (existingUserWithEmail is null)
                return ApiResponse<string>.Failure(ErrorCode.Conflict);

            var newUser = _mapper.ToEntity(userDto);

            var result = await _userManager.UpdateAsync(newUser);

            if (!result.Succeeded)
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);

            return ApiResponse<string>.Success("User updated successfully");

        }
    }
}
