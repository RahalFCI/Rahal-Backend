using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Application.DTOs;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.OAuth;
using Users.Application.Interfaces;
using Users.Application.Settings;

namespace Users.Application.Services
{
    /// <summary>
    /// Service for validating Google OAuth tokens and extracting user information
    /// </summary>
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly GoogleAuthSettings _settings;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(
            IOptions<GoogleAuthSettings> settings,
            ILogger<GoogleTokenValidator> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<GoogleUserInfo>> ValidateAsync(
            string idToken, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idToken))
                {
                    _logger.LogWarning("Google token validation failed: Token is empty or null");
                    return ApiResponse<GoogleUserInfo>.Failure(ErrorCode.InvalidRequest);
                }

                _logger.LogInformation("Validating Google token");

                GoogleJsonWebSignature.Payload payload;

                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                }
                catch (InvalidJwtException ex) when (ex.Message.Contains("expired",
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Google token validation failed: Token has expired");
                    return ApiResponse<GoogleUserInfo>.Failure(ErrorCode.InvalidRequest);
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogWarning(ex, "Google token validation failed: Invalid token format");
                    return ApiResponse<GoogleUserInfo>.Failure(ErrorCode.InvalidRequest);
                }

                if (string.IsNullOrWhiteSpace(payload.Email))
                {
                    _logger.LogWarning("Google token validation failed: Email claim missing");
                    return ApiResponse<GoogleUserInfo>.Failure(ErrorCode.InvalidRequest);
                }

                var userInfo = new GoogleUserInfo(
                    Email: payload.Email,
                    FirstName: payload.GivenName ?? string.Empty,
                    LastName: payload.FamilyName ?? string.Empty,
                    PictureUrl: payload.Picture,
                    GoogleId: payload.Subject);

                _logger.LogInformation("Google token validated successfully for email: {Email}", payload.Email);
                return ApiResponse<GoogleUserInfo>.Success(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during Google token validation");
                return ApiResponse<GoogleUserInfo>.Failure(ErrorCode.UnknownError);
            }
        }
    }
}
