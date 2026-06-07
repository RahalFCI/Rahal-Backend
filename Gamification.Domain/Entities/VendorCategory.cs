using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gamification.Domain.Entities
{
    public class VendorCategory : BaseEntity
    {
        public required string CategoryName { get; set; }

        public IEnumerable<VendorProfile> VendorProfiles { get; set; } = new List<VendorProfile>();
    }
}
