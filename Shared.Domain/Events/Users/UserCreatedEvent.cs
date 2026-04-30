using Shared.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Domain.Events
{
    public record UserCreatedEvent(Guid UserId, string Name, string Email) : BaseDomainEvent
    {
    }
}
