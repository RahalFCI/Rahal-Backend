using Shared.Domain.Events;

namespace Shared.Domain.Events
{
    public record AdminProfileCreatedEvent(Guid UserId) : BaseDomainEvent;
}
