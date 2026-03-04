using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Users.Domain.Entities
{
    internal class VendorCategory : BaseEntity<int>
    {
        public required int CategoryName { get; set; }
    }
}
