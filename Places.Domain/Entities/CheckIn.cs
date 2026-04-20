using Places.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Places.Domain.Entities
{
    public class CheckIn
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public CheckInValidationStatus ValidationStatus { get; set; }
    }
}
