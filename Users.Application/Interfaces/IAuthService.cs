using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs;
using Users.Domain.Enums;
using Shared.Application.DTOs;

namespace Users.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterRequestDto requestDto, UserRoleEnum userType);
        Task<ApiResponse<string>> LoginAsync(LoginRequestDto loginRequestDto);
        Task LogoutAsync();
    }
}
