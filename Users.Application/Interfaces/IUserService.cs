using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllUsersIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<BaseUserDto>> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateUser(BaseUserDto user, IFormFile? profilePicture = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUserPermanently(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default);
    }
}
