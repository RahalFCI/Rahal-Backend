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
            _logger.LogInformation("Creating coupon {CouponTitle} for vendor {VendorId}", dto.Title, dto.VendorId);

            var exisitngCopoun = await _repository.GetTable()
                .AsNoTracking()
                .AnyAsync(c => c.Title == dto.Title && c.VendorId == dto.VendorId, cancellationToken);
            if (exisitngCopoun)
            {
                _logger.LogWarning("Coupon {CouponTitle} already exists for vendor {VendorId}", dto.Title, dto.VendorId);
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.AlreadyExists);
            }

            var coupon = RewardsMapper.ToEntity(dto);
            _repository.Add(coupon);
            await _repository.SaveChangesAsync(cancellationToken);

            var result = RewardsMapper.ToDto(coupon);
            await _searchService.IndexAsync(result, cancellationToken);

            _logger.LogInformation("Coupon {CouponId} created and indexed successfully", coupon.Id);

            return ApiResponse<GetCouponDto>.Success(result);
        }

        public async Task<ApiResponse<GetCouponDto>> UpdateAsync(Guid id, UpdateCouponDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating coupon {CouponId}", id);

            var coupon = await _repository.GetByIdAsync(id, cancellationToken);
            if (coupon is null)
            {
                _logger.LogWarning("Coupon {CouponId} not found", id);
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.NotFound);
            }

            if(coupon.Title != dto.Title)
            {
                _logger.LogWarning("Coupon {CouponId} title update rejected. Existing title {ExistingTitle}, requested title {RequestedTitle}", id, coupon.Title, dto.Title);
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.BusinessRuleViolation);
            }

            if (dto.MaxClaims < coupon.CurrentClaims)
            {
                _logger.LogWarning("Coupon {CouponId} max claims update rejected. Current claims {CurrentClaims}, requested max claims {RequestedMaxClaims}", id, coupon.CurrentClaims, dto.MaxClaims);
                return ApiResponse<GetCouponDto>.Failure(ErrorCode.BusinessRuleViolation);
            }

            RewardsMapper.Update(coupon, dto);
            await _repository.SaveChangesAsync(cancellationToken);

            var result = RewardsMapper.ToDto(coupon);
            await _searchService.IndexAsync(result, cancellationToken);

            _logger.LogInformation("Coupon {CouponId} updated and indexed successfully", id);

            return ApiResponse<GetCouponDto>.Success(result);
        }

        public async Task<ApiResponse<string>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting coupon {CouponId}", id);

            var coupon = await _repository.GetByIdAsync(id, cancellationToken);
            if (coupon is null)
            {
                _logger.LogWarning("Coupon {CouponId} not found", id);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            coupon.IsDeleted = true;
            coupon.DeletedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync(cancellationToken);
            await _searchService.DeleteAsync(id, cancellationToken);

            _logger.LogInformation("Coupon {CouponId} deleted and removed from search index", id);

            return ApiResponse<string>.Success("Coupon deleted successfully");
        }

        public async Task<ApiResponse<GetCouponDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching coupon {CouponId}", id);

            var coupon = await _repository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (coupon is null)
                _logger.LogWarning("Coupon {CouponId} not found", id);

            return coupon is null
                ? ApiResponse<GetCouponDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetCouponDto>.Success(RewardsMapper.ToDto(coupon));
        }

        public async Task<ApiResponse<PagedResult<GetCouponDto>>> GetByVendorIdAsync(Guid vendorId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching coupons for vendor {VendorId}", vendorId);

            var query = _repository.GetTable()
                .AsNoTracking()
                .Where(c => c.VendorId == vendorId)
                .OrderBy(c => c.ExpiresAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<PagedResult<GetCouponDto>>> GetAllAsync(OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching all coupons - page {Page}, pageSize {PageSize}", request.Page, request.PageSize);

            var query = _repository.GetTable()
                .AsNoTracking()
                .OrderBy(c => c.ExpiresAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<SearchResult<GetCouponDto>>> SearchAsync(CouponSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching coupons with query {Query}, page {Page}, pageSize {PageSize}", request.Query, request.Page, request.PageSize);

            var result = await _searchService.SearchAsync(request, cancellationToken);
            return ApiResponse<SearchResult<GetCouponDto>>.Success(result);
        }
    }
}
