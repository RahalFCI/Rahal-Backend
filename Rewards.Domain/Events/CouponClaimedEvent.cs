using Shared.Domain.Events;

namespace Rewards.Domain.Events
{
    public record CouponClaimedEvent(Guid ExplorerId, Guid CouponId, Guid UserCouponId) : BaseDomainEvent;
}
