using Rewards.Application.DTOs.UserCoupons;
using Shared.Application.DTOs;
using Shared.Application.Pagination;

namespace Rewards.Application.Interfaces
{
    public interface IUserCouponService
    {
        Task<ApiResponse<GetUserCouponDto>> ClaimAsync(Guid explorerId, Guid couponId, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetUserCouponDto>> RedeemAsync(RedeemCouponDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetUserCouponDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetUserCouponDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default);
    }
}
