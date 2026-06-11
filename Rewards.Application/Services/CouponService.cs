using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rewards.Application.DTOs.Coupons;
using Rewards.Application.Interfaces;
using Rewards.Application.Mappers;
using Rewards.Domain.Entities;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Application.Search;
using Shared.Domain.Enums;
using Shared.Infrastructure.Pagination;

namespace Rewards.Application.Services
{
    internal class CouponService : ICouponService
    {
        private readonly IRewardsRepository<Coupon> _repository;
        private readonly ICouponSearchService _searchService;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            IRewardsRepository<Coupon> repository,
            ICouponSearchService searchService,
            ILogger<CouponService> logger)
        {
            _repository = repository;
            _searchService = searchService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetCouponDto>> CreateAsync(CreateCouponDto dto, CancellationToken cancellationToken = default)
        {
            var exisitngCopoun = await _repository.GetTable()
                .AsNoTracking()
                .AnyAsync(c => c.Title == dto.Title && c.VendorId == dto.VendorId, cancellationToken);
            if (exisitngCopoun)
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.AlreadyExists);

            var coupon = RewardsMapper.ToEntity(dto);
            _repository.Add(coupon);
            await _repository.SaveChangesAsync(cancellationToken);

            var result = RewardsMapper.ToDto(coupon);
            await _searchService.IndexAsync(result, cancellationToken);
            return ApiResponse<GetCouponDto>.Success(result);
        }

        public async Task<ApiResponse<GetCouponDto>> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
        {
            var coupon = await _repository.GetByIdAsync(id, cancellationToken);
            if (coupon is null)
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.NotFound);

            if(coupon.Title != dto.Title)
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.BusinessRuleViolation);

            if (dto.MaxClaims < coupon.CurrentClaims)
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.BusinessRuleViolation);

            RewardsMapper.Update(coupon, dto);
            await _repository.SaveChangesAsync(cancellationToken);

            var result = RewardsMapper.ToDto(coupon);
            await _searchService.IndexAsync(result, cancellationToken);
            return ApiResponse<GetCouponDto>.Success(result);
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var coupon = await _repository.GetByIdAsync(id, cancellationToken);
            if (coupon is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            coupon.IsDeleted = true;
            coupon.DeletedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
            await _searchService.DeleteAsync(id, cancellationToken);
            return ApiResponse<string>.Success("Coupon deleted successfully");
        }

        public async Task<ApiResponse<GetCouponDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var coupon = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            return coupon is null
                ? ApiResponse<GetCouponDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetCouponDto>.Success(RewardsMapper.ToDto(coupon));
        }

        public async Task<ApiResponse<PagedResult<GetCouponDto>>> GetByVendorIdAsync(Guid vendorId, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetTable()
                .AsNoTracking()
                .Where(c => c.VendorId == vendorId)
                .OrderBy(c => c.ExpiresAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, new OffsetPaginationRequest(), cancellationToken);
            return ApiResponse<PagedResult<GetCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<PagedResult<GetCouponDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _repository.GetTable()
                .AsNoTracking()
                .OrderBy(c => c.ExpiresAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<SearchResult<GetCouponDto>>> SearchAsync(CouponSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _searchService.SearchAsync(request, cancellationToken);
            return ApiResponse<SearchResult<GetCouponDto>>.Success(result);
        }
    }
}
