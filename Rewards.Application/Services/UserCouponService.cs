using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rewards.Application.DTOs.UserCoupons;
using Rewards.Application.Interfaces;
using Rewards.Application.Mappers;
using Rewards.Domain.Entities;
using Rewards.Domain.Enums;
using Shared.Application.DTOs;
using Shared.Application.Pagination;
using Shared.Domain.Enums;
using Shared.Infrastructure.Pagination;

namespace Rewards.Application.Services
{
    internal class UserCouponService : IUserCouponService
    {
        private readonly IRewardsRepository<Coupon> _couponRepository;
        private readonly IRewardsRepository<UserCoupon> _userCouponRepository;
        private readonly IRewardsGamificationService _gamificationService;
        private readonly ILogger<UserCouponService> _logger;

        public UserCouponService(
            IRewardsRepository<Coupon> couponRepository,
            IRewardsRepository<UserCoupon> userCouponRepository,
            IRewardsGamificationService gamificationService,
            ILogger<UserCouponService> logger)
        {
            _couponRepository = couponRepository;
            _userCouponRepository = userCouponRepository;
            _gamificationService = gamificationService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetUserCouponDto>> ClaimAsync(Guid explorerId, Guid couponId, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepository.GetTable()
                .FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);

            if (coupon is null)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.NotFound);

            if (!coupon.IsActive || coupon.ExpiresAt <= DateTime.UtcNow || coupon.CurrentClaims >= coupon.MaxClaims)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.BusinessRuleViolation);

            var alreadyClaimed = await _userCouponRepository.GetTable()
                .AnyAsync(c => c.ExplorerId == explorerId && c.CouponId == couponId, cancellationToken);
            if (alreadyClaimed)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.AlreadyExists);

            var userCoupon = new UserCoupon
            {
                ExplorerId = explorerId,
                CouponId = coupon.Id,
                Code = GenerateCode(),
                Status = UserCouponStatus.Pending,
                IsRedeemed = false,
                ExpiresAt = coupon.ExpiresAt
            };

            coupon.CurrentClaims += 1;
            _userCouponRepository.Add(userCoupon);
            await _userCouponRepository.SaveChangesAsync(cancellationToken);

            var spendResult = await _gamificationService.SpendXpAsync(
                userCoupon.Id,
                explorerId,
                coupon.XpCost,
                "CouponPurchase",
                userCoupon.Id,
                cancellationToken);

            if (!spendResult.IsSuccess)
            {
                if (spendResult.errorCode is not ErrorCode.Timeout and not ErrorCode.ExternalServiceError)
                {
                    userCoupon.Status = UserCouponStatus.Cancelled;
                    userCoupon.UpdatedAt = DateTime.UtcNow;
                    coupon.CurrentClaims = Math.Max(0, coupon.CurrentClaims - 1);
                    await _userCouponRepository.SaveChangesAsync(cancellationToken);
                }

                return ApiResponse<GetUserCouponDto>.Failure(spendResult.errorCode);
            }

            userCoupon.Status = UserCouponStatus.Claimed;
            userCoupon.ClaimedAt = DateTime.UtcNow;
            userCoupon.UpdatedAt = DateTime.UtcNow;
            await _userCouponRepository.SaveChangesAsync(cancellationToken);

            userCoupon.Coupon = coupon;
            return ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
        }

        public async Task<ApiResponse<GetUserCouponDto>> RedeemAsync(RedeemCouponDto dto, CancellationToken cancellationToken = default)
        {
            var userCoupon = await _userCouponRepository.GetTable()
                .Include(c => c.Coupon)
                .FirstOrDefaultAsync(c => c.Code == dto.Code, cancellationToken);

            if (userCoupon is null || userCoupon.Coupon is null)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.NotFound);

            if (userCoupon.Coupon.VendorId != dto.VendorId)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.Forbidden);

            if (userCoupon.Status != UserCouponStatus.Claimed || userCoupon.IsRedeemed || userCoupon.ExpiresAt <= DateTime.UtcNow)
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.BusinessRuleViolation);

            userCoupon.IsRedeemed = true;
            userCoupon.Status = UserCouponStatus.Redeemed;
            userCoupon.RedeemedAt = DateTime.UtcNow;
            userCoupon.UpdatedAt = DateTime.UtcNow;
            await _userCouponRepository.SaveChangesAsync(cancellationToken);

            return ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
        }

        public async Task<ApiResponse<GetUserCouponDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            var userCoupon = await _userCouponRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Coupon)
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

            return userCoupon is null
                ? ApiResponse<GetUserCouponDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
        }

        public async Task<ApiResponse<PagedResult<GetUserCouponDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _userCouponRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Coupon)
                .Where(c => c.ExplorerId == explorerId)
                .OrderByDescending(c => c.ClaimedAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetUserCouponDto>>.Success(result);
        }

        private static string GenerateCode()
        {
            return $"CPN-{Guid.NewGuid():N}";
        }
    }
}
