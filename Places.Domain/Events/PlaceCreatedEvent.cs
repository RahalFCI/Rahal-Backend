using Shared.Domain.Events;

namespace Places.Domain.Events
{
    public record PlaceCreatedEvent(Guid PlaceId, string Name, Guid PlaceCategoryId) : BaseDomainEvent;
}
