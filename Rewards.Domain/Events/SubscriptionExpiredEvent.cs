using Shared.Domain.Events;

namespace Rewards.Domain.Events
{
    public record SubscriptionExpiredEvent(Guid ExplorerId, Guid SubscriptionId) : BaseDomainEvent;
}
