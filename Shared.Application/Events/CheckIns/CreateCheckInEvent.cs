using Shared.Domain.Events;

namespace Shared.Application.Events.CheckIns
{
    public record CreateCheckInEvent(Guid ExplorerId, Guid CheckInId) : BaseDomainEvent;
}
