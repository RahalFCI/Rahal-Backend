namespace Places.Application.DTOs.Place
{
    public class CreatePlaceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid PlaceCategoryId { get; set; }
        public double TicketPrice { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeoFenceRange { get; set; }
    }
}
