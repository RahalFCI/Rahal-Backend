using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Enums;
using Shared.Application.DTOs;
using Users.Domain.Entities._Common;
using Users.Application.DTOs.Auth;

namespace Users.Application.Interfaces
{
    public interface IAuthService<TUser> where TUser : User
    {
        Task<ApiResponse<AuthResponseDto?>> RegisterAsync(User user, String Password);
        Task<ApiResponse<AuthResponseDto?>> LoginAsync(AuthRequestDto loginRequestDto);
        Task LogoutAsync();
    }
}
