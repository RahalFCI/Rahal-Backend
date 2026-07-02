using Microsoft.AspNetCore.Http;
using Shared.Application.DTOs;

namespace Gamification.Application.Interfaces
{
    public interface ICheckInChallengeAiValidationService
    {
        Task<ApiResponse<bool>> ValidateCheckInChallengeAsync(
            IFormFile image,
            string description,
            CancellationToken cancellationToken = default);
    }
}
