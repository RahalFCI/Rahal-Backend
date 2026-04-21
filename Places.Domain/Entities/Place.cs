using Shared.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Domain.Entities
{
    public class Place : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid PlaceCategoryId { get; set; }
        public PlaceCategory? PlaceCategory { get; set; }
        public double TicketPrice { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Address? Address { get; set; }
        public int GeoFenceRange { get; set; }


    }
}
