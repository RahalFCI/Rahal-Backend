using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            IRewardsRepository<Subscription> subscriptionRepository,
            IRewardsRepository<PlanTier> planTierRepository,
            IRewardsGamificationService gamificationService,
            ILogger<SubscriptionService> logger)
        {
            _subscriptionRepository = subscriptionRepository;
            _planTierRepository = planTierRepository;
            _gamificationService = gamificationService;
            _logger = logger;
        }

        public async Task<ApiResponse<GetSubscriptionDto>> PurchaseAsync(Guid explorerId, PurchaseSubscriptionDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Explorer {ExplorerId} is purchasing subscription plan tier {PlanTierId} using {PaymentMethod}", explorerId, dto.PlanTierId, dto.PaymentMethod);

            //Get Plan tier and validate
            var planTier = await _planTierRepository.GetTable()
                .FirstOrDefaultAsync(p => p.Id == dto.PlanTierId && p.IsActive, cancellationToken);
            if (planTier is null)
            {
                _logger.LogWarning("Active plan tier {PlanTierId} not found for explorer {ExplorerId}", dto.PlanTierId, explorerId);
                return ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.NotFound);
            }


            // Check if there's an active subscription
            var now = DateTime.UtcNow;
            var activeSubscription = await _subscriptionRepository.GetTable()
                .AnyAsync(s => s.ExplorerId == explorerId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > now,
                    cancellationToken);
            if (activeSubscription)
            {
                _logger.LogWarning("Explorer {ExplorerId} already has an active subscription", explorerId);
                return ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.Conflict);
            }

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

            _logger.LogInformation("Subscription {SubscriptionId} created as pending for explorer {ExplorerId}", subscription.Id, explorerId);

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
                    _logger.LogWarning("XP spend failed for subscription {SubscriptionId}. ErrorCode: {ErrorCode}", subscription.Id, spendResult.errorCode);

                    if (spendResult.errorCode is not ErrorCode.Timeout and not ErrorCode.ExternalServiceError)
                    {
                        subscription.Status = SubscriptionStatus.Cancelled;
                        subscription.CancelledAt = DateTime.UtcNow;
                        subscription.UpdatedAt = DateTime.UtcNow;
                        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

                        _logger.LogInformation("Subscription {SubscriptionId} cancelled after XP spend failure", subscription.Id);
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
                {
                    _logger.LogWarning("Setting premium failed for subscription {SubscriptionId}. ErrorCode: {ErrorCode}", subscription.Id, premiumResult.errorCode);
                    return ApiResponse<GetSubscriptionDto>.Failure(premiumResult.errorCode);
                }

                subscription.Status = SubscriptionStatus.Active;
                subscription.StartedAt = now;
                subscription.ExpiresAt = now.AddDays(7);
                subscription.UpdatedAt = DateTime.UtcNow;
                await _subscriptionRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Subscription {SubscriptionId} activated for explorer {ExplorerId}", subscription.Id, explorerId);
            }

            subscription.PlanTier = planTier;
            return ApiResponse<GetSubscriptionDto>.Success(RewardsMapper.ToDto(subscription));
        }

        public async Task<ApiResponse<GetSubscriptionDto>> GetActiveAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching active subscription for explorer {ExplorerId}", explorerId);

            var subscription = await _subscriptionRepository.GetTable()
                .AsNoTracking()
                .Include(s => s.PlanTier)
                .Where(s => s.ExplorerId == explorerId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription is null)
                _logger.LogWarning("Active subscription not found for explorer {ExplorerId}", explorerId);

            return subscription is null
                ? ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetSubscriptionDto>.Success(RewardsMapper.ToDto(subscription));
        }

        public async Task<ApiResponse<string>> CancelAsync(Guid explorerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cancelling active subscription for explorer {ExplorerId}", explorerId);

            var subscription = await _subscriptionRepository.GetTable()
                .FirstOrDefaultAsync(s => s.ExplorerId == explorerId && s.Status == SubscriptionStatus.Active, cancellationToken);
            if (subscription is null)
            {
                _logger.LogWarning("Active subscription not found for cancellation for explorer {ExplorerId}", explorerId);
                return ApiResponse<string>.Failure(ErrorCode.NotFound);
            }

            if (subscription.Status != SubscriptionStatus.Active && subscription.Status != SubscriptionStatus.Pending)
            {
                _logger.LogWarning("Subscription {SubscriptionId} cannot be cancelled from status {Status}", subscription.Id, subscription.Status);
                return ApiResponse<string>.Failure(ErrorCode.BusinessRuleViolation);
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _subscriptionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} cancelled for explorer {ExplorerId}", subscription.Id, explorerId);

            return ApiResponse<string>.Success("Subscription cancelled successfully");
        }

        public async Task<ApiResponse<GetSubscriptionDto>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching latest subscription for explorer {ExplorerId} - page {Page}, pageSize {PageSize}", explorerId, request.Page, request.PageSize);

            var subscription = await _subscriptionRepository.GetTable()
                .AsNoTracking()
                .Include(s => s.PlanTier)
                .Where(s => s.ExplorerId == explorerId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => RewardsMapper.ToDto(s))
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription is null)
            {
                _logger.LogWarning("Subscription not found for explorer {ExplorerId}", explorerId);
                return ApiResponse<GetSubscriptionDto>.Failure(ErrorCode.NotFound);
            }

            return ApiResponse<GetSubscriptionDto>.Success(subscription);
        }
    }
}
