using Shared.Application.DTOs;

namespace Rewards.Application.Interfaces
{
    public interface IRewardsGamificationService
    {
        Task<ApiResponse<string>> SpendXpAsync(Guid operationId, Guid explorerId, int amount, string sourceType, Guid referenceId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> SetPremiumAsync(Guid operationId, Guid explorerId, bool isPremium, Guid? planTierId, CancellationToken cancellationToken = default);
    }
}
