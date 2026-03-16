using MediatR;
using Microsoft.AspNetCore.Identity;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Users.Application.DTOs.Auth;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    internal class AuthService<TUser> : IAuthService<TUser> where TUser : User
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly TokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager, TokenService tokenService, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _currentUserService = currentUserService;
        }
        public async Task<ApiResponse<AuthResponseDto?>> LoginAsync(AuthRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
            if (user == null)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidCredentials);

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, lockoutOnFailure: true);

            if (result.IsNotAllowed)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.Unauthorized);
            
            if (result.IsLockedOut)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.LockedOut);

            if(!result.Succeeded)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidCredentials);

            var roles = await _userManager.GetRolesAsync(user);
            if(!roles.Any())
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.Forbidden);

            var authResponse = _tokenService.GenerateToken(user, roles, null);

            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            return ApiResponse<AuthResponseDto?>.Success(authResponse);

        }

        public async Task LogoutAsync()
        {
            var userId = _currentUserService.UserId;

            if (userId is null)
                throw new UnauthorizedAccessException("User is not logged in");

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
                throw new UnauthorizedAccessException("User not found");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task<ApiResponse<AuthResponseDto?>> RegisterAsync(User user, string Password)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email!);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.AlreadyExists);

            var result = await _userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidRequest);

            var roleResult = await _userManager.AddToRoleAsync(user, user.Role.ToString());

            if (!roleResult.Succeeded)
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidRequest);

            var roles = new List<string> { user.Role.ToString() };

            var authResponse = _tokenService.GenerateToken(user, roles, null);

            return ApiResponse<AuthResponseDto?>.Success(authResponse);
        }
    }
}
