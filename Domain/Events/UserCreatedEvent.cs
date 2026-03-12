using Shared.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Domain.Events
{
    internal record UserCreatedEvent(int UserId, string Name, string Email) : BaseDomainEvent
    {
    }
}
