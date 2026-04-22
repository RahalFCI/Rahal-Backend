namespace Places.Application.DTOs.PlacePhoto
{
    public class GetPlacePhotoDto
    {
        public Guid PlaceId { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
