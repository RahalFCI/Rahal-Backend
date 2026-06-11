using Microsoft.EntityFrameworkCore;
using Rewards.Application.DTOs.TravelPlans;
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
    internal class TravelPlanService : ITravelPlanService
    {
        private readonly IRewardsRepository<TravelPlan> _travelPlanRepository;
        private readonly IRewardsRepository<Subscription> _subscriptionRepository;

        public TravelPlanService(
            IRewardsRepository<TravelPlan> travelPlanRepository,
            IRewardsRepository<Subscription> subscriptionRepository)
        {
            _travelPlanRepository = travelPlanRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<ApiResponse<GetTravelPlanDto>> CreateAsync(Guid explorerId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default)
        {
            // TODO: integrate RAG system. For now the generated JSON is provided by the caller.
            string testPlan = "{}";

            var activeSubscription = await _subscriptionRepository.GetTable()
                .Include(s => s.PlanTier)
                .Where(s => s.ExplorerId == explorerId
                    && s.Status == SubscriptionStatus.Active
                    && s.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeSubscription?.PlanTier is null)
                return ApiResponse<GetTravelPlanDto>.Failure(ErrorCode.InvalidRequest);

            var periodStart = activeSubscription.StartedAt ?? activeSubscription.CreatedAt;
            var periodEnd = activeSubscription.ExpiresAt ?? DateTime.UtcNow;

            var existingCount = await _travelPlanRepository.GetTable()
                .CountAsync(t => t.ExplorerId == explorerId
                    && t.SubscriptionId == activeSubscription.Id
                    && t.CreatedAt >= periodStart
                    && t.CreatedAt <= periodEnd,
                    cancellationToken);

            if (existingCount >= activeSubscription.PlanTier.MaxTravelPlans)
                return ApiResponse<GetTravelPlanDto>.Failure(ErrorCode.BusinessRuleViolation);

            var travelPlan = new TravelPlan
            {
                ExplorerId = explorerId,
                SubscriptionId = activeSubscription.Id,
                BudgetLimit = dto.BudgetLimit,
                StayDurationDays = dto.StayDurationDays,
                Prompt = dto.Prompt,
                GeneratedPlanJson = testPlan
            };

            _travelPlanRepository.Add(travelPlan);
            await _travelPlanRepository.SaveChangesAsync(cancellationToken);
            return ApiResponse<GetTravelPlanDto>.Success(RewardsMapper.ToDto(travelPlan));
        }

        public async Task<ApiResponse<GetTravelPlanDto>> GetByIdAsync(Guid explorerId, Guid id, CancellationToken cancellationToken = default)
        {
            var travelPlan = await _travelPlanRepository.GetTable()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.ExplorerId == explorerId, cancellationToken);

            return travelPlan is null
                ? ApiResponse<GetTravelPlanDto>.Failure(ErrorCode.NotFound)
                : ApiResponse<GetTravelPlanDto>.Success(RewardsMapper.ToDto(travelPlan));
        }

        public async Task<ApiResponse<PagedResult<GetTravelPlanDto>>> GetByExplorerAsync(Guid explorerId, OffsetPaginationRequest request, CancellationToken cancellationToken = default)
        {
            var query = _travelPlanRepository.GetTable()
                .AsNoTracking()
                .Where(t => t.ExplorerId == explorerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => RewardsMapper.ToDto(t));

            var result = await PaginationExtensions.ToPagedResultAsync(query, request, cancellationToken);
            return ApiResponse<PagedResult<GetTravelPlanDto>>.Success(result);
        }
    }
}
