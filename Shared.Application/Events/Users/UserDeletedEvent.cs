using Shared.Domain.Events;

namespace Shared.Domain.Events.Users
{
    public record UserDeletedEvent(Guid UserId) : BaseDomainEvent;
}
