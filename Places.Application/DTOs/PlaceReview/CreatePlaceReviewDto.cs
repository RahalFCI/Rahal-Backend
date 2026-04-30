namespace Places.Application.DTOs.PlaceReview
{
    public class CreatePlaceReviewDto
    {
        public Guid ExplorerId { get; set; }
        public Guid PlaceId { get; set; }
        public Guid CheckInId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
