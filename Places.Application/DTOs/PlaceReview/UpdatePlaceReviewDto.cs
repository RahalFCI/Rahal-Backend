namespace Places.Application.DTOs.PlaceReview
{
    public class UpdatePlaceReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
