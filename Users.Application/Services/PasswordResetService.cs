using Microsoft.AspNetCore.Identity;
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
    /// Manages OTP generation and password reset completion
    /// </summary>
    internal class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;
        private const int OTP_LENGTH = 6;
        private const int OTP_EXPIRY_MINUTES = 10;

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

                // Always return success even if user doesn't exist
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

                // Generate 6-digit OTP
                var otp = GenerateOtp();

                // Store OTP and expiry time in user
                user.PasswordResetOtp = otp;
                user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES);

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to store OTP for user {UserId}. Errors: {Errors}",
                        user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return;
                }

                _logger.LogInformation("Generated OTP for user {UserId}", user.Id);

                // Create and send email with OTP
                var mailRequest = MailTemplates.PasswordReset(email, user.DisplayName, otp);

                await _emailService.SendAsync(mailRequest, ct);

                _logger.LogInformation("Password reset OTP email sent successfully to {Email}", email);
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

                // Validate OTP
                if (string.IsNullOrWhiteSpace(user.PasswordResetOtp))
                {
                    _logger.LogWarning("Password reset failed: No OTP found for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                if (user.PasswordResetOtpExpiry is null || user.PasswordResetOtpExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("Password reset failed: OTP expired for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                if (!user.PasswordResetOtp.Equals(request.Otp, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Password reset failed: Invalid OTP provided for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Reset password
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    _logger.LogError("Failed to remove old password for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.UnknownError);
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (!addPasswordResult.Succeeded)
                {
                    _logger.LogWarning("Password reset failed for user {UserId}. Errors: {Errors}",
                        user.Id, string.Join(", ", addPasswordResult.Errors.Select(e => e.Description)));
                    return ApiResponse<string>.Failure(ErrorCode.UnknownError);
                }

                // Clear OTP after successful password reset
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;

                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);
                return ApiResponse<string>.Success("Password reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during password reset for email: {Email}", request.Email);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
        }

        /// <summary>
        /// Generates a random 6-digit OTP
        /// </summary>
        private string GenerateOtp()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            _logger.LogDebug("Generated OTP");
            return otp;
        }
    }
}
