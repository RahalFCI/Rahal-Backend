using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.Entities
{
    public abstract class BaseEntity
    {

        public Guid Id { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? UpdatedAt { get; set; }


        public DateTime? DeletedAt { get; set; }


        public bool IsDeleted { get; set; } = false;
    }
}
