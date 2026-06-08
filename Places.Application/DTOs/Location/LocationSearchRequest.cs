using Shared.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Application.DTOs.Location
{
    public class LocationSearchRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusInMeters { get; set; } = 5000;
        public Guid? CategoryId { get; set; }
        public required OffsetPaginationRequest offsetPaginationRequest { get; set; }
    }
}
