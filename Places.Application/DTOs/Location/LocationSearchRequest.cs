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
    }
}
