using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Entities
{
    public abstract class BaseAuditableEntity<Tkey> : BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
