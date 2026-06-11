using Rewards.Application.DTOs.Subscriptions;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Rewards.Application.Interfaces
{
    public interface ISubscriptionService
    {
        Task<ApiResponse<GetSubscriptionDto>> PurchaseAsync(Guid explorerId, PurchaseSubscriptionDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetSubscriptionDto>> GetActiveAsync(Guid explorerId, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> CancelAsync(Guid explorerId, Guid subscriptionId, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetSubscriptionDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
