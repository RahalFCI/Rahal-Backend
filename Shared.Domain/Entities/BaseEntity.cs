using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected init; } = default!;
        public bool IsDeleted { get; set; } = false;
    }
}
