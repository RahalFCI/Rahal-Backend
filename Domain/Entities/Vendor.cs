using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Nodes;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{
    public class Vendor : User
    {
        public required string CountryCode { get; set; } //TODO: validate on the country code (RegionInfo
        public required string Address { get; set; }
        public required string AddressUrl{ get; set; }
        public required Dictionary<DayOfWeek, string> WorkingHours { get; set; } = new();
        public required int CategoryId { get; set; }
        public VendorCategory? Category { get; set; }
        public bool IsApproved { get; set; } = false;
    }
}
