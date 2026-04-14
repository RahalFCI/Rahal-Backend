using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;
using Users.Application.DTOs.OAuth;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;

namespace Users.Application.Services
{
    /// <summary>
    /// Service for handling OAuth authentication and registration
    /// </summary>
    internal class GoogleAuthService : IOAuthGoogleService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(
            UserManager<User> userManager,
            ITokenService tokenService,
            ILogger<GoogleAuthService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> SignInAsync(User user, GoogleUserInfo googleUserInfo, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("OAuth signin initiated for user {UserId}", user.Id);

                // Add Google login if not already linked
                var addLoginResult = await AddGoogleLoginAsync(user, googleUserInfo);
                if (!addLoginResult.IsSuccess)
                {
                    return addLoginResult;
                }

                // Generate and return tokens
                return await GenerateAuthTokenAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth signin for user {UserId}", user.Id);
                return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
            }
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(User user, GoogleUserInfo googleUserInfo, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("OAuth register initiated for email {Email}", googleUserInfo.Email);

                // Create user in database
                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create user during OAuth registration. Errors: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
                }

                _logger.LogDebug("User {UserId} successfully created for OAuth registration", user.Id);

                var roleResult = await _userManager.AddToRoleAsync(user, user.UserType.ToString());

                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Registration failed: Could not assign role {Role} to user {UserId}. Errors: {Errors}",
                        user.UserType.ToString(), user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    await _userManager.DeleteAsync(user);
                    return ApiResponse<AuthResponseDto>.Failure(ErrorCode.InvalidRequest);
                }

                // Add Google login
                var addLoginResult = await AddGoogleLoginAsync(user, googleUserInfo);
                if (!addLoginResult.IsSuccess)
                {
                    return addLoginResult;
                }

                // Generate and return tokens
                return await GenerateAuthTokenAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth registration for email {Email}", googleUserInfo.Email);
                return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
            }
        }

        private async Task<ApiResponse<AuthResponseDto>> AddGoogleLoginAsync(User user, GoogleUserInfo googleUserInfo)
        {
            try
            {
                _logger.LogDebug("Adding Google login for user {UserId}", user.Id);

                var logins = await _userManager.GetLoginsAsync(user);
                var googleLoginExists = logins.Any(l => l.LoginProvider == "Google");

                if (!googleLoginExists)
                {
                    var loginInfo = new UserLoginInfo("Google", googleUserInfo.GoogleId, "Google");
                    var result = await _userManager.AddLoginAsync(user, loginInfo);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to add Google login for user {UserId}. Errors: {Errors}",
                            user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                        return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
                    }

                    _logger.LogDebug("Google login successfully added for user {UserId}", user.Id);
                }
                else
                {
                    _logger.LogDebug("Google login already exists for user {UserId}", user.Id);
                }

                return ApiResponse<AuthResponseDto>.Success(null!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Google login for user {UserId}", user.Id);
                return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
            }
        }

        private async Task<ApiResponse<AuthResponseDto>> GenerateAuthTokenAsync(User user)
        {
            try
            {
                _logger.LogDebug("Generating tokens for user {UserId}", user.Id);

                var userRoles = await _userManager.GetRolesAsync(user);

                if (!userRoles.Any())
                {
                    _logger.LogError("User {UserId} has no assigned roles", user.Id);
                    return ApiResponse<AuthResponseDto>.Failure(ErrorCode.Forbidden);
                }

                var authResponse = _tokenService.GenerateToken(user, userRoles, null);

                _logger.LogInformation("Tokens successfully generated for user {UserId}", user.Id);
                return ApiResponse<AuthResponseDto>.Success(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {UserId}", user.Id);
                return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
            }
        }
    }
}
