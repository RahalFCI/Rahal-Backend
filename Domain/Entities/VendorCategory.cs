using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Domain.Entities
{
    public class VendorCategory : BaseEntity
    {
        public required string CategoryName { get; set; }
    }
}
