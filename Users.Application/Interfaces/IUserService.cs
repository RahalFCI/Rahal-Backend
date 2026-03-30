using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Explorer;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    /// <summary>
    /// Generic interface for type-specific user services
    /// Each user type (Explorer, Vendor, Admin) has its own service implementation
    /// </summary>
    public interface IUserService<TDto, TSummary> 
        where TDto : BaseUserDto 
        where TSummary : BaseUserSummaryDto
    {
        Task<ApiResponse<IEnumerable<TSummary>>> GetAllUsers(CancellationToken cancellationToken = default);
        Task<ApiResponse<IEnumerable<TSummary>>> GetAllUsersIncludingDeleted(CancellationToken cancellationToken = default);
        Task<ApiResponse<TDto>> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdateUser(TDto user, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> RestoreDeletedUser(Guid id, CancellationToken cancellationToken = default);
    }
}
