using Shared.Application.DTOs;

namespace Rewards.Application.Interfaces
{
    public interface IRewardsPaymentService
    {
        Task<ApiResponse<string>> ProcessPaymentAsync(
            Guid operationId,
            Guid explorerId,
            decimal amount,
            string paymentMethod,
            Guid referenceId,
            CancellationToken cancellationToken = default);
    }
}
