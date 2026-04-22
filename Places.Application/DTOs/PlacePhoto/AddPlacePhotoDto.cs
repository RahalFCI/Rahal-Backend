namespace Places.Application.DTOs.PlacePhoto
{
    public class AddPlacePhotoDto
    {
        public Guid PlaceId { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
