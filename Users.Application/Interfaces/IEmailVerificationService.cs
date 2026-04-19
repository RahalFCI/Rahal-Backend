using Shared.Application.DTOs;
using Users.Application.DTOs.Auth;
using Users.Domain.Entities._Common;

namespace Users.Application.Interfaces
{

    public interface IEmailVerificationService
    {
        Task<ApiResponse<string>> SendVerificationOtpAsync(User user, CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> VerifyOtpAsync(string email, string otp, CancellationToken cancellationToken = default);

        Task<ApiResponse<string>> ResendOtpAsync(string email, CancellationToken cancellationToken = default);
    }
}
