using Shared.Domain.Events;

namespace Users.Domain.Events
{
    public record UserUpdatedEvent(Guid UserId) : BaseDomainEvent;
}
