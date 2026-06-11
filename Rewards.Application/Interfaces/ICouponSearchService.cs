using Rewards.Application.DTOs.Coupons;
using Shared.Application.Search;

namespace Rewards.Application.Interfaces
{
    public interface ICouponSearchService
    {
        Task IndexAsync(GetCouponDto coupon, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid couponId, CancellationToken cancellationToken = default);
        Task<SearchResult<GetCouponDto>> SearchAsync(CouponSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
