using Places.Domain.Enums;

namespace Places.Application.DTOs.CheckIn
{
    public class GetCheckInDto
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
        public CheckInValidationStatus ValidationStatus { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string ValidationStatusName { get; set; } = string.Empty;
    }
}
