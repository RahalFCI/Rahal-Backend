using Places.Domain.Enums;
using Shared.Domain.Entities;

namespace Places.Domain.Entities
{
    public class CheckIn : BaseEntity
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public CheckInValidationStatus ValidationStatus { get; set; }
    }
}
