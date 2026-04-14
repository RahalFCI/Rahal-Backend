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
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Application.Services
{
    /// <summary>
    /// Facade for orchestrating Google OAuth authentication flow
    /// Coordinates token validation, user existence check, and appropriate auth/registration flow
    /// Creates user + profile based on user type if user doesn't exist
    /// </summary>
    internal class GoogleOAuthFacade : IOAuthGoogleFacade
    {
        private readonly IGoogleTokenValidator _tokenValidator;
        private readonly IOAuthGoogleService _oauthService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<GoogleOAuthFacade> _logger;

        public GoogleOAuthFacade(
            IGoogleTokenValidator tokenValidator,
            IOAuthGoogleService oauthService,
            UserManager<User> userManager,
            ILogger<GoogleOAuthFacade> logger)
        {
            _tokenValidator = tokenValidator;
            _oauthService = oauthService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponseDto>> AuthenticateAsync(
            string idToken,
            UserRoleEnum userType,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Google OAuth authentication initiated for user type: {UserType}", userType);

                // Step 1: Validate Google token
                var validationResult = await _tokenValidator.ValidateAsync(idToken, ct);
                if (!validationResult.IsSuccess)
                {
                    _logger.LogWarning("Google OAuth authentication failed: Token validation failed");
                    return ApiResponse<AuthResponseDto>.Failure(ErrorCode.Unauthorized);
                }

                var googleUserInfo = validationResult.Data!;
                _logger.LogDebug("Google token validated for email: {Email}", googleUserInfo.Email);

                // Step 2: Check if user exists by email
                var existingUser = await _userManager.FindByEmailAsync(googleUserInfo.Email);

                if (existingUser is not null)
                {
                    _logger.LogInformation("User with email {Email} already exists. Proceeding with OAuth signin", googleUserInfo.Email);

                    // Step 3a: Sign in existing user
                    var signInResult = await _oauthService.SignInAsync(existingUser, googleUserInfo, ct);

                    if (!signInResult.IsSuccess)
                    {
                        _logger.LogError("Google OAuth authentication failed: OAuth signin failed for existing user {UserId}", existingUser.Id);
                        return signInResult;
                    }

                    _logger.LogInformation("Google OAuth signin successful for existing user {UserId}", existingUser.Id);
                    return signInResult;
                }

                _logger.LogInformation("No existing user found for email {Email}. Creating new user for type {UserType}", googleUserInfo.Email, userType);

                // Step 3b: Create new user with profile based on user type
                var newUser = CreateUserWithProfile(googleUserInfo, userType);

                if (newUser is null)
                {
                    _logger.LogError("Google OAuth authentication failed: Could not create user for type {UserType}", userType);
                    return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
                }

                _logger.LogDebug("User created for type {UserType}. Registering via OAuth", userType);

                // Step 4: Register new user
                var registerResult = await _oauthService.RegisterAsync(newUser, googleUserInfo, ct);

                if (!registerResult.IsSuccess)
                {
                    _logger.LogError("Google OAuth authentication failed: OAuth registration failed for email {Email}", googleUserInfo.Email);
                    return registerResult;
                }

                _logger.LogInformation("Google OAuth registration successful for new user {UserId}", newUser.Id);
                return registerResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during Google OAuth authentication");
                return ApiResponse<AuthResponseDto>.Failure(ErrorCode.UnknownError);
            }
        }

        /// <summary>
        /// Creates a new user with appropriate profile based on user type
        /// </summary>
        private User? CreateUserWithProfile(GoogleUserInfo googleUserInfo, UserRoleEnum userType)
        {
            try
            {
                return userType switch
                {
                    UserRoleEnum.Explorer => CreateExplorerUser(googleUserInfo),
                    UserRoleEnum.Vendor => CreateVendorUser(googleUserInfo),
                    UserRoleEnum.Admin => CreateAdminUser(googleUserInfo),
                    _ => throw new InvalidOperationException($"Invalid user type: {userType}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user for type {UserType}", userType);
                return null;
            }
        }

        /// <summary>
        /// Creates an Explorer user with profile from Google claims
        /// </summary>
        private User CreateExplorerUser(GoogleUserInfo googleUserInfo)
        {
            _logger.LogDebug("Creating Explorer user from Google claims for email: {Email}", googleUserInfo.Email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = googleUserInfo.Email,
                UserName = googleUserInfo.Email,
                DisplayName = $"{googleUserInfo.FirstName} {googleUserInfo.LastName}".Trim(),
                UserType = UserRoleEnum.Explorer,
                ProfilePictureURL = googleUserInfo.PictureUrl ?? string.Empty,
                EmailConfirmed = true, // Auto-confirm for OAuth users
            };

            user.ExplorerProfile = new ExplorerProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                Bio = string.Empty,
                CountryCode = "US", // Default country
                Gender = GenderEnum.Male, // Default gender
                BirthDate = new DateOnly(2000, 1, 1), // Default birthdate
                IsPublic = false,
                CreatedAt = DateTime.UtcNow
            };

            return user;
        }

        /// <summary>
        /// Creates a Vendor user with profile from Google claims
        /// </summary>
        private User CreateVendorUser(GoogleUserInfo googleUserInfo)
        {
            _logger.LogDebug("Creating Vendor user from Google claims for email: {Email}", googleUserInfo.Email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = googleUserInfo.Email,
                UserName = googleUserInfo.Email,
                DisplayName = $"{googleUserInfo.FirstName} {googleUserInfo.LastName}".Trim(),
                UserType = UserRoleEnum.Vendor,
                ProfilePictureURL = googleUserInfo.PictureUrl ?? string.Empty,
                EmailConfirmed = true, // Auto-confirm for OAuth users
            };

            user.VendorProfile = new VendorProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                CountryCode = "US", // Default country
                Address = string.Empty,
                AddressUrl = string.Empty,
                WorkingHours = new Dictionary<DayOfWeek, string>(), // Empty working hours
                CategoryId = 1, // Default category
                CreatedAt = DateTime.UtcNow
            };

            return user;
        }

        /// <summary>
        /// Creates an Admin user with profile from Google claims
        /// </summary>
        private User CreateAdminUser(GoogleUserInfo googleUserInfo)
        {
            _logger.LogDebug("Creating Admin user from Google claims for email: {Email}", googleUserInfo.Email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = googleUserInfo.Email,
                UserName = googleUserInfo.Email,
                DisplayName = $"{googleUserInfo.FirstName} {googleUserInfo.LastName}".Trim(),
                UserType = UserRoleEnum.Admin,
                ProfilePictureURL = googleUserInfo.PictureUrl ?? string.Empty,
                EmailConfirmed = true, // Auto-confirm for OAuth users
            };

            user.AdminProfile = new AdminProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            return user;
        }
    }
}
