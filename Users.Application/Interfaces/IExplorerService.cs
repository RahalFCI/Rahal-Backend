using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Admin;

namespace Users.Application.Interfaces
{
    internal interface IExplorerService
    {
        public ApiResponse<AdminDto> GetAllUsers();
        public Task<ApiResponse<AdminDto>> GetById(Guid id);
        public Task<ApiResponse<AdminDto>> UpdateUser(AdminDto user);
        public Task<ApiResponse<string>> DeleteUser(Guid id);
        public Task<ApiResponse<string>> DeleteUserPermanently(Guid id);
        public Task<ApiResponse<string>> UpdatePassword(Guid id, string oldPassword, string newPassword);
    }
}
