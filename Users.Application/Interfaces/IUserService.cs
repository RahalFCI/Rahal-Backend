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
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllExplorersUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllVendorsUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllAdminsUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllUsersIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllExplorersIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllVendorsIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<BaseUserDto>>> GetAllAdminsIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<BaseUserDto>> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateUser(BaseUserDto user, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUserPermanently(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default);
    }
}
