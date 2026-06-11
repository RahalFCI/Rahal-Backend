using Rewards.Application.DTOs.Coupons;
using Rewards.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Application.Search;

namespace Rewards.Infrastructure.Search
{
    public class CouponSearchService : ICouponSearchService
    {
        private readonly ISearchService<CouponSearchDocument> _searchService;

        public CouponSearchService(ISearchService<CouponSearchDocument> searchService)
        {
            _searchService = searchService;
        }

        public Task IndexAsync(GetCouponDto coupon, CancellationToken cancellationToken = default)
        {
            return _searchService.IndexAsync(ToDocument(coupon), cancellationToken);
        }

        public Task DeleteAsync(Guid couponId, CancellationToken cancellationToken = default)
        {
            return _searchService.DeleteAsync(couponId.ToString(), cancellationToken);
        }

        public async Task<SearchResult<GetCouponDto>> SearchAsync(CouponSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var filters = new List<string>();
            if (request.VendorId.HasValue)
                filters.Add($"vendorId = \"{request.VendorId.Value}\"");
            if (!string.IsNullOrWhiteSpace(request.DiscountType))
                filters.Add($"discountType = \"{request.DiscountType}\"");
            if (request.MaxXpCost.HasValue)
                filters.Add($"xpCost <= {request.MaxXpCost.Value}");
            if (request.IsActive.HasValue)
                filters.Add($"isActive = {request.IsActive.Value.ToString().ToLowerInvariant()}");

            var result = await _searchService.SearchAsync(
                string.IsNullOrWhiteSpace(request.Query) ? " " : request.Query,
                new SearchOptions
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Filter = filters.Count == 0 ? null : string.Join(" AND ", filters)
                },
                cancellationToken);

            return new SearchResult<GetCouponDto>
            {
                Hits = result.Hits.Select(ToDto),
                TotalHits = result.TotalHits,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        private static CouponSearchDocument ToDocument(GetCouponDto coupon)
        {
            return new CouponSearchDocument
            {
                Id = coupon.Id.ToString(),
                VendorId = coupon.VendorId.ToString(),
                Title = coupon.Title,
                Description = coupon.Description,
                XpCost = coupon.XpCost,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MinimumCharge = coupon.MinimumCharge,
                ExpiresAt = coupon.ExpiresAt,
                IsActive = coupon.IsActive,
                RemainingClaims = coupon.RemainingClaims
            };
        }

        private static GetCouponDto ToDto(CouponSearchDocument document)
        {
            return new GetCouponDto
            {
                Id = Guid.Parse(document.Id),
                VendorId = Guid.Parse(document.VendorId),
                Title = document.Title,
                Description = document.Description,
                XpCost = document.XpCost,
                DiscountType = document.DiscountType,
                DiscountValue = document.DiscountValue,
                MinimumCharge = document.MinimumCharge,
                ExpiresAt = document.ExpiresAt,
                IsActive = document.IsActive,
                RemainingClaims = document.RemainingClaims
            };
        }
    }
}
