using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Explorer;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    public interface IUserService<TDto, TSummary> 
        where TDto : BaseUserDto 
        where TSummary : BaseUserSummaryDto
    {
        Task<ApiResponse<PagedResult<TSummary>>> GetAllUsers(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<TSummary>>> GetAllUsersIncludingDeleted(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<TDto>> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateUser(TDto user, IFormFile? profilePicture = null, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUserPermanently(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default);
    }
}
