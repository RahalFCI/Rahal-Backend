namespace Places.Application.DTOs.CheckIn
{
    public class CreateCheckInDto
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
    }
}
