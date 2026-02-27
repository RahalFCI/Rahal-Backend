using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Entities
{
    public abstract class BaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        public TKey Id { get; protected init; } = default!;
        public bool IsDeleted { get; set; } = false;
    }
}
