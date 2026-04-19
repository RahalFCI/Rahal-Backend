using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Users.Domain.Entities._Common;

namespace Users.Domain.Entities
{

    public class VendorProfile : BaseEntity
    {

        public required Guid UserId { get; set; }


        public required User User { get; set; }


        public required string CountryCode { get; set; }


        public required string Address { get; set; }

        public required string AddressUrl { get; set; }


        public required Dictionary<DayOfWeek, string> WorkingHours { get; set; }


        public required int CategoryId { get; set; }


        public VendorCategory? Category { get; set; }


        public bool IsApproved { get; set; } = false;
    }
}
