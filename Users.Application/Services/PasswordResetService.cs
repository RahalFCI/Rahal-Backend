using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Templates;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Application.DTOs.Auth;
using Users.Application.Interfaces;
using Users.Domain.Entities._Common;

namespace Users.Application.Services
{
    /// <summary>
    /// Service for handling password reset operations
    /// Manages forgot password token generation and password reset completion
    /// </summary>
    internal class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            UserManager<User> userManager,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Password reset requested for email: {Email}", email);

                var user = await _userManager.FindByEmailAsync(email);

                if (user is null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return;
                }

                if (user.IsDeleted)
                {
                    _logger.LogWarning("Password reset requested for deleted user: {UserId}", user.Id);
                    return;
                }

                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Encode token for URL safety
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

                // Build reset link - adjust based on your frontend URL
                var resetLink = $"https://rahal.com/reset-password?token={encodedToken}&email={Uri.EscapeDataString(email)}";

                _logger.LogInformation("Generated password reset token for user {UserId}", user.Id);

                // Create and send email
                var mailRequest = MailTemplates.PasswordReset(email, user.DisplayName, resetLink);

                await _emailService.SendAsync(mailRequest, ct);

                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password process for email: {Email}", email);
                throw;
            }
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Password reset initiated for email: {Email}", request.Email);

                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user is null)
                {
                    _logger.LogWarning("Password reset failed: User with email {Email} not found", request.Email);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                if (user.IsDeleted)
                {
                    _logger.LogWarning("Password reset failed: User {UserId} is deleted", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Decode token from URL-safe format
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                // Reset password
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);
                    return ApiResponse<string>.Success("Password reset successfully.");
                }

                _logger.LogWarning("Password reset failed for user {UserId}. Errors: {Errors}",
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
            catch (FormatException formatEx)
            {
                _logger.LogError(formatEx, "Invalid token format provided for password reset");
                return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during password reset for email: {Email}", request.Email);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
        }
    }
}
