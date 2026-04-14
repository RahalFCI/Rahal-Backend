using Shared.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Users.Application.DTOs.Auth;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{
    public interface ITokenService
    {
        AuthResponseDto GenerateToken(User user, IEnumerable<string> role, string? ValidRefreshToken);
        public string GenerateRefreshToken();
        ClaimsPrincipal? GetUserInfoFromExpiredToken(string? token);
        Task<ApiResponse<AuthResponseDto>> RefreshExpiredToken(TokenDto token);
    }
}
