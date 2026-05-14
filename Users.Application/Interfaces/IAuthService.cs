using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;
using Users.Application.DTOs._Common;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.Register;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(BaseRegisterDto userDto, string Password, CancellationToken cancellationToken = default);
        Task<ApiResponse<AuthResponseDto?>> LoginAsync(AuthRequestDto loginRequestDto, CancellationToken cancellationToken = default);
        Task LogoutAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteUserWithoutProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
