using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Explorer;
using Users.Application.DTOs.Vendor;

namespace Users.Application.Interfaces
{
    internal interface IVendorService
    {
        public ApiResponse<VendorSummaryDto> GetAllUsers();
        public Task<ApiResponse<VendorDto>> GetById(Guid id);
        public Task<ApiResponse<VendorDto>> UpdateUser(VendorDto user);
        public Task<ApiResponse<string>> DeleteUser(Guid id);
        public Task<ApiResponse<string>> DeleteUserPermanently(Guid id);
        public Task<ApiResponse<string>> UpdatePassword(Guid id, string oldPassword, string newPassword);
    }
}
