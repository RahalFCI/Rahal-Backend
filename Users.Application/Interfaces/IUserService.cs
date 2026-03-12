using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        public ApiResponse<UserSummaryDto> GetAllUsers();
        public Task<ApiResponse<UserDto>> GetById(Guid id);
        public Task<ApiResponse<UserDto>> UpdateUser(UserDto user);
        public Task<ApiResponse<string>> DeleteUser(Guid id);
        public Task<ApiResponse<string>> UpdatePassword(Guid id, string oldPassword, string newPassword);
    }
}
