using Shared.Domain.Events;

namespace Rewards.Domain.Events
{
    public record CouponRedeemedEvent(Guid ExplorerId, Guid CouponId, Guid UserCouponId, Guid VendorId) : BaseDomainEvent;
}
