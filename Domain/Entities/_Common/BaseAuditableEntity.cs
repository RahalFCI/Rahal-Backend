using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities._Common
{

    public abstract class BaseAuditableEntity
    {

        public Guid Id { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public DateTime? UpdatedAt { get; set; }


        public DateTime? DeletedAt { get; set; }


        public bool IsDeleted { get; set; } = false;
    }
}
