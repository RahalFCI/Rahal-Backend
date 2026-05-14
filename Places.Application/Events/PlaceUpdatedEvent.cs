using Shared.Domain.Events;

namespace Places.Application.Events
{
    public record PlaceUpdatedEvent(Guid PlaceId, string Name, Guid PlaceCategoryId) : BaseDomainEvent;
}
