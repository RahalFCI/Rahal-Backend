using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Domain.Enums;
using Users.Application.DTOs.Auth;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    /// <summary>
    /// Single instance authentication service for all user types
    /// Handles login, logout, and registration regardless of user type
    /// </summary>
    internal class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenService tokenService,
            ICurrentUserService currentUserService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto?>> LoginAsync(AuthRequestDto loginRequestDto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Login attempt initiated for email: {Email}", loginRequestDto.Email);

            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", loginRequestDto.Email);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidCredentials);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, lockoutOnFailure: true);

            if (result.IsNotAllowed)
            {
                _logger.LogWarning("Login failed: User {UserId} account is not allowed to sign in", user.Id);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.Unauthorized);
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Login failed: User {UserId} account is locked out", user.Id);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.LockedOut);
            }

            if(!result.Succeeded)
            {
                _logger.LogWarning("Login failed: Invalid credentials for user {UserId}", user.Id);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidCredentials);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if(!roles.Any())
            {
                _logger.LogError("Login failed: User {UserId} has no assigned roles", user.Id);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.Forbidden);
            }

            var authResponse = _tokenService.GenerateToken(user, roles, null);

            user.RefreshToken = authResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} with email {Email} successfully logged in with roles: {Roles}", 
                user.Id, user.Email, string.Join(", ", roles));

            return ApiResponse<AuthResponseDto?>.Success(authResponse);
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Logout initiated for user");

            var userId = _currentUserService.UserId;

            if (userId is null)
            {
                _logger.LogWarning("Logout failed: No current user context available");
                throw new UnauthorizedAccessException("User is not logged in");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                _logger.LogWarning("Logout failed: User {UserId} not found", userId);
                throw new UnauthorizedAccessException("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("Logout failed: Could not update user {UserId}. Errors: {Errors}", 
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new InvalidOperationException("Failed to logout user");
            }

            _logger.LogInformation("User {UserId} successfully logged out", userId);
        }

        public async Task<ApiResponse<AuthResponseDto?>> RegisterAsync(User user, string Password, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User registration initiated for email: {Email}", user.Email);

            var existingUser = await _userManager.FindByEmailAsync(user.Email!);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} is already registered", user.Email);
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.AlreadyExists);
            }

            var result = await _userManager.CreateAsync(user, Password);

            if (!result.Succeeded)
            {
                _logger.LogError("Registration failed: Could not create user {Email}. Errors: {Errors}",
                    user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidRequest);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, user.UserType.ToString());

            if (!roleResult.Succeeded)
            {
                _logger.LogError("Registration failed: Could not assign role {Role} to user {UserId}. Errors: {Errors}",
                    user.UserType.ToString(), user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return ApiResponse<AuthResponseDto?>.Failure(ErrorCode.InvalidRequest);
            }

            var roles = new List<string> { user.UserType.ToString() };

            var authResponse = _tokenService.GenerateToken(user, roles, null);

            _logger.LogInformation("User {UserId} with email {Email} successfully registered with role {Role}",
                user.Id, user.Email, user.UserType);

            return ApiResponse<AuthResponseDto?>.Success(authResponse);
        }
    }
}
