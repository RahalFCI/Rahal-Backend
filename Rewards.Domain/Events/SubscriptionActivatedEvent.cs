using Shared.Domain.Events;

namespace Rewards.Domain.Events
{
    public record SubscriptionActivatedEvent(Guid ExplorerId, Guid SubscriptionId, Guid PlanTierId) : BaseDomainEvent;
}
