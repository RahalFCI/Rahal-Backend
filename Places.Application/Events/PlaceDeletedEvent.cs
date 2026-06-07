using Shared.Domain.Events;

namespace Places.Application.Events
{
    public record PlaceDeletedEvent(Guid PlaceId) : BaseDomainEvent;
}
