using Shared.Domain.Events;

namespace Shared.Application.Events.CheckIns
{
    public record CheckInInvalidatedEvent(Guid ExplorerId, Guid CheckInId, int XpReward) : BaseDomainEvent;
}
