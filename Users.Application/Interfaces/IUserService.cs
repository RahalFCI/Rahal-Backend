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
    public interface IUserService<TUser, TDto, TSummary> where TUser : User where TDto : BaseUserDto where TSummary : BaseUserSummaryDto
    {
        public Task<ApiResponse<IEnumerable<TSummary>>> GetAllUsers(CancellationToken cancellationToken = default);
        public Task<ApiResponse<TDto>> GetById(Guid id, CancellationToken cancellationToken = default);
        public Task<ApiResponse<string>> UpdateUser(TDto user, CancellationToken cancellationToken = default);
        public Task<ApiResponse<string>> DeleteUser(Guid id, CancellationToken cancellationToken = default);
        public Task<ApiResponse<string>> UpdatePassword(Guid id, UpdatePasswordDto updatePasswordDto, CancellationToken cancellationToken = default);
    }
}
