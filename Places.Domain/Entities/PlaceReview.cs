using Shared.Domain.Entities;

namespace Places.Domain.Entities
{
    public class PlaceReview : BaseEntity
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public Guid CheckInId { get; set; }
        public CheckIn? CheckIn { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
    }
}
