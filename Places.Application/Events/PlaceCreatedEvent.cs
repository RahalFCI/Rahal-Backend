using Shared.Domain.Events;

namespace Places.Application.Events
{
    public record PlaceCreatedEvent(Guid PlaceId, string Name, Guid PlaceCategoryId) : BaseDomainEvent;
}
