using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Domain.Entities
{
    public class PlacePhoto
    {
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
