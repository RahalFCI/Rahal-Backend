using Shared.Domain.Events;

namespace Places.Domain.Events
{
    public record PlaceDeletedEvent(Guid PlaceId) : BaseDomainEvent;
}
