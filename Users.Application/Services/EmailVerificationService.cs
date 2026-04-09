using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using Shared.Application.DTOs;
using Shared.Application.Interfaces;
using Shared.Application.Templates;
using Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Users.Application.Interfaces;
using Users.Domain.Entities;
using Users.Domain.Entities._Common;
using Users.Domain.Events;

namespace Users.Application.Services
{

    internal class EmailVerificationService : IEmailVerificationService
    {
        private readonly IEmailVerificationRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly IPublisher _publisher;
        private readonly ILogger<EmailVerificationService> _logger;

        private const int OTP_EXPIRY_MINUTES = 10;
        private const int RESEND_COOLDOWN_SECONDS = 300; // 5 minutes

        public EmailVerificationService(
            IEmailVerificationRepository repository,
            UserManager<User> userManager,
            IEmailService emailService,
            IPublisher publisher,
            ILogger<EmailVerificationService> logger)
        {
            _repository = repository;
            _userManager = userManager;
            _emailService = emailService;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> SendVerificationOtpAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending verification OTP for user {UserId} with email {Email}", user.Id, user.Email);

                // Check if user already has an active token
                var existingToken = await _repository.GetByExpression(
                    t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

                // Delete old token if exists to avoid multiple active tokens
                if (existingToken != null)
                {
                    _logger.LogInformation("Removing existing active token for user {UserId}", user.Id);
                    _repository.Delete(existingToken);
                }

                // Generate OTP
                var otp = GenerateOtp();
                var codeHash = HashOtp(otp);

                // Create verification token
                var verificationToken = new EmailVerificationToken
                {
                    UserId = user.Id,
                    User = user,
                    CodeHash = codeHash,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(OTP_EXPIRY_MINUTES),
                    IsUsed = false,
                    AttemptCount = 0
                };

                _repository.Add(verificationToken);

                _logger.LogInformation("Created verification token for user {UserId}", user.Id);

                // Save changes to database
                await _repository.SaveChangesAsync(cancellationToken);

                // Send email with OTP
                var mailRequest = MailTemplates.VerificationOtp(user.Email!, user.DisplayName, otp);
                await _emailService.SendAsync(mailRequest, cancellationToken);

                _logger.LogInformation("Verification OTP email sent successfully to {Email}", user.Email);

                return ApiResponse<string>.Success("OTP sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending verification OTP for user {UserId}", user.Id);
                return ApiResponse<string>.Failure(ErrorCode.ExternalServiceError);
            }
        }

        public async Task<ApiResponse<string>> VerifyOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("OTP verification attempt for email {Email}", email);

                // Validate input
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                {
                    _logger.LogWarning("OTP verification failed: Invalid input for email {Email}", email);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("OTP verification failed: User with email {Email} not found", email);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                // Find active verification token
                var verificationToken = await _repository.GetByExpression(
                    t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

                if (verificationToken == null)
                {
                    _logger.LogWarning("OTP verification failed: No active token for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Check attempt limit
                if (verificationToken.AttemptCount >= EmailVerificationToken.MaxAttempts)
                {
                    _logger.LogWarning("OTP verification failed: Max attempts exceeded for user {UserId}", user.Id);
                    _repository.Delete(verificationToken);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Verify OTP
                var otpHash = HashOtp(otp);
                if (verificationToken.CodeHash != otpHash)
                {
                    verificationToken.AttemptCount++;
                    _repository.Update(verificationToken);
                    await _repository.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("OTP verification failed: Invalid OTP for user {UserId}. Attempts: {Attempts}", 
                        user.Id, verificationToken.AttemptCount);

                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // OTP is valid, mark as used and update user
                verificationToken.IsUsed = true;
                _repository.Update(verificationToken);

                user.EmailConfirmed = true;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to mark email as confirmed for user {UserId}. Errors: {Errors}",
                        user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                    return ApiResponse<string>.Failure(ErrorCode.UnknownError);
                }

                // Save repository changes
                await _repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Email successfully verified for user {UserId}", user.Id);

                // Publish UserCreatedEvent to trigger welcome email and other handlers
                var userCreatedEvent = new UserCreatedEvent(
                    user.Id,
                    user.DisplayName,
                    user.Email!
                );

                await _publisher.Publish(userCreatedEvent, cancellationToken);
                _logger.LogInformation("UserCreatedEvent published for user {UserId}", user.Id);

                return ApiResponse<string>.Success("Email verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OTP verification for email {Email}", email);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
        }

        public async Task<ApiResponse<string>> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("OTP resend request for email {Email}", email);

                // Validate input
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("OTP resend failed: Invalid email");
                    return ApiResponse<string>.Failure(ErrorCode.InvalidRequest);
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("OTP resend failed: User with email {Email} not found", email);
                    return ApiResponse<string>.Failure(ErrorCode.NotFound);
                }

                // Check if email is already verified
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("OTP resend failed: Email already verified for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
                }

                // Check recent resend attempts
                var recentToken = await _repository.GetByExpression(
                    t => t.UserId == user.Id && 
                         !t.IsUsed && 
                         t.CreatedAt > DateTime.UtcNow.AddSeconds(-RESEND_COOLDOWN_SECONDS),
                    cancellationToken);

                if (recentToken != null)
                {
                    _logger.LogWarning("OTP resend failed: Resend cooldown not expired for user {UserId}", user.Id);
                    return ApiResponse<string>.Failure(ErrorCode.InvalidOperation);
                }

                // Send new OTP
                return await SendVerificationOtpAsync(user, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OTP resend for email {Email}", email);
                return ApiResponse<string>.Failure(ErrorCode.UnknownError);
            }
        }

        private static string GenerateOtp()
        {

            byte[] data = RandomNumberGenerator.GetBytes(32);
            int randomNumber = BitConverter.ToInt32(data, 0);
            int otp = Math.Abs(randomNumber % 1000000); // Get last 6 digits
            return otp.ToString("D6"); // Ensure 6 digits with leading zeros if needed
        }

        private static string HashOtp(string otp)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
