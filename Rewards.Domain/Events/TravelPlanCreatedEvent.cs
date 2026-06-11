using Shared.Domain.Events;

namespace Rewards.Domain.Events
{
    public record TravelPlanCreatedEvent(Guid ExplorerId, Guid TravelPlanId, Guid SubscriptionId) : BaseDomainEvent;
}
