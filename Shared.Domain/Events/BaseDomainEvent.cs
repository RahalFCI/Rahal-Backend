using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Events
{
    public abstract record BaseDomainEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
