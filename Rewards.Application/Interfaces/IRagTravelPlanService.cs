using Shared.Application.DTOs;

namespace Rewards.Application.Interfaces
{
    public interface IRagTravelPlanService
    {
        Task<ApiResponse<string>> GenerateTravelPlanAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
