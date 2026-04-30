using Shared.Domain.Events;

namespace Places.Domain.Events
{
    public record PlaceUpdatedEvent(Guid PlaceId, string Name, Guid PlaceCategoryId) : BaseDomainEvent;
}
