using Rewards.Application.DTOs.Coupons;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Application.Search;

namespace Rewards.Application.Interfaces
{
    public interface ICouponService
    {
        Task<ApiResponse<GetCouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetCouponDto>> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<GetCouponDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiResponse<PagedResult<GetCouponDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default);
        Task<ApiResponse<SearchResult<GetCouponDto>>> SearchAsync(CouponSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
