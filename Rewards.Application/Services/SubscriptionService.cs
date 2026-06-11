using Microsoft.EntityFrameworkCore;
using Rewards.Application.DTOs.Subscriptions;
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
    internal class SubscriptionService : ISubscriptionService
    {
        private readonly IRewardsRepository<Subscription> _subscriptionRepository;
        private readonly IRewardsRepository<PlanTier> _planTierRepository;
        private readonly IRewardsGamificationService _gamificationService;

        public SubscriptionService(
            IRewardsRepository<Subscription> subscriptionRepository,
            IRewardsRepository<PlanTier> planTierRepository,
            IRewardsGamificationService gamificationService)
        {
            _subscriptionRepository = subscriptionRepository;
            _planTierRepository = planTierRepository;
            _gamificationService = gamificationService;
        }

        public async Task<ApiResponse<GetSubscriptionDto>> PurchaseAsync(Guid explorerId, PurchaseSubscriptionDto dto, CancellationToken cancellationToken = default)
        {
            //Get Plan tier and validate
            var planTier = await _planTierRepository.GetTable()
                .FirstOrDefaultAsync(p => p.Id == dto.PlanTierId && p.IsActive, cancellationToken);
            if (planTier is null)
                return ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.NotFound);


            // Check if there's an active subscription
            var now = DateTime.UtcNow;
            var activeSubscription = await _subscriptionRepository.GetTable()
                .AnyAsync(s => s.ExplorerId == explorerId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > now,
                    cancellationToken);
            if (activeSubscription)
                return ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.Conflict);

            // Create subscription with pending status
            var subscription = new Subscription
            {
                ExplorerId = explorerId,
                PlanTierId = planTier.Id,
                PaymentMethod = dto.PaymentMethod,
                Status = SubscriptionStatus.Pending
            };

            _subscriptionRepository.Add(subscription);
            await _subscriptionRepository.SaveChangesAsync(cancellationToken);

            if (dto.PaymentMethod == SubscriptionPaymentMethod.Xp)
            {
                var spendResult = await _gamificationService.SpendXpAsync(
                    subscription.Id,
                    explorerId,
                    planTier.WeeklyXpCost,
                    "SubscriptionPurchase",
                    subscription.Id,
                    cancellationToken);

                if (!spendResult.IsSuccess)
                {
                    if (spendResult.errorCode is not ErrorCode.Timeout and not ErrorCode.ExternalServiceError)
                    {
                        subscription.Status = SubscriptionStatus.Cancelled;
                        subscription.CancelledAt = DateTime.UtcNow;
                        subscription.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionRepository.SaveChangesAsync(cancellationToken);
                    }

                    return ApiResponse<GetSubscriptionDto>.Failure(spendResult.errorCode);
                }

                var premiumResult = await _gamificationService.SetPremiumAsync(
                    subscription.Id,
                    explorerId,
                    true,
                    planTier.Id,
                    cancellationToken);

                if (!premiumResult.IsSuccess)
                    return ApiResponse<GetSubscriptionDto>.Failure(premiumResult.errorCode);

                subscription.Status = SubscriptionStatus.Active;
                subscription.StartedAt = now;
                subscription.ExpiresAt = now.AddDays(7);
                subscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.SaveChangesAsync(cancellationToken);
            }

            subscription.PlanTier = planTier;
            return ApiResponse<GetSubscriptionDto>.Success(RewardsMapper.ToDto(subscription));
        }

        public async Task<ApiResponse<GetSubscriptionDto>> GetActiveAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            var subscription = await _subscriptionRepository.GetTable()
                .AsNoTracking()
                .Include(s => s.PlanTier)
                .Where(s => s.ExplorerId == explorerId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            return subscription is null
                ? ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetSubscriptionDto>.Success(RewardsMapper.ToDto(subscription));
        }

        public async Task<ApiResponse<string>> CancelAsync(Guid explorerId, Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            var subscription = await _subscriptionRepository.GetTable()
                .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.ExplorerId == explorerId, cancellationToken);
            if (subscription is null)
                return ApiResponse<string>.Failure(ErrorCode.NotFound);

            if (subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Pending)
                return ApiResponse<string>.Failure(ErrorCode.BusinessRuleViolation);

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.SaveChangesAsync(cancellationToken);

            var hasOtherActive = await _subscriptionRepository.GetTable()
                .AnyAsync(s => s.ExplorerId == explorerId
                    && s.Id != subscriptionId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (!hasOtherActive)
                await _gamificationService.SetPremiumAsync(subscription.Id, explorerId, false, null, cancellationToken);

            return ApiResponse<string>.Success("Subscription cancelled successfully");
        }

        public async Task<ApiResponse<PagedResult<GetSubscriptionDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _subscriptionRepository.GetTable()
                .AsNoTracking()
                .Include(s => s.PlanTier)
                .Where(s => s.ExplorerId == explorerId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => RewardsMapper.ToDto(s));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetSubscriptionDto>>.Success(result);
        }
    }
}
