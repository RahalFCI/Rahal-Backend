using Shared.Domain.Entities;

namespace Places.Domain.Entities
{
    public class PlacePhoto : BaseEntity
    {
        public Guid PlaceId { get; set; }
        public Place? Place { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
