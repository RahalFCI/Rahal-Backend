using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rewards.Application.DTOs.Coupons;
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
        private readonly IRewardsUnitOfWork _rewardsUnitOfWork;
        private readonly ILogger<UserCouponService> _logger;

        public UserCouponService(
            IRewardsRepository<Coupon> couponRepository,
            IRewardsRepository<UserCoupon> userCouponRepository,
            IRewardsGamificationService gamificationService,
            IRewardsUnitOfWork rewardsUnitOfWork,
            ILogger<UserCouponService> logger)
        {
            _couponRepository = couponRepository;
            _userCouponRepository = userCouponRepository;
            _gamificationService = gamificationService;
            _rewardsUnitOfWork = rewardsUnitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<GetUserCouponDto>> ClaimAsync(Guid explorerId, Guid couponId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _rewardsUnitOfWork.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Explorer {ExplorerId} is claiming coupon {CouponId}", explorerId, couponId);

                var alreadyClaimed = await _userCouponRepository.GetTable()
                    .AnyAsync(c => c.ExplorerId == explorerId && c.CouponId == couponId, cancellationToken);
                if (alreadyClaimed)
                {
                    _logger.LogWarning("Explorer {ExplorerId} already claimed coupon {CouponId}", explorerId, couponId);
                    await _rewardsUnitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.AlreadyExists);
                }

                var affectedRows = await _couponRepository.GetTable()
                    .Where(c =>
                        c.Id == couponId &&
                        c.IsActive &&
                        c.ExpiresAt > DateTime.UtcNow &&
                        c.CurrentClaims < c.MaxClaims)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(c => c.CurrentClaims, c => c.CurrentClaims + 1),
                        cancellationToken);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("Coupon {CouponId} is not available for claiming by explorer {ExplorerId}", couponId, explorerId);
                    await _rewardsUnitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.BusinessRuleViolation);
                }

                var coupon = await _couponRepository.GetTable().Where(c => c.Id == couponId).FirstOrDefaultAsync(cancellationToken);

                var userCoupon = new UserCoupon
                {
                    ExplorerId = explorerId,
                    CouponId = couponId,
                    Code = GenerateCode(),
                    Status = UserCouponStatus.Pending,
                    IsRedeemed = false,
                    ExpiresAt = coupon!.ExpiresAt
                };

                _userCouponRepository.Add(userCoupon);
                await _userCouponRepository.SaveChangesAsync(cancellationToken);
                await _rewardsUnitOfWork.CommitTransactionAsync(cancellationToken);


                _logger.LogInformation("User coupon {UserCouponId} created as pending for explorer {ExplorerId}", userCoupon.Id, explorerId);

                var spendResult = await _gamificationService.SpendXpAsync(
                    userCoupon.Id,
                    explorerId,
                    coupon.XpCost,
                    "CouponPurchase",
                    userCoupon.Id,
                    cancellationToken);

                await _rewardsUnitOfWork.BeginTransactionAsync(cancellationToken);

                if (!spendResult.IsSuccess)
                {
                    _logger.LogWarning("XP spend failed for user coupon {UserCouponId}. ErrorCode: {ErrorCode}", userCoupon.Id, spendResult.errorCode);

                    if (spendResult.errorCode is not ErrorCode.Timeout and not ErrorCode.ExternalServiceError)
                    {
                        userCoupon.Status = UserCouponStatus.Cancelled;
                        userCoupon.UpdatedAt = DateTime.UtcNow;
                        await _userCouponRepository.SaveChangesAsync(cancellationToken);              

                        _logger.LogInformation("User coupon {UserCouponId} cancelled and coupon {CouponId} claim count reverted", userCoupon.Id, couponId);
                    }

                    await _couponRepository.GetTable()
                            .Where(c => c.Id == couponId && c.CurrentClaims > 0)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(c => c.CurrentClaims, c => c.CurrentClaims - 1),
                                cancellationToken);

                    await _rewardsUnitOfWork.CommitTransactionAsync(cancellationToken);
                    return ApiResponse<GetUserCouponDto>.Failure(spendResult.errorCode);
                }



                userCoupon.Status = UserCouponStatus.Claimed;
                userCoupon.ClaimedAt = DateTime.UtcNow;
                userCoupon.UpdatedAt = DateTime.UtcNow;
                userCoupon.Coupon = coupon;
                _userCouponRepository.Update(userCoupon);
                await _userCouponRepository.SaveChangesAsync(cancellationToken);
                await _rewardsUnitOfWork.CommitTransactionAsync(cancellationToken);


                _logger.LogInformation("Explorer {ExplorerId} claimed coupon {CouponId} successfully with user coupon {UserCouponId}", explorerId, couponId, userCoupon.Id);


                return ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
            }
            catch (Exception)
            {
                await _rewardsUnitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.UnknownError);
            }
        }

        public async Task<ApiResponse<GetUserCouponDto>> RedeemAsync(RedeemCouponDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Redeeming user coupon for vendor {VendorId}", dto.VendorId);


            var userCoupon = await _userCouponRepository.GetTable()
                .Include(c => c.Coupon)
                .FirstOrDefaultAsync(c => c.Code == dto.Code && c.Coupon!.VendorId == dto.VendorId, cancellationToken);

            if (userCoupon is null || userCoupon.Coupon is null)
            {
                _logger.LogWarning("User coupon not found for redemption attempt by vendor {VendorId}", dto.VendorId);
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.NotFound);
            }

            if (userCoupon.Coupon.VendorId != dto.VendorId)
            {
                _logger.LogWarning("Vendor {VendorId} is forbidden from redeeming user coupon {UserCouponId} owned by vendor {CouponVendorId}", dto.VendorId, userCoupon.Id, userCoupon.Coupon.VendorId);
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.Forbidden);
            }

            if (userCoupon.Status != UserCouponStatus.Claimed || userCoupon.IsRedeemed || userCoupon.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "User coupon {UserCouponId} redemption rejected. Status {Status}, IsRedeemed {IsRedeemed}, ExpiresAt {ExpiresAt}",
                    userCoupon.Id,
                    userCoupon.Status,
                    userCoupon.IsRedeemed,
                    userCoupon.ExpiresAt);
                return ApiResponse<GetUserCouponDto>.Failure(ErrorCode.BusinessRuleViolation);
            }

            userCoupon.IsRedeemed = true;
            userCoupon.Status = UserCouponStatus.Redeemed;
            userCoupon.RedeemedAt = DateTime.UtcNow;
            userCoupon.UpdatedAt = DateTime.UtcNow;
            await _userCouponRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User coupon {UserCouponId} redeemed successfully for vendor {VendorId}", userCoupon.Id, dto.VendorId);

            return ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
        }

        public async Task<ApiResponse<GetUserCouponDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching user coupon by code");

            var userCoupon = await _userCouponRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Coupon)
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

            if (userCoupon is null)
                _logger.LogWarning("User coupon not found by code");

            return userCoupon is null
                ? ApiResponse<GetUserCouponDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetUserCouponDto>.Success(RewardsMapper.ToDto(userCoupon));
        }

        public async Task<ApiResponse<PagedResult<GetUserCouponDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching user coupons for explorer {ExplorerId} - page {Page}, pageSize {PageSize}", explorerId, request.Page, request.PageSize);

            var query = _userCouponRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Coupon)
                .Where(c => c.ExplorerId == explorerId)
                .OrderByDescending(c => c.ClaimedAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetUserCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<PagedResult<GetUserCouponDto>>> GetByCouponAsync(Guid couponId, Guid? vendorId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.Id == couponId)
                .Select(c => new { c.Id, c.VendorId })
                .FirstOrDefaultAsync(cancellationToken);

            if (coupon is null)
                return ApiResponse<PagedResult<GetUserCouponDto>>.Failure(ErrorCode.NotFound);

            if (vendorId.HasValue && coupon.VendorId != vendorId.Value)
                return ApiResponse<PagedResult<GetUserCouponDto>>.Failure(ErrorCode.Forbidden);

            var query = _userCouponRepository.GetTable()
                .AsNoTracking()
                .Include(c => c.Coupon)
                .Where(c => c.CouponId == couponId && (c.Status == UserCouponStatus.Redeemed || c.IsRedeemed))
                .OrderByDescending(c => c.RedeemedAt ?? c.ClaimedAt)
                .Select(c => RewardsMapper.ToDto(c));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetUserCouponDto>>.Success(result);
        }

        public async Task<ApiResponse<CouponStatsDto>> GetStatsByCouponAsync(Guid couponId, Guid? vendorId, CancellationToken cancellationToken = default)
        {
            var coupon = await _couponRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.Id == couponId)
                .Select(c => new { c.Id, c.VendorId })
                .FirstOrDefaultAsync(cancellationToken);

            if (coupon is null)
                return ApiResponse<CouponStatsDto>.Failure(ErrorCode.NotFound);

            if (vendorId.HasValue && coupon.VendorId != vendorId.Value)
                return ApiResponse<CouponStatsDto>.Failure(ErrorCode.Forbidden);

            var query = _userCouponRepository.GetTable()
                .AsNoTracking()
                .Where(c => c.CouponId == couponId);

            var totalClaims = await query.CountAsync(cancellationToken);
            var redeemedCount = await query.CountAsync(c => c.Status == UserCouponStatus.Redeemed || c.IsRedeemed, cancellationToken);
            var claimedCount = await query.CountAsync(c => c.Status == UserCouponStatus.Claimed, cancellationToken);
            var pendingCount = await query.CountAsync(c => c.Status == UserCouponStatus.Pending, cancellationToken);
            var expiredCount = await query.CountAsync(c => c.Status == UserCouponStatus.Expired, cancellationToken);
            var cancelledCount = await query.CountAsync(c => c.Status == UserCouponStatus.Cancelled, cancellationToken);
            var lastRedeemedAt = await query
                .Where(c => c.RedeemedAt.HasValue)
                .MaxAsync(c => c.RedeemedAt, cancellationToken);

            return ApiResponse<CouponStatsDto>.Success(new CouponStatsDto
            {
                CouponId = couponId,
                TotalClaims = totalClaims,
                RedeemedCount = redeemedCount,
                ClaimedCount = claimedCount,
                PendingCount = pendingCount,
                ExpiredCount = expiredCount,
                CancelledCount = cancelledCount,
                RedemptionRate = totalClaims == 0 ? 0 : redeemedCount / (double)totalClaims,
                LastRedeemedAt = lastRedeemedAt
            });
        }

        private static string GenerateCode()
        {
            return $"CPN-{Guid.NewGuid():N}";
        }
    }
}
