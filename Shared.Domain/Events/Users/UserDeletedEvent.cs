using Shared.Domain.Events;

namespace Users.Domain.Events
{
    public record UserDeletedEvent(Guid UserId) : BaseDomainEvent;
}
